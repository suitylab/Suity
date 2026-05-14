using Suity.Editor.Design;
using Suity.Views;
using System;

namespace Suity.Editor.Types;

/// <summary>
/// Backend implementation for native type registration and resolution.
/// </summary>
internal class NativeTypeExternalBK : NativeTypeExternal
{
    /// <summary>
    /// Gets the singleton instance of <see cref="NativeTypeExternalBK"/>.
    /// </summary>
    public static NativeTypeExternalBK Instance { get; } = new();

    private NativeTypeExternalBK()
    { }

    private bool _init;

    /// <summary>
    /// Initializes the native type external backend by registering all built-in native types.
    /// </summary>
    internal void Initialize()
    {
        if (_init)
        {
            return;
        }
        _init = true;

        if (GlobalIdResolver.Current is null)
        {
            throw new InvalidOperationException();
        }

        NativeTypeExternal._external = this;

        EditorObjectManager.Instance.DoUnwatchedAction(() =>
        {
            InitSystemGroup();
            InitSuityGroup();
            InitSuityControllingGroup();
            InitSuityEnitySystemGroup();
            InitGaming();
            InitMisc();
        });
    }

    private void InitSystemGroup()
    {
        string systemIcon = "*CoreIcon|System";

        NativeTypeFamilyBuilder builder = new NativeTypeFamilyBuilder()
            .WithLocalName("*System")
            .WithNameSpace("System")
            .WithAsset();

        NativeTypes.BooleanType = new DPrimative("Boolean", typeof(Boolean), TypeCode.Boolean, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.ByteType = new DPrimative("Byte", typeof(Byte), TypeCode.Byte, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.Int16Type = new DPrimative("Int16", typeof(Int16), TypeCode.Int16, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.Int32Type = new DPrimative("Int32", typeof(Int32), TypeCode.Int32, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.Int64Type = new DPrimative("Int64", typeof(Int64), TypeCode.Int64, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.SByteType = new DPrimative("SByte", typeof(SByte), TypeCode.SByte, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.UInt16Type = new DPrimative("UInt16", typeof(UInt16), TypeCode.UInt16, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.UInt32Type = new DPrimative("UInt32", typeof(UInt32), TypeCode.UInt32, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.UInt64Type = new DPrimative("UInt64", typeof(UInt64), TypeCode.UInt64, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.SingleType = new DPrimative("Single", typeof(Single), TypeCode.Single, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.DoubleType = new DPrimative("Double", typeof(Double), TypeCode.Double, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.DateTimeType = new DPrimative("DateTime", typeof(DateTime), TypeCode.DateTime, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.StringType = new DPrimative("String", typeof(String), TypeCode.String, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.VoidType = new DPrimative("Void", typeof(void), TypeCode.Empty, systemIcon).WithGroup(builder).ResolveDefinition();
        NativeTypes.TextBlockType = new DNativeStruct("TextBlock", typeof(TextBlock)).WithGroup(builder).ResolveDefinition();

        NativeTypes.ActionType = new DSystemAbstractFunctionBuilder("Action", null, "*CoreIcon|Action")
            .WithGroupBuilder(builder)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.ActionArrayType = NativeTypes.ActionType.MakeArrayType();
        NativeTypes.ConditionType = new DSystemAbstractFunctionBuilder("Condition", NativeTypes.BooleanType, "*CoreIcon|Filter")
            .WithGroupBuilder(builder)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.ConditionArrayType = NativeTypes.ConditionType.MakeArrayType();

        NativeTypes.ObjectType = new DNativeType("Object", typeof(object), systemIcon).WithGroup(builder)
            .ResolveDefinition();

        NativeTypes.DelegateType = new DDelegateBuilder("Delegate", "*CoreIcon|Delegate")
            .WithGroupBuilder(builder)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.NumericType = NumericTypeDefinition.Instance;

        //NativeTypes.SItemType = new DNativeType("SItem", typeof(SItem), "*CoreIcon|Value")
        //    .WithGroup(builder)
        //    .ResolveDefinition();

        //NativeTypes.DataRowType = new DNativeType("DataRow", typeof(IDataRow), "*CoreIcon|Data")
        //    .WithGroup(group)
        //    .ResolveDefinition();


        SystemGroup = builder.ResolveAsset();
    }


    private void InitSuityGroup()
    {
        var group = new NativeTypeFamilyBuilder()
            .WithLocalName("*Suity")
            .WithNameSpace("Suity")
            .WithResolvedProduct();

        NativeTypes.ByteArrayType = new DStructBuilder("ByteArray", "*CoreIcon|Array")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.EmptyResultType = new DStructBuilder("EmptyResult", "*CoreIcon|Return")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        var errorResultDef = new DStructBuilder("ErrorResult", "*CoreIcon|Error").WithAsset();
        errorResultDef.AddOrUpdateField("StatusCode", NativeTypes.StringType, AssetAccessMode.Public, null, true, null, null, true);
        errorResultDef.AddOrUpdateField("Message", NativeTypes.StringType, AssetAccessMode.Public, null, true, null, null, true);
        errorResultDef.AddOrUpdateField("Location", NativeTypes.StringType, AssetAccessMode.Public, null, true, null, null, true);
        NativeTypes.ErrorResultType = errorResultDef
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.ObjectPrototypeType = new DAbstractBuilder("ObjectPrototype", "*CoreIcon|Prototype")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

/*        var statusCodesDef = new DEnumBuilder("StatusCodes", "*CoreIcon|System");
        foreach (var item in Enum.GetValues(typeof(StatusCodes)))
        {
            statusCodesDef.AddOrUpdateField(item.ToString(), 0);
        }

        NativeTypes.StatusCodesType = statusCodesDef
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();*/

        NativeTypes.AttributeType = new DAbstract("Attribute", "*CoreIcon|Attribute")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.ToolDefinitionType = new DAbstract("ToolDefinition", "*CoreIcon|Tool")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.HandlerType = new DAbstract("Handler", "*CoreIcon|Handler")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.DefaultHandlerType = new DStruct("DefaultHandler", "*Suity|Handler", "*CoreIcon|Handler")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.ServerHandlerType = new DStruct("ServerHandler", "*Suity|Handler", "*CoreIcon|Server")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.ClientHandlerType = new DStruct("ClientHandler", "*Suity|Handler", "*CoreIcon|Client")
            .WithGroup(group)
            .ResolveDefinition();

        SuityGroup = group.ResolveAsset();
    }

    private void InitSuityControllingGroup()
    {
        NativeTypeFamilyBuilder group = new NativeTypeFamilyBuilder()
            .WithLocalName("*Suity.Controlling")
            .WithNameSpace("Suity.Controlling")
            .WithResolvedProduct();

        NativeTypes.StateType = new DStructBuilder("State", "*CoreIcon|State")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.StateMachineType = new DStructBuilder("StateMachine", "*CoreIcon|StateMachine")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.TriggerType = new DStructBuilder("Trigger", "*CoreIcon|Trigger")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.ControllerType = new DAbstract("Controller", "*CoreIcon|Controller")
            .WithGroup(group)
            .ResolveDefinition();

        NativeTypes.ComponentType = new DAbstract("Component", "*CoreIcon|Component")
            .WithGroup(group)
            .ResolveDefinition();

        SuityControllingGroup = group.ResolveAsset();
    }

    private void InitSuityEnitySystemGroup()
    {
        NativeTypeFamilyBuilder group = new NativeTypeFamilyBuilder()
            .WithLocalName("*Suity.EntitySystem")
            .WithNameSpace("Suity.EntitySystem")
            .WithResolvedProduct();

        NativeTypes.EntitySyncItemType = new DStructBuilder("EntitySyncItem", "*CoreIcon|Entity")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.EntityValueItemType = new DStructBuilder("EntityValueItem", "*CoreIcon|Value")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();
    }

    private void InitGaming()
    {
        NativeTypeFamilyBuilder group = new NativeTypeFamilyBuilder()
            .WithLocalName("*Suity.Gaming")
            .WithNameSpace("Suity.Gaming")
            .WithResolvedProduct();
        
        NativeTypes.GameEntityIdType = new DAbstractBuilder("GameEntityId", "*CoreIcon|Identity")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameEntityType = new DAbstractBuilder("GameEntity", "*CoreIcon|Entity")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameEventType = new DAbstractBuilder("GameEvent", "*CoreIcon|Event")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameConditionType = new DAbstractBuilder("GameCondition", "*CoreIcon|Condition")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameSelectorType = new DAbstractBuilder("GameSelector", "*CoreIcon|Selector")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameActionType = new DAbstractBuilder("GameAction", "*CoreIcon|Action")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        NativeTypes.GameEffectType = new DAbstractBuilder("GameEffect", "*CoreIcon|Effect")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();


        var tacticAI = new DAbstractBuilder("TacticAI", "*CoreIcon|AI");
        NativeTypes.TacticAIType = tacticAI
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

        var tacticAIAttr = new SArrayAttributeDesign();
        tacticAIAttr.AddAttribute<DataUsageAttribute>(o => o.Usage = DataUsageMode.TreeGraph);

        var nodeIdFieldAttr = new SArrayAttributeDesign();
        nodeIdFieldAttr.AddAttribute<ToolTipsAttribute>(o => o.ToolTips = "Node id in PascalCase, without any symbol such as [#].");

        tacticAI.UpdateAttributes(tacticAIAttr);
        tacticAI.AddOrUpdateField("NodeId", NativeTypes.StringType, AssetAccessMode.Public, null, true, null, nodeIdFieldAttr, true);
        //tacticAI.AddOrUpdateField("DefaultOutput", NativeTypes.TacticAIType, AssetAccessMode.Public, null, true, null, null, true);

        NativeTypes.GameActivityType = new DAbstractBuilder("GameActivity", "*CoreIcon|Activity")
            .WithAsset()
            .WithGroupBuilder(group)
            .EnsureAsset()
            .ResolveDefinition();

    }

    private void InitMisc()
    {
    }

    #region Groups

    internal GroupAsset SystemGroup { get; private set; }
    internal GroupAsset SuityGroup { get; private set; }
    internal GroupAsset SuityControllingGroup { get; private set; }

    #endregion

    /// <inheritdoc/>
    public override TypeDefinition GetBuildInTypeDefinition(string fullName) => fullName switch
    {
        "*System|Boolean" => NativeTypes.BooleanType,
        "*System|Byte" => NativeTypes.ByteType,
        "*System|SByte" => NativeTypes.SByteType,
        "*System|Int16" => NativeTypes.Int16Type,
        "*System|Int32" => NativeTypes.Int32Type,
        "*System|Int64" => NativeTypes.Int64Type,
        "*System|UInt16" => NativeTypes.UInt16Type,
        "*System|UInt32" => NativeTypes.UInt32Type,
        "*System|UInt64" => NativeTypes.UInt64Type,
        "*System|Single" => NativeTypes.SingleType,
        "*System|Double" => NativeTypes.DoubleType,
        "*System|Decimal" => NativeTypes.DecimalType,
        "*System|DateTime" => NativeTypes.DateTimeType,
        "*System|String" => NativeTypes.StringType,
        "*System|Object" => NativeTypes.ObjectType,
        "*System|Void" => NativeTypes.VoidType,
        "*System|Action" => NativeTypes.ActionType,
        "*System|Condition" => NativeTypes.ConditionType,
        "*System|Delegate" => NativeTypes.DelegateType,
        "*System|Numeric" => NativeTypes.NumericType,
        _ => null,
    };

    /// <inheritdoc/>
    public override TypeDefinition GetTypeDefinition(Type type)
    {
        if (type is null)
        {
            return null;
        }

        var dtype = NativeTypeReflector.Instance.GetDType(type);
        if (dtype != null)
        {
            return dtype.Definition;
        }

        string typeName = GetFullName(type.Name);

        var typeDef = GetBuildInTypeDefinition(typeName);
        if (!TypeDefinition.IsNullOrEmpty(typeDef))
        {
            return typeDef;
        }

        return GetAssetLinkDefinition(type);
    }

    /// <inheritdoc/>
    public override TypeDefinition GetAssetLinkDefinition(Type type)
    {
        var assetLink = AssetManager.Instance.GetAssetLink(type);

        return assetLink?.Definition;
    }

    public override string GetFullName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        return typeName.ToLower() switch
        {
            "bool" or "boolean" => "*System|Boolean",
            "byte" => "*System|Byte",
            "short" or "int16" => "*System|Int16",
            "int" or "int32" => "*System|Int32",
            "long" or "int64" => "*System|Int64",
            "sbyte" => "*System|Sbyte",
            "ushort" or "uint16" => "*System|UInt16",
            "uint" or "uint32" => "*System|UInt32",
            "ulong" or "uint64" => "*System|UInt64",
            "float" or "single" => "*System|Single",
            "double" or "number" => "*System|Double",
            "decimal" => "*System|Decimal",
            "datetime" => "*System|Datetime",
            "string" => "*System|String",
            "object" => "*System|Object",
            "void" => "*System|Void",
            "action" => "*System|Action",
            "condition" => "*System|Condition",
            "delegate" => "*System|Delegate",
            _ => typeName,
        };
    }

    public override string GetNativeTypeAlias(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        return typeName.ToLowerInvariant() switch
        {
            "boolean" or "*system|boolean" => "bool",
            "byte" or "*system|byte" => "byte",
            "int16" or "*system|int16" => "short",
            "int32" or "*system|int32" => "int",
            "int64" or "*system|int64" => "long",
            "sbyte" or "*system|sbyte" => "sbyte",
            "uint16" or "*system|uint16" => "ushort",
            "uint32" or "*system|uint32" => "uint",
            "uint64" or "*system|uint64" => "ulong",
            "single" or "*system|single" => "float",
            "double" or "*system|double" or "number" => "double",
            "decimal" or "*system|decimal" => "decimal",
            "datetime" or "*system|datetime" => "DateTime",
            "string" or "*system|string" => "string",
            "object" or "*system|object" => "object",
            "void" or "*system|void" => "void",
            "action" or "*system|action" => "Action",
            "condition" or "*system|condition" => "Condition",
            "delegate" or "*system|delegate" => "Delegate",
            _ => typeName,
        };
    }

    public override string GetNativeTypeShortName(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName))
        {
            return string.Empty;
        }

        return typeName.ToLowerInvariant() switch
        {
            "bool" or "*system|boolean" => "Boolean",
            "byte" or "*system|byte" => "Byte",
            "short" or "*system|int16" => "Int16",
            "int" or "*system|int32" => "Int32",
            "long" or "*system|int64" => "Int64",
            "sbyte" or "*system|sbyte" => "SByte",
            "ushort" or "*system|uint16" => "UInt16",
            "uint" or "*system|uint32" => "UInt32",
            "ulong" or "*system|uint64" => "UInt64",
            "float" or "*system|single" => "Single",
            "double" or "*system|double" or "number" => "Double",
            "decimal" or "*system|decimal" => "Decimal",
            "datetime" or "*system|datetime" => "DateTime",
            "string" or "*system|string" => "String",
            "object" or "*system|object" => "Object",
            "void" or "*system|void" => "Void",
            "action" or "*system|action" => "Action",
            "condition" or "*system|condition" => "Condition",
            "delegate" or "*system|delegate" => "Delegate",
            _ => typeName,
        };
    }

    public override bool GetIsNativeType(string typeName)
    {
        string nTypeName = GetNativeTypeShortName(typeName);

        return GetIsTypeNormalizedNative(nTypeName);
    }

    public override Type GetNativeTypeByLocalName(TypeDefinition typeInfo)
        => GetNativeType(typeInfo.Target?.LocalName);

    public override Type GetNativeType(string typeName)
    {
        typeName = GetFullName(typeName);

        return typeName switch
        {
            "String" => typeof(String),
            "Boolean" => typeof(Boolean),
            "Byte" => typeof(Byte),
            "Int16" => typeof(Int16),
            "Int32" => typeof(Int32),
            "Int64" => typeof(Int64),
            "SByte" => typeof(SByte),
            "UInt16" => typeof(UInt16),
            "UInt32" => typeof(UInt32),
            "UInt64" => typeof(UInt64),
            "Single" => typeof(Single),
            "Double" => typeof(Double),
            "Decimal" => typeof(Decimal),
            "DateTime" => typeof(DateTime),
            "Object" => typeof(Object),
            _ => null,
        };
    }

    private bool GetIsTypeNormalizedNative(string typeName)
    {
        return typeName switch
        {
            "Boolean" or "Byte" or "Int16" or "Int32" or "Int64" or "SByte" or "UInt16" or "UInt32" or "UInt64" or "Single" or "Double" or "String" or "DateTime" or "Decimal" or "Object" or "Action" or "Condition" or "Delegate" or "Void" => true,
            _ => false,
        };
    }
}