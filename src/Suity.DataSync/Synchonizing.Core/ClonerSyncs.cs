using Suity.Collections;
using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Synchonizing.Core;

internal delegate object CloneElementCreate(Type type, object parameter);

internal delegate void CloneElementClone(object objFrom, object objTo);

internal class ClonePropertySync(Dictionary<string, SyncValueInfo> values) : MarshalByRefObject, IPropertySync
{
    private readonly Dictionary<string, SyncValueInfo> _values = values ?? throw new ArgumentNullException(nameof(values));

    public CloneElementCreate Creater;
    public CloneElementClone Cloner;

    public SyncMode Mode => SyncMode.SetAll;

    public SyncIntent Intent => SyncIntent.Clone;

    public string Name => null;

    public IEnumerable<string> Names => _values.Keys.Where(str => !str.StartsWith("@"));

    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        bool readOnly = (flag & SyncFlag.GetOnly) == SyncFlag.GetOnly;
        bool byRef = (flag & SyncFlag.ByRef) == SyncFlag.ByRef;
        bool notNull = (flag & SyncFlag.NotNull) == SyncFlag.NotNull;

        if ((flag & SyncFlag.AttributeMode) == SyncFlag.AttributeMode)
        {
            name = "@" + name;
        }

        SyncValueInfo info = _values.GetValueSafe(name);
        if (info is null)
        {
            return readOnly || notNull ? obj : defaultValue;
        }

        object resultObj = null;

        if (readOnly)
        {
            resultObj = obj;
        }
        else
        {
            if (byRef)
            {
                resultObj = info.Value;
            }
            else
            {
                resultObj = info.Value != null ? Creater(info.Value.GetType(), info.Value) : null;
            }
        }

        if (resultObj is T tObj)
        {
            if (!byRef)
            {
                Cloner(info.Value, resultObj);
            }

            return tObj;
        }
        else
        {
            T noResult = readOnly ? obj : defaultValue;
            if (noResult == null && notNull)
            {
                noResult = obj;
            }

            return noResult;
        }
    }
}

internal class CloneIndexSync : MarshalByRefObject, IIndexSync
{
    private readonly List<SyncValueInfo> _values;
    private readonly Dictionary<string, string> _attributes;

    public CloneElementCreate Creater;
    public CloneElementClone Cloner;

    public CloneIndexSync(List<SyncValueInfo> values, Dictionary<string, string> attributes)
    {
        _values = values ?? throw new ArgumentNullException(nameof(values));
        _attributes = attributes ?? throw new ArgumentNullException(nameof(attributes));
    }

    public SyncMode Mode => SyncMode.SetAll;
    public SyncIntent Intent => SyncIntent.Clone;
    public int Count => _values.Count;
    public int Index => -1;
    public object Value => null;

    public int SyncCount(int count) => _values.Count;

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        bool readOnly = (flag & SyncFlag.GetOnly) == SyncFlag.GetOnly;
        bool byRef = (flag & SyncFlag.ByRef) == SyncFlag.ByRef;
        bool notNull = (flag & SyncFlag.NotNull) == SyncFlag.NotNull;

        SyncValueInfo info = _values.GetListItemSafe(index);
        if (info is null)
        {
            return readOnly || notNull ? obj : default;
        }

        object result = null;

        if (readOnly)
        {
            result = obj;
        }
        else
        {
            if (byRef)
            {
                result = info.Value;
            }
            else
            {
                result = info.Value != null ? Creater(info.Value.GetType(), info.Value) : null;
            }
        }

        if (result is T tResult)
        {
            if (!byRef)
            {
                Cloner(info.Value, result);
            }

            return tResult;
        }
        else
        {
            T noResult = readOnly ? obj : default;
            if (noResult == null && notNull)
            {
                noResult = obj;
            }

            return noResult;
        }
    }

    public string SyncAttribute(string name, string value)
    {
        if (!string.IsNullOrEmpty(name))
        {
            if (_attributes.TryGetValue(name, out string result))
            {
                return result;
            }
            else
            {
                return null;
            }
        }
        else
        {
            return null;
        }
    }
}