using Suity.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Collector for data inputs.
/// </summary>
public class DataInputCollector : IDataInputOwner
{
    private readonly Dictionary<Guid, IDataInput> _dataInputs = new Dictionary<Guid, IDataInput>();

    /// <summary>
    /// Creates an empty data input collector.
    /// </summary>
    public DataInputCollector()
    { }

    /// <summary>
    /// Creates a data input collector from an owner.
    /// </summary>
    /// <param name="owner">The data input owner.</param>
    public DataInputCollector(IDataInputOwner owner)
    {
        AddDataInputOwner(owner);
    }

    /// <summary>
    /// Creates a data input collector from a collection of inputs.
    /// </summary>
    /// <param name="inputs">The data inputs.</param>
    public DataInputCollector(IEnumerable<IDataInput> inputs)
    {
        if (inputs is null)
        {
            throw new ArgumentNullException(nameof(inputs));
        }

        foreach (var input in inputs)
        {
            AddDataInput(input);
        }
    }

    /// <summary>
    /// Adds a data input to the collector.
    /// </summary>
    /// <param name="input">The data input.</param>
    /// <returns>True if added successfully.</returns>
    public bool AddDataInput(IDataInput input)
    {
        if (input is null || input.RenderableId == Guid.Empty)
        {
            return false;
        }

        if (_dataInputs.ContainsKey(input.RenderableId))
        {
            return false;
        }

        _dataInputs.Add(input.RenderableId, input);

        var obj = EditorObjectManager.Instance.GetObject(input.RenderableId);

        if (obj is IDataInputOwner owner)
        {
            AddDataInputOwner(owner);
        }

        return true;
    }

    /// <summary>
    /// Adds all data inputs from an owner.
    /// </summary>
    /// <param name="owner">The data input owner.</param>
    public void AddDataInputOwner(IDataInputOwner owner)
    {
        if (owner is null)
        {
            throw new ArgumentNullException(nameof(owner));
        }

        foreach (var input in owner.GetDataInputs())
        {
            AddDataInput(input);
        }
    }

    /// <summary>
    /// Gets a data input by id.
    /// </summary>
    /// <param name="id">The id.</param>
    /// <returns>The data input, or null if not found.</returns>
    public IDataInput GetDataInput(Guid id) => _dataInputs.GetValueSafe(id);

    /// <summary>
    /// Gets a data input by asset key.
    /// </summary>
    /// <param name="assetKey">The asset key.</param>
    /// <returns>The data input, or null if not found.</returns>
    public IDataInput GetDataInput(string assetKey)
    {
        Guid id = GlobalIdResolver.Resolve(assetKey);
        if (id != Guid.Empty)
        {
            return GetDataInput(id);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Clears all data inputs.
    /// </summary>
    public void Clear() => _dataInputs.Clear();

    #region IDataInputOwner

    /// <inheritdoc/>
    public IEnumerable<IDataInput> GetDataInputs()
    {
        return _dataInputs.Values.Where(o => EditorObjectManager.Instance.GetObject(o.RenderableId) is not IDataInputOwner);
    }

    /// <inheritdoc/>
    public bool ContainsDataInput(Guid id)
    {
        if (id == Guid.Empty)
        {
            return false;
        }

        return _dataInputs.ContainsKey(id);
    }

    #endregion
}