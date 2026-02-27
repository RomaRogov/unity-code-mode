# Converting Unity CustomEditor to AiAgentEditor

## Purpose

This guide explains how to convert a Unity internal `CustomEditor` into an `AiAgentEditor` — a headless, API-driven inspector that an AI agent can use to read, write, and understand properties of Unity objects via JSON.

An `AiAgentEditor` does **not** draw any UI. It provides three capabilities:
1. **Dump** — serialize the object's current state to a `JObject`
2. **Definition** — emit a TypeScript-like class definition describing the object's schema
3. **Set Property** — modify a specific property by path, given a `JToken` value (with automatic get→merge→set for partial patches)

---

## Design Philosophy: LLM-Friendly Over Mechanical

The goal is **not** to expose Unity's internal serialization tree as-is. Instead, think of the AI agent as a junior developer who needs to understand what each property *means* and how to use it.

**Key principles:**

1. **Semantic names over serialized paths** — expose `fieldOfView`, not `field of view` or `m_FieldOfView`. The AI should be able to reason about properties by name alone.
2. **Composite properties over raw fields** — if several serialized fields work together as one concept, expose them as a single structured object. Example: RectTransform's `horizontal` bundles anchor, offset, size, and mode into one readable JSON, rather than scattering `m_AnchoredPosition.x`, `m_AnchorMin.x`, `m_SizeDelta.x` across unrelated paths.
3. **Enums as strings, not ints** — dump `"Skybox"`, not `1`. The AI can understand `clearFlags: "Skybox"` immediately; `clearFlags: 1` requires looking up a table.
4. **Conditional output over exhaustive output** — only dump properties that are relevant to the current state. Example: Camera should dump `orthographicSize` when orthographic, `fieldOfView` when perspective. Don't dump both — that confuses the AI about which one matters.
5. **Meaningful comments in definitions** — a `WithComment("Half-size of the camera in orthographic mode")` saves the AI from guessing. Think of the definition as documentation the AI reads before acting.
6. **Partial patch support** — the AI should be able to set `horizontal.offset` without knowing or specifying every other field of `horizontal`. The get→merge→set system handles this automatically.

---

## Architecture Overview

```
AiEditorBase (abstract)
├── AiAgentEditor (partial, for Unity Object instances)
│   ├── TransformAgentEditor
│   ├── CameraAgentEditor
│   ├── MaterialAgentEditor
│   ├── TextureImporterAgentEditor
│   └── ... (one per inspected type)
└── AiSettingsEditor (for project settings like Physics, Lighting)
    ├── PhysicsSettingsEditor
    └── ...
```

### Inheritance

- `AiEditorBase` — shared infrastructure: dump helpers, definition helpers, property handler registration, `FindHandlerForPath`, `ApplyPatch`
- `AiAgentEditor` — adds `target` (the Unity Object), `serializedObject` (cached SerializedObject), and factory via `[CustomAiAgentEditor]`
- Your custom editor extends `AiAgentEditor` and overrides `OnEnable()`, `OnDumpRequested()`, `OnDefinitionRequested()`

### Key Capability: Built-in SerializedObject

The base `AiAgentEditor` automatically initializes a `SerializedObject` for you.
- For Components (e.g. `Transform`), `serializedObject` targets the component.
- For Assets (e.g. `Texture2D`), `serializedObject` targets the **Importer** (e.g. `TextureImporter`) automatically.

**Best Practice:** Always use the `serializedObject` property for reading and writing. This ensures Unity's Undo/Redo system works correctly without manual undo logic.

---

## When to Create a Custom AiAgentEditor

Create one when the default `SerializedObject`-based output is insufficient:

1. **Types needing curated output** (GameObject, Transform) — the raw serialized data is noisy; you want clean, meaningful property names.
2. **Types with complex conversion** (Transform Rotation) — serialized as Quaternion, but must be exposed as Euler angles so the AI can reason about "rotate 45 degrees on Y".
3. **Types with composite properties** (RectTransform) — multiple serialized fields represent a single concept (layout axis) that should be presented as one structured object.
4. **Types with conditional sections** (Camera, TextureImporter) — certain properties only make sense in specific modes; you need control flow in dump/definition.
5. **Types with non-standard serialization** (Material shader properties) — properties accessed via API, not serialized fields.

