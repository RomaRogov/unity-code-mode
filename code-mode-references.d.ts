// Instance reference should newer be kept in memory
type InstanceReference = { id: string; type: string };

interface IExposedAttributes { type?: string, visible?: boolean, multiline?: boolean, min?: number, max?: number }
// Decorator for properties
declare function property(options: IExposedAttributes): any

// Unity types
type Vector2 = { x: number; y: number; }
type Vector3 = { x: number; y: number; z: number; }
type Vector4 = { x: number; y: number; z: number; w: number; }
type Bounds = { center: Vector3; size: Vector3; }
type Quaternion = { x: number; y: number; z: number; w: number; }
type Vector2Int = { x: number; y: number; }
type Vector3Int = { x: number; y: number; z: number; }
type RectInt = { x: number; y: number; width: number; height: number; }
type BoundsInt = { position: Vector3Int; size: Vector3Int; }
type Matrix4x4 = { m00: number; m01: number; m02: number; m03: number; 
    m10: number; m11: number; m12: number; m13: number; 
    m20: number; m21: number; m22: number; m23: number; 
    m30: number; m31: number; m32: number; m33: number; }
type Color = { r: number; g: number; b: number; a: number; }
type Rect = { x: number; y: number; width: number; height: number; }
type Size = { width: number, height: number };
type AnimationCurve = { keys: Array<{ time: number, value: number, inTangent: number, outTangent: number }> }
type Gradient = { colorKeys: Array<{ color: Color, time: number }>, alphaKeys: Array<{ alpha: number, time: number }>, mode: number }

/**
 * Unity Editor Tools
 */
declare namespace UnityEditor {
    /** Create empty asset or folder of given type. Returns reference to the new asset. */
    function AssetCreate(args: {
        assetPath?: string,
        preset: "Folder" | "Prefab" | "Material" | "ScriptableObject" | "Scene" | "PhysicsMaterial" | "PhysicsMaterial2D" | "SpriteAtlas" | "RenderTexture" | "AnimatorController" | "AnimationClip",
        scriptableObjectType?: string,
        options?: { overwrite: boolean; rename: boolean }
    }): { reference: InstanceReference };

    /** Get asset reference by path. Supports subasset paths like 'Assets/Model.fbx/MeshName'. */
    function AssetGetAtPath(args: { assetPath: string }): { reference: InstanceReference };

    /** Returns preview image of the asset (Prefab, Texture, Model, Material, etc.) as base64 JPEG. */
    function AssetGetPreview(args: {
        reference: InstanceReference,
        jpegQuality: number
    }): { type: string, data: string, mimeType: string };

    interface IAssetTree {
        filesystemPath?: string;
        reference: InstanceReference;
        name: string;
        children: IAssetTree[];
    }
    /** Get the asset and subAsset hierarchy tree. Children have recursive structure. */
    function AssetGetTree(args: {
        reference?: InstanceReference,
        assetPath?: string
    }): IAssetTree;

    /** Import an external file as an asset into the project */
    function AssetImport(args: {
        sourceFilesystemPath: string,
        targetAssetPath: string,
        imageType?: "Default" | "NormalMap" | "GUI" | "Cookie" | "Lightmap" | "Cursor" | "Sprite" | "SingleChannel" | "Shadowmask" | "DirectionalLightmap" | "Image" | "HDRI" | "Advanced" | "Cubemap" | "Reflection" | "Bump",
        options?: { overwrite: boolean; rename: boolean }
    }): { reference: InstanceReference };

    /** Perform operations on assets: move, copy, delete, open, refresh, reimport */
    function AssetOperate(args: {
        operation: "Move" | "Copy" | "Delete" | "Open" | "Refresh" | "Reimport",
        reference: InstanceReference,
        targetAssetPath?: string,
        options?: { overwrite: boolean; rename: boolean }
    }): { success: boolean, error?: string };

    /** Get last N editor log entries */
    function EditorGetLogs(args: {
        count: number,
        showStack: boolean,
        order: "NewestToOldest" | "OldestToNewest"
    }): { logLines: string[] };

    /** Returns preview image of scene view. Return result DIRECTLY to visualize. */
    function EditorGetScenePreview(args: {
        width: number,
        height: number,
        jpegQuality: number,
        cameraPosition: Vector3,
        targetPosition: Vector3
    }): { type: string, data: string, mimeType: string };

    /** Common editor operations: save, play, pause, step, stop, refresh, compile */
    function EditorOperate(args: {
        operation: "Save" | "Play" | "Pause" | "Step" | "Stop" | "Refresh" | "Compile"
    }): { success: boolean, error?: string };

