using System;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Workspace controller info
/// </summary>
public class WorkSpaceControllerInfo
{
    /// <summary>
    /// Gets the controller name
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// Gets the display name
    /// </summary>
    public string DisplayName { get; }
    /// <summary>
    /// Gets the icon key
    /// </summary>
    public string IconKey { get; }
    /// <summary>
    /// Gets the controller type
    /// </summary>
    public Type ControllerType { get; }
    /// <summary>
    /// Gets the order
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Initializes a new instance with type and attribute
    /// </summary>
    /// <param name="type">Controller type</param>
    /// <param name="attribute">Usage attribute</param>
    public WorkSpaceControllerInfo(Type type, WorkSpaceControllerUsageAttribute attribute)
    {
        ControllerType = type;

        if (attribute != null)
        {
            Name = attribute.Name;
            if (!string.IsNullOrEmpty(attribute.DisplayName))
            {
                DisplayName = attribute.DisplayName;
            }
            else
            {
                DisplayName = Name;
            }

            IconKey = attribute.IconKey;
            Order = attribute.Order;
        }
        else
        {
            Name = DisplayName = type.Name;
        }
    }

    /// <summary>
    /// Initializes a new instance with type only
    /// </summary>
    /// <param name="type">Controller type</param>
    public WorkSpaceControllerInfo(Type type)
    {
        ControllerType = type;

        DisplayName = Name = type.Name;
    }
}