---

## The 3-Arg Handler Pattern

Every property handler **must** have both a getter and a setter:

```csharp
AddSettingPropertyHandler("propertyName",
    () => /* get: return current value as JToken */,
    v => /* set: apply JToken value */);
```

### Why getters are required

The `SetProperty` flow is: **get current → merge patch → set result**.

This enables:
- **Partial patches** — AI sets `horizontal.offset` and the system reads the full `horizontal` object, patches just `offset`, and writes the whole thing back.
- **Deep merges** — AI sends `{ "r": 1.0 }` to a Color property and the system merges it into the existing `{ "r": 0.5, "g": 0.5, "b": 0.5, "a": 1.0 }`.
- **Mode switching** — AI sets `horizontal` with `{ "mode": "Stretch", "from": 0, "to": 1 }` and the setter receives the complete object to handle mode conversion.

### How SetProperty resolves handlers

1. **Exact match** — `"fieldOfView"` finds the `"fieldOfView"` handler directly.
2. **Prefix match** — `"horizontal.offset"` finds the `"horizontal"` handler. The system calls the getter, patches `"offset"` in the result, and passes the merged object to the setter.
3. **Fallback** — if no handler matches, the base class attempts direct SerializedProperty resolution by path.

---

## Step-by-Step Conversion Process

### Step 1: Identify the Source Material

Find the Unity source for the inspector you're converting. Key sources:
- `Editor/Mono/Inspector/` — inspectors for Components (CameraEditor, LightEditor, etc.)
- `Editor/Mono/ImportSettings/` — importer inspectors (TextureImporterInspector, etc.)

From the source, identify:
- **Which serialized property paths** are cached in `OnEnable()` or `CacheSerializedProperties()`
- **Which properties work together** as a single concept (candidates for composite properties)
- **Which enums** are used for dropdowns
- **Which properties are conditional** on other property values

### Step 2: Design the AI-Facing API

Before writing code, decide the property names and structure the AI will see. Think about what would make sense to an LLM reading the dump:

| Unity Internal | AI-Facing | Why |
|---|---|---|
| `m_LocalPosition` (Vector3) | `localPosition` (Vector3) | Clean name, matches C# API |
| `m_LocalRotation` (Quaternion) | `localRotation` (Vector3, Euler) | AI can't reason about quaternions |
| `m_ClearFlags` (int) | `clearFlags` ("Skybox") | String enum is self-documenting |
| `m_AnchorMin.x` + `m_AnchorMax.x` + `m_AnchoredPosition.x` + `m_SizeDelta.x` | `horizontal` (composite object) | One concept = one property |
| `orthographic size` (float, only in ortho mode) | `orthographicSize` (conditional dump) | Don't confuse AI with irrelevant fields |

### Step 3: Implement OnEnable (Cache Properties + Register Handlers)

Cache `SerializedProperty` objects and register 3-arg handlers.

```csharp
private SerializedProperty m_LocalPosition;
private SerializedProperty m_LocalRotation;

protected override void OnEnable()
{
    m_LocalPosition = serializedObject.FindProperty("m_LocalPosition");
    m_LocalRotation = serializedObject.FindProperty("m_LocalRotation");

    // Simple property: get and set are symmetric
    AddSettingPropertyHandler("localPosition",
        () => m_LocalPosition.vector3Value.SerializeToJObject(),
        v => m_LocalPosition.vector3Value = v.DeserializeToVector3());

    // Converted property: Quaternion stored, Euler exposed
    AddSettingPropertyHandler("localRotation",
        () => m_LocalRotation.quaternionValue.eulerAngles.SerializeToJObject(),
        v => m_LocalRotation.quaternionValue = Quaternion.Euler(v.DeserializeToVector3()));
}
```

The base `AiAgentEditor` handles `serializedObject.Update()` before and `serializedObject.ApplyModifiedProperties()` after your handler. You do NOT need to call them.

### Step 4: Implement OnDumpRequested()

