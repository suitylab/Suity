using Suity.Editor.Types;

namespace Suity;

public static class TypeOpExtensions
{
    public static string TypeStr(this TypeDefinition typeInfo)
    {
        return string.Format("\"{0}\"", typeInfo.TypeCode);
    }

    public static string TypeStr(this string type)
    {
        return string.Format("\"{0}\"", type);
    }

    public static string TypeNew(this TypeDefinition typeInfo)
    {
        return string.Format("new \"{0}\"", typeInfo.TypeCode);
    }

    public static string TypeAs(this TypeDefinition typeInfo)
    {
        return string.Format("as \"{0}\"", typeInfo.TypeCode);
    }

    public static string TypeCast(this TypeDefinition typeInfo)
    {
        return string.Format("cast \"{0}\"", typeInfo.TypeCode);
    }

    public static string TypeOf(this TypeDefinition typeInfo)
    {
        return string.Format("typeof \"{0}\"", typeInfo.TypeCode);
    }

    public static string StaticType(this TypeDefinition typeInfo)
    {
        return string.Format("oftype(\"{0}\")", typeInfo.TypeCode);
    }

    public static string StaticType(this string type)
    {
        return string.Format("oftype(\"{0}\")", TypeDefinition.Resolve(type, true).TypeCode);
    }

    public static string GetFormatterTypeName(this DType type)
    {
        string parentFullTypeName = type?.ParentAsset?.FullTypeName ?? "???";
        return parentFullTypeName + "Formatter";
    }

    public static string GetFormatterTypeName(this TypeDefinition type)
    {
        string parentFullTypeName = type?.Target?.ParentAsset?.FullTypeName ?? "???";
        return parentFullTypeName + "Formatter";
    }
}