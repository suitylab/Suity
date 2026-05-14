using Suity.Editor.Design;
using Suity.Editor.Values;
using System;

namespace Suity.Editor.Types;

/// <summary>
/// Provides built-in native type definitions.
/// </summary>
public static class NativeTypes
{
    /// <summary>
    /// The type name for numeric types.
    /// </summary>
    public const string NumericTypeName = "*System|Numeric";

    /// <summary>
    /// The type name for value string types.
    /// </summary>
    public const string ValueStringTypeName = "*System|ValueString";

    /// <summary>
    /// The type name for action types.
    /// </summary>
    public const string ActionTypeName = "*System|Action";

    /// <summary>
    /// The type name for action array types.
    /// </summary>
    public const string ActionArrayTypeName = "*System|Action[]";

    /// <summary>
    /// The type name for condition types.
    /// </summary>
    public const string ConditionTypeName = "*System|Condition";

    /// <summary>
    /// The type name for condition array types.
    /// </summary>
    public const string ConditionArrayTypeName = "*System|Condition[]";

    /// <summary>
    /// The type name for delegate types.
    /// </summary>
    public const string DelegateTypeName = "*System|Delegate";

    /// <summary>
    /// The type name for void types.
    /// </summary>
    public const string VoidTypeName = "*System|Void";

    /// <summary>
    /// The type name for void array types.
    /// </summary>
    public const string VoidArrayTypeName = "*System|Void[]";

    /// <summary>
    /// The type name for attribute types.
    /// </summary>
    public const string AttributeTypeName = "*Suity|Attribute";

    #region NativeTypes

    /// <summary>
    /// Gets the Boolean type definition.
    /// </summary>
    public static TypeDefinition BooleanType { get; internal set; }

    /// <summary>
    /// Gets the Byte type definition.
    /// </summary>
    public static TypeDefinition ByteType { get; internal set; }

    /// <summary>
    /// Gets the Int16 type definition.
    /// </summary>
    public static TypeDefinition Int16Type { get; internal set; }

    /// <summary>
    /// Gets the Int32 type definition.
    /// </summary>
    public static TypeDefinition Int32Type { get; internal set; }

    /// <summary>
    /// Gets the Int64 type definition.
    /// </summary>
    public static TypeDefinition Int64Type { get; internal set; }

    /// <summary>
    /// Gets the SByte type definition.
    /// </summary>
    public static TypeDefinition SByteType { get; internal set; }

    /// <summary>
    /// Gets the UInt16 type definition.
    /// </summary>
    public static TypeDefinition UInt16Type { get; internal set; }

    /// <summary>
    /// Gets the UInt32 type definition.
    /// </summary>
    public static TypeDefinition UInt32Type { get; internal set; }

    /// <summary>
    /// Gets the UInt64 type definition.
    /// </summary>
    public static TypeDefinition UInt64Type { get; internal set; }

    /// <summary>
    /// Gets the Single (float) type definition.
    /// </summary>
    public static TypeDefinition SingleType { get; internal set; }

    /// <summary>
    /// Gets the Double type definition.
    /// </summary>
    public static TypeDefinition DoubleType { get; internal set; }

    /// <summary>
    /// Gets the Decimal type definition.
    /// </summary>
    public static TypeDefinition DecimalType { get; internal set; }

    /// <summary>
    /// Gets the DateTime type definition.
    /// </summary>
    public static TypeDefinition DateTimeType { get; internal set; }

    /// <summary>
    /// Gets the Color type definition.
    /// </summary>
    public static TypeDefinition ColorType { get; internal set; }

    /// <summary>
    /// Gets the String type definition.
    /// </summary>
    public static TypeDefinition StringType { get; internal set; }

    /// <summary>
    /// Gets the Object type definition.
    /// </summary>
    public static TypeDefinition ObjectType { get; internal set; }

    /// <summary>
    /// Gets the Void type definition.
    /// </summary>
    public static TypeDefinition VoidType { get; internal set; }