Use cached properties to build the dump. Apply conditional logic for context-dependent properties.

```csharp
protected override void OnDumpRequested()
{
    // Always dump these
    DumpProperty("orthographic", m_Orthographic.boolValue);

    // Conditional: only dump the relevant size property
    if (m_Orthographic.boolValue)
        DumpProperty("orthographicSize", m_OrthographicSize.floatValue);
    else
        DumpProperty("fieldOfView", m_FieldOfView.floatValue);
}
```

### Step 5: Implement OnDefinitionRequested()

Emit the TypeScript definition. Use comments and decorators to help the AI understand constraints.

```csharp
protected override void OnDefinitionRequested()
{
    GenerateEnumDefinition(typeof(CameraClearFlags));

    EmitClassDefinition("Camera", new List<TsPropertyDef>
    {
        TsPropertyDef.Field("clearFlags", "CameraClearFlags"),
        TsPropertyDef.Field("orthographic", "boolean"),
        TsPropertyDef.Field("orthographicSize", "number")
            .WithComment("Half-size of the camera in orthographic mode"),
        TsPropertyDef.Field("fieldOfView", "number")
            .WithDecorator("type: Float, min: 0.00001, max: 179")
            .WithComment("Vertical field of view in degrees (perspective mode)"),
    });
}
```

---

## Complete Example: Simple Editor (Transform)

Standard pattern: cached properties, symmetric get/set handlers, Quaternion-to-Euler conversion.

```csharp
[CustomAiAgentEditor(typeof(Transform))]
public class TransformAgentEditor : AiAgentEditor
{
    private SerializedProperty m_LocalPosition;
    private SerializedProperty m_LocalRotation;
    private SerializedProperty m_LocalScale;

    protected override void OnEnable()
    {
        m_LocalPosition = serializedObject.FindProperty("m_LocalPosition");
        m_LocalRotation = serializedObject.FindProperty("m_LocalRotation");
        m_LocalScale = serializedObject.FindProperty("m_LocalScale");

        AddSettingPropertyHandler("localPosition",
            () => m_LocalPosition.vector3Value.SerializeToJObject(),
            v => m_LocalPosition.vector3Value = v.DeserializeToVector3());

        AddSettingPropertyHandler("localRotation",
            () => m_LocalRotation.quaternionValue.eulerAngles.SerializeToJObject(),
            v => m_LocalRotation.quaternionValue = Quaternion.Euler(v.DeserializeToVector3()));

        AddSettingPropertyHandler("localScale",
            () => m_LocalScale.vector3Value.SerializeToJObject(),
            v => m_LocalScale.vector3Value = v.DeserializeToVector3());
    }

    protected override void OnDumpRequested()
    {
        DumpProperty("localPosition", m_LocalPosition.vector3Value.SerializeToJObject());
        DumpProperty("localRotation", m_LocalRotation.quaternionValue.eulerAngles.SerializeToJObject());
        DumpProperty("localScale", m_LocalScale.vector3Value.SerializeToJObject());
    }

    protected override void OnDefinitionRequested()
    {
        EmitClassDefinition("Transform", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("localPosition", "Vector3"),
            TsPropertyDef.Field("localRotation", "Vector3")
                .WithComment("Euler angles in degrees"),
            TsPropertyDef.Field("localScale", "Vector3"),
        });
    }
}
```

## Complete Example: Composite Properties (RectTransform)

Demonstrates the most important pattern: bundling multiple serialized fields into a single **semantic property** with mode discrimination. The AI sees `horizontal` as one concept instead of scattered anchor/offset/size fields.

The getter builds a structured JSON object; the setter receives the (possibly patched) object and applies all fields atomically. This means `set("horizontal.offset", 50)` works — the system reads the current `horizontal`, patches `offset`, and passes the complete object to the setter.

