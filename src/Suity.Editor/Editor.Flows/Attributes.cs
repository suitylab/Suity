using System;

namespace Suity.Editor.Flows;

[System.AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class FlowConnectorAliasAttribute : Attribute
{
    public FlowConnectorAliasAttribute(string aliasName, string realName)
    {
        AliasName = aliasName;
        RealName = realName;
    }

    public string AliasName { get; }
    public string RealName { get; }
}

/// <summary>
/// Document view usage
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class FlowExpandedViewUsageAttribute : Attribute
{
    public Type ObjectType { get; }

    public FlowExpandedViewUsageAttribute(Type objectType)
    {
        ObjectType = objectType;
    }
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class DefaultFlowExpandedViewAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class CanvasAssetTypeAttribute : Attribute
{
    public Type AssetType { get; }

    public CanvasAssetTypeAttribute(Type assetType)
    {
        AssetType = assetType;
    }
}