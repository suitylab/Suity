using System;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// Workspace controller attribute
/// </summary>
[System.AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class WorkSpaceControllerUsageAttribute : Attribute
{
    /// <summary>
    /// Gets the controller name
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    public string DisplayName { get; set; }

    /// <summary>
    /// Gets or sets the icon key
    /// </summary>
    public string IconKey { get; set; }

    /// <summary>
    /// Gets or sets the order
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// Initializes a new instance of <see cref="WorkSpaceControllerUsageAttribute"/>
    /// </summary>
    /// <param name="name">Controller name</param>
    public WorkSpaceControllerUsageAttribute(string name)
    {
        Name = name;
    }
}