```csharp
[CustomAiAgentEditor(typeof(RectTransform))]
public class RectTransformAgentEditor : TransformAgentEditor
{
    private SerializedProperty m_AnchoredPosition;
    private SerializedProperty m_SizeDelta;
    private SerializedProperty m_AnchorMin;
    private SerializedProperty m_AnchorMax;
    private SerializedProperty m_Pivot;
    private SerializedProperty m_OffsetMin;
    private SerializedProperty m_OffsetMax;

    protected override void OnEnable()
    {
        base.OnEnable();
        m_AnchoredPosition = serializedObject.FindProperty("m_AnchoredPosition");
        m_SizeDelta = serializedObject.FindProperty("m_SizeDelta");
        m_AnchorMin = serializedObject.FindProperty("m_AnchorMin");
        m_AnchorMax = serializedObject.FindProperty("m_AnchorMax");
        m_Pivot = serializedObject.FindProperty("m_Pivot");
        m_OffsetMin = serializedObject.FindProperty("m_OffsetMin");
        m_OffsetMax = serializedObject.FindProperty("m_OffsetMax");

        // Composite handler: getter builds full layout state, setter applies it
        AddSettingPropertyHandler("horizontal",
            () => GetLayoutAxisDump(0),
            v => ApplyLayoutAxis(0, v));
        AddSettingPropertyHandler("vertical",
            () => GetLayoutAxisDump(1),
            v => ApplyLayoutAxis(1, v));
        AddSettingPropertyHandler("pivot",
            () => m_Pivot.vector2Value.SerializeToJObject(),
            v => m_Pivot.vector2Value = v.DeserializeToVector2());
    }

    private JObject GetLayoutAxisDump(int axis)
    {
        var anchorMin = m_AnchorMin.vector2Value;
        var anchorMax = m_AnchorMax.vector2Value;
        bool isStretched = !Mathf.Approximately(anchorMin[axis], anchorMax[axis]);

        if (isStretched)
        {
            return new JObject
            {
                ["mode"] = "Stretch",
                ["from"] = anchorMin[axis],
                ["to"] = anchorMax[axis],
                ["insetStart"] = m_OffsetMin.vector2Value[axis],
                ["insetEnd"] = -m_OffsetMax.vector2Value[axis]
            };
        }
        return new JObject
        {
            ["mode"] = "Point",
            ["anchor"] = anchorMin[axis],
            ["offset"] = m_AnchoredPosition.vector2Value[axis],
            ["size"] = m_SizeDelta.vector2Value[axis]
        };
    }

    private void ApplyLayoutAxis(int axis, JToken data)
    {
        var mode = data["mode"]?.ToString();
        if (mode == "Point")
        {
            // Set anchors equal, apply offset and size
            var anchor = data["anchor"]?.ToObject<float>() ?? 0.5f;
            // ... apply to m_AnchorMin, m_AnchorMax, m_AnchoredPosition, m_SizeDelta
        }
        else if (mode == "Stretch")
        {
            // Set anchor range, apply insets
            // ... apply to m_AnchorMin, m_AnchorMax, m_OffsetMin, m_OffsetMax
        }
    }

    protected override void OnDumpRequested()
    {
        DumpProperty("horizontal", GetLayoutAxisDump(0));
        DumpProperty("vertical", GetLayoutAxisDump(1));
        DumpProperty("pivot", m_Pivot.vector2Value.SerializeToJObject());
        base.OnDumpRequested(); // Transform properties
    }

    protected override void OnDefinitionRequested()
    {
        base.OnDefinitionRequested();

        // Discriminated union types help the AI understand modes
        EmitClassDefinition("LayoutAxisPoint", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("mode", "LayoutAxisMode").WithValue("'Point'"),
            TsPropertyDef.Field("anchor", "number")
                .WithComment("Normalized anchor (0-1) relative to parent"),
            TsPropertyDef.Field("offset", "number")
                .WithComment("Offset in pixels from the anchor point"),
            TsPropertyDef.Field("size", "number")
                .WithComment("Size in pixels"),
        }, "LayoutAxis");

        EmitClassDefinition("LayoutAxisStretch", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("mode", "LayoutAxisMode").WithValue("'Stretch'"),
            TsPropertyDef.Field("from", "number")
                .WithComment("Normalized anchor start (0-1)"),
            TsPropertyDef.Field("to", "number")
                .WithComment("Normalized anchor end (0-1)"),
            TsPropertyDef.Field("insetStart", "number")
                .WithComment("Margin from start in pixels"),
            TsPropertyDef.Field("insetEnd", "number")
                .WithComment("Margin from end in pixels"),
        }, "LayoutAxis");

        EmitClassDefinition("RectTransform", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("horizontal", "LayoutAxis"),
            TsPropertyDef.Field("vertical", "LayoutAxis"),
            TsPropertyDef.Field("pivot", "Vector2"),
        }, "Transform");
    }
}
```

