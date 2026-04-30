using System;

namespace Suity.Editor.Types
{
    #region NativeTypeAttribute

    /// <summary>
    /// Marks a type as a native type in the editor.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class NativeTypeAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the NativeTypeAttribute class.
        /// </summary>
        public NativeTypeAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NativeTypeAttribute class with a name.
        /// </summary>
        public NativeTypeAttribute(string name)
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentNullException();
            Name = name;
        }

        /// <summary>
        /// Gets or sets the code base.
        /// </summary>
        public string CodeBase { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the detail.
        /// </summary>
        public string Detail { get; set; }

        /// <summary>
        /// Gets or sets the brief description.
        /// </summary>
        public string Brief { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the return type.
        /// </summary>
        public string ReturnType { get; set; }

        /// <summary>
        /// Gets or sets the icon.
        /// </summary>
        public string Icon { get; set; }

        /// <summary>
        /// Gets or sets the color.
        /// </summary>
        public string Color { get; set; }

        /// <summary>
        /// Gets or sets the base type.
        /// </summary>
        public string BaseType { get; set; }

        /// <summary>
        /// Gets or sets whether this is a primary type.
        /// </summary>
        public bool IsPrimaryType { get; set; }

        /// <summary>
        /// Gets or sets the return type binding.
        /// </summary>
        public DReturnTypeBinding ReturnTypeBinding { get; set; }

        /// <summary>
        /// Gets or sets the controller type.
        /// </summary>
        public Type Controller { get; set; }
    }
    #endregion

    #region NativeAbstractAttribute

    /// <summary>
    /// Marks a type with abstract type references.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class NativeAbstractAttribute : Attribute
    {
        private readonly string[] _abstractTypes;

        /// <summary>
        /// Initializes a new instance of the NativeAbstractAttribute class.
        /// </summary>
        public NativeAbstractAttribute(params string[] abstractTypes)
        {
            this._abstractTypes = abstractTypes;
        }

        /// <summary>
        /// Gets the abstract types.
        /// </summary>
        public string[] AbstractTypes => _abstractTypes;
    }
    #endregion

    #region NativeReturnTypeAttribute

    /// <summary>
    /// Marks a type with a return type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class NativeReturnTypeAttribute : Attribute
    {
        private readonly string _returnType;

        /// <summary>
        /// Initializes a new instance of the NativeReturnTypeAttribute class.
        /// </summary>
        public NativeReturnTypeAttribute(string returnType)
        {
            _returnType = returnType;
        }

        /// <summary>
        /// Gets the return type.
        /// </summary>
        public string ReturnType => _returnType;
    }
    #endregion

    #region NativePrimaryAttribute

    /// <summary>
    /// Marks a type as a primary type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
    public sealed class NativePrimaryAttribute : Attribute
    {
    }
    #endregion

    #region NativeFieldAttribute

    /// <summary>
    /// Represents an editor field.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public sealed class NativeFieldAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the NativeFieldAttribute class.
        /// </summary>
        public NativeFieldAttribute()
        {
        }

        /// <summary>
        /// Initializes a new instance of the NativeFieldAttribute class with a name.
        /// </summary>
        public NativeFieldAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Gets or sets the field name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the category.
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets whether the field is optional.
        /// </summary>
        public bool Optional { get; set; }

        /// <summary>
        /// Gets or sets whether the field is hidden.
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// Gets or sets the default value.
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// Gets or sets whether this is a node view extension.
        /// </summary>
        public bool NodeViewExtension { get; set; }

        /// <summary>
        /// Gets or sets the modifier type.
        /// </summary>
        public Type Modifier { get; set; }

        /// <summary>
        /// Gets or sets the selection provider type.
        /// </summary>
        public Type SelectionProvider { get; set; }
    }
    #endregion

    #region NativeAliasAttribute

    /// <summary>
    /// Provides an alias for a native type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface | AttributeTargets.Enum, Inherited = false, AllowMultiple = true)]
    public sealed class NativeAliasAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the NativeAliasAttribute class.
        /// </summary>
        public NativeAliasAttribute()
        {
            UseForSaving = true;
        }

        /// <summary>
        /// Initializes a new instance of the NativeAliasAttribute class with an alias name.
        /// </summary>
        public NativeAliasAttribute(string aliasName)
        {
            AliasName = aliasName;
            UseForSaving = false;
        }

        /// <summary>
        /// Initializes a new instance of the NativeAliasAttribute class with an alias name and save option.
        /// </summary>
        public NativeAliasAttribute(string aliasName, bool useForSaving)
        {
            AliasName = aliasName;
            UseForSaving = useForSaving;
        }


        /// <summary>
        /// Gets the alias name.
        /// </summary>
        public string AliasName { get; }

        /// <summary>
        /// Gets or sets whether to use for saving.
        /// </summary>
        public bool UseForSaving { get; set; }
    }
    #endregion
}