    /// <summary>
    /// Gets the Numeric abstract type definition.
    /// </summary>
    public static TypeDefinition NumericType { get; internal set; }

    /// <summary>
    /// Gets the TextBlock type definition.
    /// </summary>
    public static TypeDefinition TextBlockType { get; internal set; }

    /// <summary>
    /// Gets the Action type definition.
    /// </summary>
    public static TypeDefinition ActionType { get; internal set; }

    /// <summary>
    /// Gets the Action array type definition.
    /// </summary>
    public static TypeDefinition ActionArrayType { get; internal set; }

    /// <summary>
    /// Gets the Condition type definition.
    /// </summary>
    public static TypeDefinition ConditionType { get; internal set; }

    /// <summary>
    /// Gets the Condition array type definition.
    /// </summary>
    public static TypeDefinition ConditionArrayType { get; internal set; }

    /// <summary>
    /// Gets the Delegate type definition.
    /// </summary>
    public static TypeDefinition DelegateType { get; internal set; }


    #endregion

    #region Suity Types

    /// <summary>
    /// Gets the ByteArray type definition.
    /// </summary>
    public static TypeDefinition ByteArrayType { get; internal set; }

    /// <summary>
    /// Gets the EmptyResult type definition.
    /// </summary>
    public static TypeDefinition EmptyResultType { get; internal set; }

    /// <summary>
    /// Gets the ErrorResult type definition.
    /// </summary>
    public static TypeDefinition ErrorResultType { get; internal set; }

    /// <summary>
    /// Gets the ObjectPrototype type definition.
    /// </summary>
    public static TypeDefinition ObjectPrototypeType { get; internal set; }
    /*public static TypeDefinition StatusCodesType { get; internal set; }*/

    #endregion

    #region Suity Attribute Types

    /// <summary>
    /// Gets the Attribute type definition.
    /// </summary>
    public static TypeDefinition AttributeType { get; internal set; }

    /// <summary>
    /// Gets the Tool type definition.
    /// </summary>
    public static TypeDefinition ToolDefinitionType { get; internal set; }

    /// <summary>
    /// Gets the Handler type definition.
    /// </summary>
    public static TypeDefinition HandlerType { get; internal set; }

    /// <summary>
    /// Gets the DefaultHandler type definition.
    /// </summary>
    public static TypeDefinition DefaultHandlerType { get; internal set; }

    /// <summary>
    /// Gets the ServerHandler type definition.
    /// </summary>
    public static TypeDefinition ServerHandlerType { get; internal set; }

    /// <summary>
    /// Gets the ClientHandler type definition.
    /// </summary>
    public static TypeDefinition ClientHandlerType { get; internal set; }

    #endregion

    #region Suity Controlling Types

    /// <summary>
    /// Gets the State type definition.
    /// </summary>
    public static TypeDefinition StateType { get; internal set; }

    /// <summary>
    /// Gets the StateMachine type definition.
    /// </summary>
    public static TypeDefinition StateMachineType { get; internal set; }

    /// <summary>
    /// Gets the Trigger type definition.
    /// </summary>
    public static TypeDefinition TriggerType { get; internal set; }

    /// <summary>
    /// Gets the Controller type definition.
    /// </summary>
    public static TypeDefinition ControllerType { get; internal set; }

    /// <summary>
    /// Gets the Component type definition.
    /// </summary>
    public static TypeDefinition ComponentType { get; internal set; }

    #endregion

    #region Suity Entity System Types

    /// <summary>
    /// Gets the EntitySyncItem type definition.
    /// </summary>
    public static TypeDefinition EntitySyncItemType { get; internal set; }

    /// <summary>
    /// Gets the EntityValueItem type definition.
    /// </summary>
    public static TypeDefinition EntityValueItemType { get; internal set; }

    #endregion

    #region Gaming

    /// <summary>
    /// Gets the GameEntityId type definition.
    /// </summary>
    public static TypeDefinition GameEntityIdType { get; internal set; }