**What the AI sees in the dump:**
```json
{
    "horizontal": { "mode": "Point", "anchor": 0.5, "offset": 0, "size": 200 },
    "vertical": { "mode": "Stretch", "from": 0, "to": 1, "insetStart": 10, "insetEnd": 10 },
    "pivot": { "x": 0.5, "y": 0.5 }
}
```

**How the AI can set properties:**
- `set("horizontal", { "mode": "Stretch", "from": 0, "to": 1, "insetStart": 0, "insetEnd": 0 })` — full replacement
- `set("horizontal.offset", 50)` — partial patch, only changes offset within current mode
- `set("horizontal.mode", "Stretch")` — deep merge triggers mode switch

## Complete Example: Conditional + Enum-Rich Editor (Camera)

Shows conditional dump output, string enums, and physical-camera flag conversion.

```csharp
[CustomAiAgentEditor(typeof(Camera))]
public class CameraAgentEditor : AiAgentEditor
{
    private SerializedProperty m_ClearFlags;
    private SerializedProperty m_Orthographic;
    private SerializedProperty m_OrthographicSize;
    private SerializedProperty m_FieldOfView;
    private SerializedProperty m_projectionMatrixMode;
    private SerializedProperty m_FocalLength;
    // ... more fields

    protected override void OnEnable()
    {
        m_ClearFlags = serializedObject.FindProperty("m_ClearFlags");
        m_Orthographic = serializedObject.FindProperty("orthographic");
        m_OrthographicSize = serializedObject.FindProperty("orthographic size");
        m_FieldOfView = serializedObject.FindProperty("field of view");
        m_projectionMatrixMode = serializedObject.FindProperty("m_projectionMatrixMode");
        m_FocalLength = serializedObject.FindProperty("m_FocalLength");

        // Enum as string (AI reads "Skybox" not 1)
        AddSettingPropertyHandler("clearFlags",
            () => new JValue(((CameraClearFlags)m_ClearFlags.intValue).ToString()),
            v => m_ClearFlags.intValue = (int)ParseEnum<CameraClearFlags>(v));

        AddSettingPropertyHandler("orthographic",
            () => new JValue(m_Orthographic.boolValue),
            v => m_Orthographic.boolValue = v.Value<bool>());

        AddSettingPropertyHandler("fieldOfView",
            () => new JValue(m_FieldOfView.floatValue),
            v => m_FieldOfView.floatValue = Mathf.Clamp(v.Value<float>(), 0.00001f, 179f));

        // Converted property: int enum → boolean
        AddSettingPropertyHandler("usePhysicalProperties",
            () => new JValue(m_projectionMatrixMode.intValue == 2),
            v => m_projectionMatrixMode.intValue = v.Value<bool>() ? 2 : 0);
    }

    protected override void OnDumpRequested()
    {
        DumpProperty("clearFlags", ((CameraClearFlags)m_ClearFlags.intValue).ToString());
        DumpProperty("orthographic", m_Orthographic.boolValue);

        // Conditional: only dump the property that matters for current mode
        if (m_Orthographic.boolValue)
            DumpProperty("orthographicSize", m_OrthographicSize.floatValue);
        else
            DumpProperty("fieldOfView", m_FieldOfView.floatValue);

        bool isPhysical = m_projectionMatrixMode.intValue == 2;
        DumpProperty("usePhysicalProperties", isPhysical);
        if (isPhysical)
        {
            DumpProperty("focalLength", m_FocalLength.floatValue);
            // ... more physical camera properties
        }
    }

    protected override void OnDefinitionRequested()
    {
        GenerateEnumDefinition(typeof(CameraClearFlags));

        EmitClassDefinition("Camera", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("clearFlags", "CameraClearFlags"),
            TsPropertyDef.Field("orthographic", "boolean"),
            TsPropertyDef.Field("orthographicSize", "number")
                .WithComment("Half-size of the camera in orthographic mode"),
            TsPropertyDef.Field("fieldOfView", "number")
                .WithDecorator("type: Float, min: 0.00001, max: 179")
                .WithComment("Vertical field of view in degrees (perspective mode)"),
            TsPropertyDef.Field("usePhysicalProperties", "boolean")
                .WithHeader("Physical Camera"),
            TsPropertyDef.Field("focalLength", "number")
                .WithComment("Focal length in mm"),
        });
    }
}
```

