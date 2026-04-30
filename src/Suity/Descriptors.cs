using System.Collections.Generic;

namespace Suity
{
    /// <summary>
    /// Represents type information including kind, description, and fields.
    /// </summary>
    public class TypeInfoDescriptor
    {
        public string Kind;

        public string Description;

        public string Icon;

        public string Category;

        public string BaseType;

        public bool IsValueType;

        public List<FieldDescriptor> Fields = [];
    }

    /// <summary>
    /// Represents a field description with name, type, and description.
    /// </summary>
    public class FieldDescriptor
    {
        public string Name;

        public string Description;

        public string Type;
    }
}