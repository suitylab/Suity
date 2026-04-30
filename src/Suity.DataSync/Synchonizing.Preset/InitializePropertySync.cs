using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IPropertySync for initialization mode
/// </summary>
public sealed class InitializePropertySync : MarshalByRefObject, IPropertySync
{
    public static readonly InitializePropertySync Instance = new();

    private InitializePropertySync()
    {
    }

    #region IPropertySync

    public SyncMode Mode => SyncMode.Initialize;

    public SyncIntent Intent => SyncIntent.None;

    public string Name => null;

    public IEnumerable<string> Names => [];

    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null) => obj;

    #endregion
}