namespace Suity;

/// <summary>
/// Resource definition names
/// </summary>
public static class AssetDefNames
{
    public const string AssetLinkPrefix = "*AssetLink";

    public const string TypeFamily = "DTypeFamily";

    public const string Type = "DType";

    public const string Enum = "DEnum";
    public const string Struct = "DStruct";
    public const string Abstract = "DAbstract";
    public const string Event = "DEvent";
    public const string Function = "DFunction";
    public const string AbstractFunction = "DAbstractFunction";
    public const string Delegate = "DDelegate";
    public const string NativeValueType = "DNativeValueType";

    public const string LogicModule = "LogicModule";

    public const string Data = "KeyLink";
    public const string DataFamily = "DataFamily";

    public const string Value = "Value";
    public const string ValueFamily = "ValueFamily";

    public const string Asset = "AssetLink";

    public const string Controller = "DController";

    public const string Template = "Template";

    /// <summary>
    /// Masks an asset type name with the asset link prefix.
    /// </summary>
    /// <param name="name">The asset type name.</param>
    /// <returns>The masked asset type name.</returns>
    public static string MaskAssetTypeName(string name)
    {
        if (string.IsNullOrEmpty(name))
        {
            return null;
        }

        return KeyCode.Combine(AssetLinkPrefix, name);
    }
}