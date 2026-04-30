using System;
using System.Collections.Generic;

namespace Suity.Editor.WorkSpaces;

/// <summary>
/// External abstraction for workspace operations
/// </summary>
internal abstract class WorkSpacesExternal
{
    /// <summary>
    /// Current external instance
    /// </summary>
    internal static WorkSpacesExternal _external;

    /// <summary>
    /// Gets all registered controller infos
    /// </summary>
    public abstract WorkSpaceControllerInfo[] ControllerInfos { get; }

    /// <summary>
    /// Gets controller info by name
    /// </summary>
    /// <param name="name">Controller name</param>
    /// <returns>Controller info</returns>
    public abstract WorkSpaceControllerInfo GetControllerInfo(string name);

    /// <summary>
    /// Gets controller info by type
    /// </summary>
    /// <param name="type">Controller type</param>
    /// <returns>Controller info</returns>
    public abstract WorkSpaceControllerInfo GetControllerInfo(Type type);

    /// <summary>
    /// Gets controller info by generic type
    /// </summary>
    /// <typeparam name="T">Controller type</typeparam>
    /// <returns>Controller info</returns>
    public abstract WorkSpaceControllerInfo GetControllerInfo<T>() where T : WorkSpaceController;
}