    /** Add a component to a referenced GameObject, returns reference to the new component */
    function GameObjectComponentAdd(args: {
        reference: InstanceReference,
        componentType: string
    }): { reference: InstanceReference };

    /** Remove referenced component from GameObject it is attached to. */
    function GameObjectComponentRemove(args: { reference: InstanceReference }): { success: boolean, error?: string };

    /** Get components of specific type on a GameObject. If componentType is not provided, returns all components. */
    function GameObjectComponentsGet(args: {
        reference: InstanceReference,
        componentType: string
    }): { references: InstanceReference[] };

    /** Create a new GameObject in the scene. If no parent is specified, root is used. */
    function GameObjectCreate(args: {
        name: string,
        parentReference?: InstanceReference,
        assetReference?: InstanceReference
    }): { reference: InstanceReference };

    /** Create a new GameObject with predefined primitive geometry MeshRenderer. */
    function GameObjectCreatePrimitive(args: {
        name: string,
        primitiveType: "Sphere" | "Capsule" | "Cylinder" | "Cube" | "Plane" | "Quad",
        parentReference?: InstanceReference
    }): { reference: InstanceReference };

    /** Get GameObjects at specific path in the scene hierarchy. */
    function GameObjectGetAtPath(args: { hierarchyPath: string }): { references: InstanceReference[] };

    /** Get list of globally available component types (class names). */
    function GameObjectGetAvailableComponentTypes(args: {
        includeInternal: boolean,
        filter?: string
    }): { componentTypes: string[] };

    interface IHierarchyTree {
        path?: string;
        reference: InstanceReference;
        name: string;
        active: boolean;
        components: InstanceReference[];
        children: IHierarchyTree[];
    }
    /** Get the hierarchy tree of specific GameObject or scene root if no reference is provided. */
    function GameObjectGetTree(args: { reference: InstanceReference }): IHierarchyTree;

    /** Perform operation on referenced GameObject, including prefab operations. */
    function GameObjectOperate(args: {
        operation: "Move" | "Copy" | "Delete" | "CreatePrefab" | "RevertPrefab" | "ApplyPrefab" | "UnwrapPrefab" | "UnwrapPrefabCompletely" | "OpenPrefab",
        reference: InstanceReference,
        newParentReference?: InstanceReference,
        newPrefabPath?: string,
        siblingIndex?: number
    }): {
        success: boolean,
        createdPrefabAssetReference?: InstanceReference,
        updatedGameObjectReference?: InstanceReference,
        copiedGameObjectReference?: InstanceReference
    };

    /** Generates TypeScript definition based on properties of an instance. */
    function InspectorGetInstanceDefinition(args: { reference: InstanceReference }): { definition: string };

    /** Gets plain object of properties, with no serialization info for any instance. */
    function InspectorGetInstanceProperties(args: { reference: InstanceReference }): { dump: any };

    /** Sets a property on instance of Component, GameObject or Asset. */
    function InspectorSetInstanceProperty(args: {
        reference: InstanceReference,
        propertyPath: string,
        value: any
    }): { success: boolean, error?: string };

    /** Generates TypeScript definition for project settings or common types. */
    function SettingsGetDefinition(args: { settingsType: "Physics" | "Physics2D" | "Lighting" }): { definition: string };

    /** Gets properties for project settings by type. */
    function SettingsGetProperties(args: { settingsType: "Physics" | "Physics2D" | "Lighting" }): { dump: any };

    /** Sets a property on project settings. */
    function SettingsSetProperty(args: {
        settingsType: "Physics" | "Physics2D" | "Lighting",
        propertyPath: string,
        value: any
    }): { success: boolean, error?: string };
}

/**
 * RectTransform Properties (from InspectorGetInstanceDefinition)
 * Unity's RectTransform uses LayoutAxis system, NOT legacy anchored position!
 */
interface RectTransform {
    // Horizontal layout: either Point or Stretch mode
    horizontal: LayoutAxisPoint | LayoutAxisStretch;
    // Vertical layout: either Point or Stretch mode
    vertical: LayoutAxisPoint | LayoutAxisStretch;
    pivot: Vector2;
    localPosition: Vector3;
    localRotation: Vector3;
    localScale: Vector3;
}

interface LayoutAxisPoint {
    mode: "Point";
    anchor: number;      // Normalized anchor (0-1) relative to parent
    offset: number;      // Offset in pixels from anchor point
    size: number;        // Size in pixels
}

interface LayoutAxisStretch {
    mode: "Stretch";
    from: number;        // Normalized anchor start (0-1)
    to: number;          // Normalized anchor end (0-1)  
    insetStart: number;  // Margin in pixels from start
    insetEnd: number;    // Margin in pixels from end
}
