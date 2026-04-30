using System;

namespace Suity.Synchonizing.Preset;

/// <summary>
/// Implementation of IIndexSync for initialization mode
/// </summary>
public class InitializeIndexSync : MarshalByRefObject, IIndexSync
{
    #region IIndexSync

    public SyncMode Mode => SyncMode.Initialize;

    public SyncIntent Intent => SyncIntent.None;

    public int Count => 0;

    public int Index => 0;

    public object Value => null;

    public int SyncCount(int count) => count;

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None) => obj;

    public string SyncAttribute(string name, string value) => value;

    #endregion
}