using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using System;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.UndoRedos;

/// <summary>
/// An undo/redo action that captures a snapshot of a sync object for property-level changes.
/// </summary>
public class SnapshotObjectUndoAction : UndoRedoAction
{
    private readonly ISyncObject _obj;

    private readonly ISyncTypeResolver _resolver;
    private readonly IServiceProvider _provider;

    private readonly ISyncObject _before;
    private ISyncObject _after;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotObjectUndoAction"/> class.
    /// </summary>
    /// <param name="obj">The sync object to snapshot.</param>
    /// <param name="resolver">Optional type resolver for cloning.</param>
    /// <param name="provider">Optional service provider for cloning.</param>
    /// <param name="postViewAction">Optional action to execute after view update.</param>
    public SnapshotObjectUndoAction(ISyncObject obj, 
        ISyncTypeResolver resolver = null, IServiceProvider provider = null, Action postViewAction = null)
    {
        _obj = obj ?? throw new ArgumentNullException();
        _resolver = resolver;
        _provider = provider;
        PostViewAction = postViewAction;

        _before = Cloner.Clone(_obj, _resolver, _provider);
    }

    /// <summary>
    /// Gets the name of this action.
    /// </summary>
    public override string Name => L("Edit Object");

    /// <summary>
    /// Executes the action by restoring the after state.
    /// </summary>
    public override void Do()
    {
        if (_after != null)
        {
            Cloner.CloneProperty(_after, _obj, _resolver, _provider);
        }

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Undoes the action by restoring the before state.
    /// </summary>
    public override void Undo()
    {
        if (_after == null)
        {
            _after = Cloner.Clone(_obj, _resolver, _provider);
        }

        Cloner.CloneProperty(_before, _obj, _resolver, _provider);

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }
}

/// <summary>
/// An undo/redo action that captures a snapshot of a sync list for list-level changes.
/// </summary>
public class SnapshotListUndoAction : UndoRedoAction
{
    private readonly ISyncList _list;

    private readonly ISyncTypeResolver _resolver;
    private readonly IServiceProvider _provider;

    private readonly ISyncList _before;
    private ISyncList _after;

    /// <summary>
    /// Initializes a new instance of the <see cref="SnapshotListUndoAction"/> class.
    /// </summary>
    /// <param name="list">The sync list to snapshot.</param>
    /// <param name="resolver">Optional type resolver for cloning.</param>
    /// <param name="provider">Optional service provider for cloning.</param>
    /// <param name="postViewAction">Optional action to execute after view update.</param>
    public SnapshotListUndoAction(ISyncList list, 
        ISyncTypeResolver resolver = null, IServiceProvider provider = null, Action postViewAction = null)
    {
        _list = list ?? throw new ArgumentNullException();
        _resolver = resolver;
        _provider = provider;
        PostViewAction = postViewAction;

        _before = Cloner.Clone(_list, _resolver, _provider);
    }

    /// <summary>
    /// Gets the name of this action.
    /// </summary>
    public override string Name => L("Edit List");

    /// <summary>
    /// Executes the action by restoring the after state.
    /// </summary>
    public override void Do()
    {
        if (_after != null)
        {
            Cloner.CloneProperty(_after, _list, _resolver, _provider);
        }

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Undoes the action by restoring the before state.
    /// </summary>
    public override void Undo()
    {
        if (_after == null)
        {
            _after = Cloner.Clone(_list, _resolver, _provider);
        }

        Cloner.CloneProperty(_before, _list, _resolver, _provider);

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }
}

/// <summary>
/// An undo/redo action that captures a raw snapshot of an object using serialization for deep changes.
/// </summary>
public class RawSnapshotUndoAction : UndoRedoAction
{
    private readonly object _obj;

    private readonly ISyncTypeResolver _resolver;
    private readonly IServiceProvider _provider;

    private readonly RawNode _before;
    private RawNode _after;

    /// <summary>
    /// Initializes a new instance of the <see cref="RawSnapshotUndoAction"/> class.
    /// </summary>
    /// <param name="obj">The object to snapshot.</param>
    /// <param name="resolver">Optional type resolver for serialization.</param>
    /// <param name="provider">Optional service provider for serialization.</param>
    /// <param name="postViewAction">Optional action to execute after view update.</param>
    public RawSnapshotUndoAction(object obj,
        ISyncTypeResolver resolver = null, IServiceProvider provider = null, Action postViewAction = null)
    {
        _obj = obj ?? throw new ArgumentNullException();
        _resolver = resolver;
        _provider = provider;
        PostViewAction = postViewAction;

        _before = new RawNode();
        Serializer.Serialize(_obj, new RawNodeWriter(_before));
    }

    /// <summary>
    /// Gets the name of this action.
    /// </summary>
    public override string Name => L("Edit Object");

    /// <summary>
    /// Executes the action by restoring the after state.
    /// </summary>
    public override void Do()
    {
        if (_after != null)
        {
            Serializer.Deserialize(_obj, _after);
        }

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }

    /// <summary>
    /// Undoes the action by restoring the before state.
    /// </summary>
    public override void Undo()
    {
        if (_after is null)
        {
            _after = new RawNode();
            Serializer.Serialize(_obj, new RawNodeWriter(_after));
        }

        Serializer.Deserialize(_obj, _before);

        if (PostViewAction is { } viewAction)
        {
            try
            {
                viewAction();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
    }
}