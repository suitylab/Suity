using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Selecting;
using Suity.Views.Named;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.Design;

/// <summary>
/// Represents a member that can be expressed in the form of a <see cref="KeyCode"/>
/// </summary>
public interface IMember : ISelectionItem, IHasAsset, IHasId, INamed
{
    /// <summary>
    /// Gets the container that this member belongs to
    /// </summary>
    IMemberContainer Container { get; }
}

/// <summary>
/// Defines the contract for a container that holds and manages members.
/// </summary>
public interface IMemberContainer
{
    /// <summary>
    /// Gets all members contained in this container.
    /// </summary>
    /// <value>An enumerable collection of members.</value>
    IEnumerable<IMember> Members { get; }

    /// <summary>
    /// Retrieves a specific member by its name.
    /// </summary>
    /// <param name="name">The name of the member to retrieve.</param>
    /// <returns>The member with the specified name.</returns>
    IMember GetMember(string name);

    /// <summary>
    /// Gets the count of members in this container.
    /// </summary>
    /// <value>The number of members contained.</value>
    int MemberCount { get; }
}

/// <summary>
/// Represents a field member.
/// </summary>
public interface IField : IMember
{
    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    TypeDefinition FieldType { get; }

    /// <summary>
    /// Gets the default value of the field.
    /// </summary>
    object DefaultValue { get; }

    /// <summary>
    /// Gets whether the field is public.
    /// </summary>
    bool IsPublic { get; }
}

/// <summary>
/// Represents a container of fields.
/// </summary>
public interface IFieldContainer : IMemberContainer
{
    /// <summary>
    /// Gets the asset key for the field container.
    /// </summary>
    string AssetKey { get; }

    /// <summary>
    /// Gets all fields in this container.
    /// </summary>
    IEnumerable<IField> Fields { get; }

    /// <summary>
    /// Gets a field by name.
    /// </summary>
    IField GetField(string name);
}

/// <summary>
/// Represents input for a field.
/// </summary>
public interface IFieldInput
{
    /// <summary>
    /// Gets the name of the field.
    /// </summary>
    string FieldName { get; }

    /// <summary>
    /// Gets the type of the field.
    /// </summary>
    TypeDefinition FieldType { get; }

    /// <summary>
    /// Gets the value of the field.
    /// </summary>
    object FieldValue { get; }
}

/// <summary>
/// Represents a function member.
/// </summary>
public interface IFunction : IMember, IVariableContainer
{
    /// <summary>
    /// Gets whether this function is user-defined.
    /// </summary>
    bool IsUser { get; }

    /// <summary>
    /// Gets whether this function is public.
    /// </summary>
    bool IsPublic { get; }

    /// <summary>
    /// Gets the return type of this function.
    /// </summary>
    TypeDefinition ReturnType { get; }

    /// <summary>
    /// Gets the actions of this function.
    /// </summary>
    SArray Actions { get; }
}

/// <summary>
/// Represents a container of functions.
/// </summary>
public interface IFunctionContainer
{
    /// <summary>
    /// Gets the name of this container.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Gets all functions in this container.
    /// </summary>
    IEnumerable<IFunction> Functions { get; }

    /// <summary>
    /// Gets a function by name.
    /// </summary>
    IFunction GetFunction(string name);
}

/// <summary>
/// Represents a component member.
/// </summary>
public interface IComponent : IMember
{
    /// <summary>
    /// Gets the binding name of this component.
    /// </summary>
    string BindingName { get; }

    /// <summary>
    /// Gets the type of this component.
    /// </summary>
    TypeDefinition ComponentType { get; }

    /// <summary>
    /// Gets the parent component.
    /// </summary>
    IComponent ParentComponent { get; }

    /// <summary>
    /// Gets all child components.
    /// </summary>
    IEnumerable<IComponent> ChildComponents { get; }

    /// <summary>
    /// Gets all field inputs for this component.
    /// </summary>
    IEnumerable<IFieldInput> ComponentFields { get; }

    /// <summary>
    /// Gets the component container owner.
    /// </summary>
    IComponentContainer ComponentOwner { get; }
}

/// <summary>
/// Represents a container of components.
/// </summary>
public interface IComponentContainer : IControllerContainer
{
    /// <summary>
    /// Gets all components in this container.
    /// </summary>
    IEnumerable<IComponent> Components { get; }

    /// <summary>
    /// Gets all components including nested ones.
    /// </summary>
    IEnumerable<IComponent> AllComponents { get; }

    /// <summary>
    /// Gets a component by name.
    /// </summary>
    IComponent GetComponent(string name);
}

/// <summary>
/// Represents a variable member.
/// </summary>
public interface IVariable : IMember, ISelectionItem, IHasAsset
{
    /// <summary>
    /// Gets the type of this variable.
    /// </summary>
    TypeDefinition VariableType { get; }

    /// <summary>
    /// Gets the display name of this variable.
    /// </summary>
    string DisplayName { get; }

    /// <summary>
    /// Gets whether this variable is a parameter.
    /// </summary>
    bool IsParameter { get; }

    /// <summary>
    /// Gets the default value of this variable.
    /// </summary>
    object DefaultValue { get; }
}

/// <summary>
/// Represents a container of variables.
/// </summary>
public interface IVariableContainer : IMemberContainer
{
    /// <summary>
    /// Gets the asset key for this container.
    /// </summary>
    string AssetKey { get; }

    /// <summary>
    /// Gets all variables in this container.
    /// </summary>
    IEnumerable<IVariable> Variables { get; }

    /// <summary>
    /// Gets a variable by name.
    /// </summary>
    IVariable GetVariable(string name);
}


/// <summary>
/// Represents an object that has attribute design.
/// </summary>
public interface IHasAttributeDesign
{
    /// <summary>
    /// Gets the attribute design.
    /// </summary>
    IAttributeDesign Attributes { get; }
}

/// <summary>
/// Represents an object that has tooltips.
/// </summary>
public interface IHasToolTips
{
    /// <summary>
    /// Gets the tooltip text.
    /// </summary>
    string ToolTips { get; }
}


public static class IDesignExtensions
{
    public static bool ContainsAttribute(this IHasAttributeDesign getter, string typeName)
    {
        return getter?.Attributes?.GetAttributes(typeName)?.Any() == true;
    }

    public static object GetAttribute(this IHasAttributeDesign getter, string typeName)
    {
        return getter?.Attributes?.GetAttributes(typeName)?.FirstOrDefault();
    }

    public static bool ContainsAttribute<T>(this IHasAttributeDesign getter) where T : class
    {
        return getter?.Attributes?.GetAttributes<T>()?.Any() == true;
    }

    public static T GetAttribute<T>(this IHasAttributeDesign getter) where T : class
    {
        return getter?.Attributes?.GetAttributes<T>()?.FirstOrDefault();
    }

    public static IEnumerable<T> GetAttributes<T>(this IHasAttributeDesign getter) where T : class
    {
        return getter?.Attributes?.GetAttributes<T>() ?? [];
    }
}