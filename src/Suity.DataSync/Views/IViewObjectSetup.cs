using Suity.NodeQuery;
using Suity.Synchonizing;
using System;
using System.Collections.Generic;

namespace Suity.Views;

public interface IViewObjectSetup : ISyncContext
{
    /// <summary>
    /// Obtain whether the target type supports the current editing type
    /// </summary>
    /// <param name="type">Target type</param>
    /// <returns>If the target type matches, return true; otherwise, return false.</returns>
    bool IsTypeSupported(Type type);

    bool IsViewIdSupported(int viewId);

    void AddField(Type type, ViewProperty property);

    IEnumerable<object> GetObjects();

    INodeReader Styles { get; }
}