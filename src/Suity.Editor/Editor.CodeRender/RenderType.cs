using System;
using System.Drawing;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Represents a render type asset.
/// </summary>
public class RenderType : Asset
{
    private readonly Image _icon;

    internal RenderType(string name, string displayName = null, Image icon = null)
        : base(name)
    {
        Description = displayName;
        _icon = icon;
    }

    /// <summary>
    /// Gets the default icon for the render type.
    /// </summary>
    public override Image DefaultIcon => _icon ?? base.DefaultIcon;

    /// <summary>
    /// Type family name.
    /// </summary>
    public const string TypeFamilyName = "TypeFamily";

    /// <summary>
    /// Type formatter name.
    /// </summary>
    public const string TypeFormatterName = "TypeFormatter";

    /// <summary>
    /// Struct name.
    /// </summary>
    public const string StructName = "Struct";

    /// <summary>
    /// Abstract class name.
    /// </summary>
    public const string AbstractName = "Abstract";

    /// <summary>
    /// Enum name.
    /// </summary>
    public const string EnumName = "Enum";

    /// <summary>
    /// Logic module name.
    /// </summary>
    public const string LogicModuleName = "LogicModule";

    /// <summary>
    /// Function family name.
    /// </summary>
    public const string FunctionFamilyName = "FunctionFamily";

    /// <summary>
    /// Function name.
    /// </summary>
    public const string FunctionName = "Function";

    /// <summary>
    /// Data family name.
    /// </summary>
    public const string DataFamilyName = "DataFamily";

    /// <summary>
    /// Data name.
    /// </summary>
    public const string DataName = "Data";

    /// <summary>
    /// Trigger controller name.
    /// </summary>
    public const string TriggerControllerName = "TriggerController";

    /// <summary>
    /// Component name.
    /// </summary>
    public const string ComponentName = "Component";

    /// <summary>
    /// Rex tree name.
    /// </summary>
    public const string RexTreeName = "RexTree";

    /// <summary>
    /// Binary name.
    /// </summary>
    public const string BinaryName = "Binary";

    /// <summary>
    /// Text name.
    /// </summary>
    public const string TextName = "Text";

    /// <summary>
    /// Adds a new render type.
    /// </summary>
    /// <param name="name">The name of the render type.</param>
    /// <param name="description">The description of the render type.</param>
    /// <returns>The created render type.</returns>
    public static RenderType AddRenderType(string name, string description)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        var renderType = new RenderType(name, description);
        Group.AddOrUpdateChildAsset(renderType, IdResolveType.FullName);

        return renderType;
    }

    /// <summary>
    /// Gets a render type by name.
    /// </summary>
    /// <param name="name">The name of the render type.</param>
    /// <returns>The render type, or null if not found.</returns>
    public static RenderType GetRenderType(string name)
    {
        return Group.GetChildAsset(name) as RenderType;
    }

    internal static GroupAsset Group { get; set; }

    /// <summary>
    /// Type family render type.
    /// </summary>
    public static RenderType TypeFamily { get; internal set; }

    /// <summary>
    /// Type formatter render type.
    /// </summary>
    public static RenderType TypeFormatter { get; internal set; }

    /// <summary>
    /// Struct render type.
    /// </summary>
    public static RenderType Struct { get; internal set; }

    /// <summary>
    /// Abstract class render type.
    /// </summary>
    public static RenderType Abstract { get; internal set; }

    /// <summary>
    /// Enum render type.
    /// </summary>
    public static RenderType Enum { get; internal set; }

    /// <summary>
    /// Logic module render type.
    /// </summary>
    public static RenderType LogicModule { get; internal set; }

    /// <summary>
    /// Function family render type.
    /// </summary>
    public static RenderType FunctionFamily { get; internal set; }

    /// <summary>
    /// Function render type.
    /// </summary>
    public static RenderType Function { get; internal set; }

    /// <summary>
    /// Data family render type.
    /// </summary>
    public static RenderType DataFamily { get; internal set; }

    /// <summary>
    /// Data render type.
    /// </summary>
    public static RenderType Data { get; internal set; }

    /// <summary>
    /// Trigger controller render type.
    /// </summary>
    public static RenderType TriggerController { get; internal set; }

    /// <summary>
    /// Component render type.
    /// </summary>
    public static RenderType Component { get; internal set; }

    /// <summary>
    /// Rex tree render type.
    /// </summary>
    public static RenderType RexTree { get; internal set; }

    /// <summary>
    /// Binary render type.
    /// </summary>
    public static RenderType Binary { get; internal set; }

    /// <summary>
    /// Text render type.
    /// </summary>
    public static RenderType Text { get; internal set; }
}