    /// <summary>
    /// Gets the GameEntity type definition.
    /// </summary>
    public static TypeDefinition GameEntityType { get; internal set; }


    /// <summary>
    /// Gets the GameEvent type definition.
    /// </summary>
    public static TypeDefinition GameEventType { get; internal set; }

    /// <summary>
    /// Gets the GameCondition type definition.
    /// </summary>
    public static TypeDefinition GameConditionType { get; internal set; }

    /// <summary>
    /// Gets the GameSelector type definition.
    /// </summary>
    public static TypeDefinition GameSelectorType { get; internal set; }

    /// <summary>
    /// Gets the GameAction type definition.
    /// </summary>
    public static TypeDefinition GameActionType { get; internal set; }

    /// <summary>
    /// Gets the GameEffect type definition.
    /// </summary>
    public static TypeDefinition GameEffectType { get; internal set; }

    /// <summary>
    /// Gets the TacticAI type definition.
    /// </summary>
    public static TypeDefinition TacticAIType { get; internal set; }

    /// <summary>
    /// Gets the GameActivity type definition.
    /// </summary>
    public static TypeDefinition GameActivityType { get; internal set; }


    #endregion

    /// <summary>
    /// Gets the SItem type definition.
    /// </summary>
    public static TypeDefinition SItemType => TypeDefinition.FromNative<SItem>();

    /// <summary>
    /// Gets the SObject type definition.
    /// </summary>
    public static TypeDefinition SObjectType => TypeDefinition.FromNative<SObject>();

    /// <summary>
    /// Gets the IDataContainer type definition.
    /// </summary>
    public static TypeDefinition DataTableType => TypeDefinition.FromNative<IDataContainer>();

    /// <summary>
    /// Gets the IDataItem type definition.
    /// </summary>
    public static TypeDefinition DataRowType => TypeDefinition.FromNative<IDataItem>();

    /// <summary>
    /// Gets the built-in type definition by full name.
    /// </summary>
    /// <param name="fullName">The full type name.</param>
    /// <returns>The type definition.</returns>
    public static TypeDefinition GetBuildInTypeDefinition(string fullName)
        => NativeTypeExternal._external.GetBuildInTypeDefinition(fullName);

    /// <summary>
    /// Gets the full name of a type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The full name.</returns>
    public static string GetFullName(string typeName)
        => NativeTypeExternal._external.GetFullName(typeName);

    /// <summary>
    /// Gets the native type alias.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The alias.</returns>
    public static string GetNativeTypeAlias(string typeName)
        => NativeTypeExternal._external.GetNativeTypeAlias(typeName);

    /// <summary>
    /// Gets the short name of a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The short name.</returns>
    public static string GetNativeTypeShortName(string typeName)
        => NativeTypeExternal._external.GetNativeTypeShortName(typeName);

    /// <summary>
    /// Determines whether the type is a native type.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>True if native; otherwise, false.</returns>
    public static bool GetIsNativeType(string typeName)
        => NativeTypeExternal._external.GetIsNativeType(typeName);

    /// <summary>
    /// Gets the native type by local name.
    /// </summary>
    /// <param name="typeInfo">The type definition.</param>
    /// <returns>The native type.</returns>
    internal static Type GetNativeTypeByLocalName(TypeDefinition typeInfo)
        => NativeTypeExternal._external.GetNativeTypeByLocalName(typeInfo);

    /// <summary>
    /// Gets the native type by name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The native type.</returns>
    public static Type GetNativeType(string typeName)
        => NativeTypeExternal._external.GetNativeType(typeName);

    /// <summary>
    /// Gets the native DType by name.
    /// </summary>
    /// <param name="typeName">The type name.</param>
    /// <returns>The native DType.</returns>
    public static DType GetNativeDType(string typeName)
    {
        var type = GetNativeType(typeName);
        if (type != null)
        {
            return DTypeManager.Instance.GetNativeDType(type);
        }
        else
        {
            return null;
        }
    }
}