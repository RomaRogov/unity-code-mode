using System.Collections.Generic;

namespace CodeMode.Editor.AiAgentEditors
{
    public class TsPropertyDef
    {
        public string Name;
        public string Type;
        public string GenericType;
        public bool IsReadonly;
        public bool IsOptional;
        public bool IsNullable;
        public string Comment;
        public string SectionHeader;
        public string Decorator;
        public string Value;

        public string RenderedType
        {
            get
            {
                var t = GenericType != null ? $"{Type}<{GenericType}>" : Type;
                return IsNullable ? $"{t} | null" : t;
            }
        }

        #region Factory Methods

        public static TsPropertyDef Field(string name, string type)
            => new() { Name = name, Type = type };

        public static TsPropertyDef Reference(string name, string referencedType)
            => new() { Name = name, Type = "Reference", GenericType = referencedType };

        public static TsPropertyDef InstanceRef(string name, string referencedType)
            => new() { Name = name, Type = "InstanceReference", GenericType = referencedType };

        public static TsPropertyDef ArrayOf(string name, string elementType)
            => new() { Name = name, Type = "Array", GenericType = elementType };

        #endregion

        #region Fluent Modifiers

        public TsPropertyDef Readonly()
        {
            IsReadonly = true;
            return this;
        }

        public TsPropertyDef Optional()
        {
            IsOptional = true;
            return this;
        }

        public TsPropertyDef Nullable()
        {
            IsNullable = true;
            return this;
        }

        public TsPropertyDef WithComment(string comment)
        {
            Comment = comment;
            return this;
        }

        public TsPropertyDef WithHeader(string header)
        {
            SectionHeader = header;
            return this;
        }

        public TsPropertyDef WithDecorator(string decorator)
        {
            Decorator = decorator;
            return this;
        }

        public TsPropertyDef WithValue(string value)
        {
            Value = value;
            return this;
        }

        #endregion

        #region Rendering

        public void Render(List<string> lines)
        {
            if (SectionHeader != null)
                lines.Add($"\t// --- {SectionHeader} ---");

            if (Comment != null)
            {
                if (Comment.Contains("\n"))
                {
                    lines.Add("\t/**");
                    foreach (var line in Comment.Split('\n'))
                        if (line.Trim().Length > 0)
                            lines.Add($"\t * {line.Trim()}");
                    lines.Add("\t */");
                }
                else
                {
                    lines.Add($"\t// {Comment}");
                }
            }

            if (Decorator != null)
                lines.Add($"\t@property({{ {Decorator} }})");

            var ro = IsReadonly ? "readonly " : "";
            var opt = IsOptional ? "?" : "";
            var val = string.IsNullOrEmpty(Value) ? "" : $" = {Value}";
            lines.Add($"\t{ro}{Name}{opt}: {RenderedType}{val};");
        }

        #endregion
    }
}
