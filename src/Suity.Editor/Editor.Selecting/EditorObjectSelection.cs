using Suity.Editor.Analyzing;
using Suity.Editor.Services;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;

namespace Suity.Editor.Selecting;

/// <summary>
/// Base class for editor object selections that provides selection functionality with reference tracking and navigation support.
/// </summary>
/// <typeparam name="TObject">The type of object being selected.</typeparam>
public abstract class EditorObjectSelection<TObject> : ISelection, IReference, INavigable, ISupportAnalysis
    where TObject : class
{
    internal readonly EditorObjectRef<EditorObject> _ref = new();
    internal string _key = string.Empty;

    /// <summary>
    /// Initializes a new instance of the EditorObjectSelection class.
    /// </summary>
    public EditorObjectSelection()
    {
    }

    /// <summary>
    /// Gets the display text for the selected object.
    /// </summary>
    public virtual string DisplayText => SelectedKey;

    /// <summary>
    /// Gets or sets the unique identifier of the selected object.
    /// </summary>
    public Guid Id
    {
        get => _ref.Id;
        set
        {
            if (value != _ref.Id)
            {
                _ref.Id = value;
                _key = ResolveKey(value);
            }
        }
    }

    /// <summary>
    /// Gets or sets whether listening to target updates is enabled.
    /// </summary>
    public bool ListenEnabled
    {
        get => _ref.ListenEnabled;
        set
        {
            _ref.ListenEnabled = value;
        }
    }

    #region ISelection

    /// <summary>
    /// Gets whether the selection currently has a valid target.
    /// </summary>
    public virtual bool IsValid => Target != null;

    /// <summary>
    /// Gets or sets the selected key used to identify the selected object.
    /// </summary>
    public string SelectedKey
    {
        get
        {
            return ResolveKey(_ref.Id);
        }
        set
        {
            value ??= string.Empty;

            // When cross-document copy-paste, need to re-resolve Id once

            //if (value != _key)
            //{
            _key = value;
            _ref.Id = ResolveId(_key);
            //}
        }
    }

    /// <summary>
    /// Gets the selection list for this selection.
    /// </summary>
    /// <returns>The selection list.</returns>
    public abstract ISelectionList GetList();

    #endregion

    #region INavigable

    /// <inheritdoc />
    object INavigable.GetNavigationTarget() => _ref.Id;

    #endregion

    #region Sync

    /// <summary>
    /// Synchronizes the selection data with the given property sync operation.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The sync context.</param>
    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
        sync.SyncObjectRef(_ref, ref _key, ResolveId, ResolveKey, context);

        if (sync.Intent == SyncIntent.Clone)
        {
            _key = sync.Sync("Key", _key, SyncFlag.AttributeMode | SyncFlag.ByRef);

            // When cross-document copy-paste, need to re-resolve Id once

            if (sync.IsSetter()) // && _ref.Id == Guid.Empty)
            {
                if (string.IsNullOrEmpty(_key))
                {
                    _key = ResolveKey(_ref.Id);
                }
                else
                {
                    _ref.Id = ResolveId(_key);
                }
            }
        }
    }

    #endregion

    /// <summary>
    /// Synchronizes the reference with the given reference sync operation.
    /// </summary>
    /// <param name="path">The sync path.</param>
    /// <param name="sync">The reference sync object.</param>
    public virtual void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        if (_ref.Id == Guid.Empty && !string.IsNullOrEmpty(_key))
        {
            _ref.Id = ResolveId(_key);
        }

        Guid id = sync.SyncId(path, _ref.Id, null);
        if (id != _ref.Id)
        {
            _ref.Id = id;
            _key = ResolveKey(id);
        }
    }

    /// <summary>
    /// Gets the target object of the selection.
    /// </summary>
    public virtual TObject Target => _ref.Target as TObject;
    
    /// <summary>
    /// Gets the target EditorObject of the selection.
    /// </summary>
    public EditorObject TargetObject => _ref.Target;

    /// <inheritdoc />
    AnalysisResult ISupportAnalysis.Analysis { get; set; }

    /// <inheritdoc />
    void ISupportAnalysis.CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        EditorServices.AnalysisService.CollecctProblems(this, problems, intent);
    }

    /// <summary>
    /// Resolves the unique identifier from the given key.
    /// </summary>
    /// <param name="key">The key to resolve.</param>
    /// <returns>The resolved unique identifier.</returns>
    protected internal abstract Guid ResolveId(string key);

    /// <summary>
    /// Resolves the key from the given unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier to resolve.</param>
    /// <returns>The resolved key.</returns>
    protected internal virtual string ResolveKey(Guid id)
    {
        return EditorObjectManager.Instance.GetObject(id)?.Name;
    }

    /// <summary>
    /// Attempts to repair the identifier by resolving it from the key.
    /// </summary>
    /// <returns>True if the repair was successful; otherwise, false.</returns>
    protected bool RepairId()
    {
        if (!string.IsNullOrEmpty(_key))
        {
            Guid id = ResolveId(_key);
            if (id != Guid.Empty)
            {
                _ref.Id = id;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc />
    public override string ToString() => _ref.ToString();
}