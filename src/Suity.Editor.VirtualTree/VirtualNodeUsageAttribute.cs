using System;
using System.Reflection;

namespace Suity.Editor.VirtualTree;

/// <summary>
/// Calculates a PropertyEditorAssignment priority for the given property Type in a certain context.
/// </summary>
/// <param name="propertyType"></param>
/// <param name="context"></param>
/// <returns>The priority which connects the given Type to the PropertyEditor. Return <see cref="UserPropertyAttribute.PriorityNone"/> for no assignment.</returns>
public delegate int PropertyEditorMatching(Type propertyType, ProviderContext context);

/// <summary>
/// This attribute is utilized to match PropertyEditors to the property Types they are supposed to edit.
/// </summary>
public class VirtualNodeUsageAttribute : Attribute
{
    public const int PriorityGeneral = VirtualTreeModel.EditorPriority_General;
    public const int PrioritySpecialized = VirtualTreeModel.EditorPriority_Specialized;
    public const int PriorityOverride = VirtualTreeModel.EditorPriority_Override;
    public const int PriorityNone = VirtualTreeModel.EditorPriority_None;

    private readonly Type assignToType = null;
    private readonly int assignPriority = VirtualTreeModel.EditorPriority_General;
    private readonly PropertyEditorMatching dynamicAssign = null;

    /// <summary>
    /// Creates a static PropertyEditor assignment to the specified property Type.
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="priority"></param>
    public VirtualNodeUsageAttribute(Type propertyType, int priority = VirtualTreeModel.EditorPriority_General)
    {
        this.assignToType = propertyType;
        this.assignPriority = priority;
    }

    /// <summary>
    /// Creates a dynamic PropertyEditor assignment based on the specified static <see cref="PropertyEditorMatching"/> method.
    /// </summary>
    /// <param name="methodHostType"></param>
    /// <param name="staticMethodName"></param>
    public VirtualNodeUsageAttribute(Type methodHostType, string staticMethodName)
    {
        MethodInfo methodInfo = methodHostType.GetMethod(staticMethodName, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        this.dynamicAssign = Delegate.CreateDelegate(typeof(PropertyEditorMatching), methodInfo) as PropertyEditorMatching;
    }

    /// <summary>
    /// Determines the matching level between a PropertyEditor and the specified property Type.
    /// </summary>
    /// <param name="propertyType"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public int MatchToProperty(Type propertyType, ProviderContext context)
    {
        if (this.dynamicAssign != null)
            return this.dynamicAssign(propertyType, context);
        else if (this.assignToType.IsAssignableFrom(propertyType))
            return this.assignPriority;
        else
            return VirtualTreeModel.EditorPriority_None;
    }
}