## Complete Example: Settings Editor (Physics)

Settings editors extend `AiSettingsEditor` (not `AiAgentEditor`) because they operate on static APIs — no `target`, no `serializedObject`. The 3-arg pattern still applies identically.

```csharp
[CustomSettingsEditor("Physics")]
public class PhysicsSettingsEditor : AiSettingsEditor
{
    protected override void OnEnable()
    {
        // Static API properties — getter reads, setter writes
        AddSettingPropertyHandler("gravity",
            () => Physics.gravity.SerializeToJObject(),
            v => Physics.gravity = v.DeserializeToVector3());

        AddSettingPropertyHandler("bounceThreshold",
            () => new JValue(Physics.bounceThreshold),
            v => Physics.bounceThreshold = v.Value<float>());

        AddSettingPropertyHandler("autoSyncTransforms",
            () => new JValue(Physics.autoSyncTransforms),
            v => Physics.autoSyncTransforms = v.Value<bool>());
    }

    protected override void OnDumpRequested()
    {
        DumpProperty("gravity", Physics.gravity.SerializeToJObject());
        DumpProperty("bounceThreshold", Physics.bounceThreshold);
        DumpProperty("autoSyncTransforms", Physics.autoSyncTransforms);
    }

    protected override void OnDefinitionRequested()
    {
        EmitClassDefinition("PhysicsSettings", new List<TsPropertyDef>
        {
            TsPropertyDef.Field("gravity", "Vector3"),
            TsPropertyDef.Field("bounceThreshold", "number"),
            TsPropertyDef.Field("autoSyncTransforms", "boolean"),
        });
    }
}
```

---

## Best Practices Checklist

### Data Model
- [ ] **Design the AI-facing API first** — decide property names and structure before writing code. Ask: "Would an LLM understand this dump without reading the definition?"
- [ ] **Bundle related fields** — if multiple serialized properties represent one concept, expose them as a single composite object with a getter that builds the complete state
- [ ] **Use string enums** — dump `"Skybox"` not `1`. Accept both strings and ints in setters via `ParseEnum<T>`
- [ ] **Conditional dump** — only include properties relevant to the current state (e.g., skip `fieldOfView` when orthographic)
- [ ] **Convert representations** — expose Euler angles for Quaternions, booleans for int flags, friendly names for bitmasks

### Implementation
- [ ] **Always use 3-arg `AddSettingPropertyHandler`** — every handler needs both a getter and a setter for partial patch support
- [ ] **Use `serializedObject`** — the base class provides this. Do not create new `SerializedObject` instances
- [ ] **Cache properties in `OnEnable`** — look up all `SerializedProperty` references once
- [ ] **Do not handle Undo manually** — the base calls `serializedObject.Update()` before and `ApplyModifiedProperties()` after your handler
- [ ] **Use `target` only for read-only computed values** — like `rectTransform.rect` or `texture.width`. For writing, always go through `serializedObject`

### Definition Quality
- [ ] **Use `.WithComment()`** — explain what the property does, not its type
- [ ] **Use `.WithDecorator()`** — specify constraints (`min`, `max`, `type: Integer`)
- [ ] **Use `.WithHeader()`** — group related properties visually
- [ ] **Use `.Readonly()`** — mark computed/read-only values so the AI doesn't try to set them
- [ ] **Use `.Optional()`** — mark properties that only appear conditionally in the dump
