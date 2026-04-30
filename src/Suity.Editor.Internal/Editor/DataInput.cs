using Suity.Editor.Analyzing;
using Suity.Editor.CodeRender;
using Suity.Editor.Selecting;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.Linq;

namespace Suity.Editor;

/// <summary>
/// Represents a data input for rendering, implementing interfaces for view display,
/// navigation, synchronization, and analysis.
/// </summary>
public class DataInput : IDataInputItem,
    IViewObject,
    ITextDisplay,
    IPreviewDisplay,
    IReference,
    ISyncPathObject,
    INavigable,
    IDataInput,
    ISupportAnalysis,
    IViewDoubleClickAction,
    IViewComment
{
    private DataInputList _parentList;

    private AssetSelection<IMaterial> _material = new();
    private AssetSelection<ICodeLibrary> _userCodeSource = new();
    private readonly UserCodeFilter _userCodeFilter = new();
    private bool _comment;

    /// <summary>
    /// Gets the parent list that contains this data input.
    /// </summary>
    public DataInputList ParentList => _parentList;

    /// <summary>
    /// Initializes a new instance of the <see cref="DataInput"/> class with default values.
    /// </summary>
    public DataInput()
    {
        _userCodeSource.Filter = _userCodeFilter;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataInput"/> class with the specified renderable ID.
    /// </summary>
    /// <param name="id">The renderable ID for this data input.</param>
    public DataInput(Guid id)
        : this()
    {
        RenderableId = id;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataInput"/> class with the specified ID, material, and workspace.
    /// </summary>
    /// <param name="id">The renderable ID for this data input.</param>
    /// <param name="material">The material asset key.</param>
    /// <param name="workSpace">The workspace for the user code source.</param>
    public DataInput(Guid id, string material, string workSpace)
        : this(id)
    {
        _material.SelectedKey = material;
        _userCodeSource.SelectedKey = workSpace;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DataInput"/> class by copying from an existing <see cref="IDataInput"/>.
    /// </summary>
    /// <param name="input">The data input to copy from.</param>
    public DataInput(IDataInput input)
        : this(input.RenderableId)
    {
        _material.Id = input.Material?.Id ?? Guid.Empty;
        _userCodeSource.SelectedKey = input.GetBuildConfig()?.WorkSpace;
    }

    /// <summary>
    /// Gets the target asset associated with this data input.
    /// </summary>
    public Asset Target => AssetManager.Instance.GetAsset(RenderableId);

    #region IViewObject

    /// <inheritdoc/>
    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        // Synchronize the renderable ID and related properties
        Guid id = RenderableId;
        sync.SyncId(ref id, context);

        // FullName is used for loading after publish
        if (id == Guid.Empty)
        {
            id = sync.Sync("FullName", RenderableId, SyncFlag.NotNull);
        }
        RenderableId = id;

        if (sync.IsGetter())
        {
            sync.Sync("FullName", AssetManager.Instance.GetAsset(RenderableId)?.AssetKey);
        }

        _material = sync.Sync("Material", _material, SyncFlag.NotNull);
        _userCodeSource = sync.Sync("UserCodeSource", _userCodeSource, SyncFlag.NotNull);
        _comment = sync.Sync("Comment", _comment, SyncFlag.NotNull, false);

        _userCodeSource.Filter = _userCodeFilter;
    }

    /// <inheritdoc/>
    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        // Configure inspector fields for material and user code source
        if (EditorObjectManager.Instance.GetObject(RenderableId) is not IDataInputOwner)
        {
            setup.InspectorField(_material, new ViewProperty("Material", "Material"));
            setup.InspectorField(_userCodeSource, new ViewProperty("UserCodeSource", "User Code Source"));
        }
    }

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    string ITextDisplay.DisplayText => (Renderable as Asset)?.DisplayText ?? RenderableId.ToString();

    /// <inheritdoc/>
    object ITextDisplay.DisplayIcon => (Renderable as Asset)?.Icon ?? CoreIconCache.DataGrid;

    /// <inheritdoc/>
    TextStatus ITextDisplay.DisplayStatus
    {
        get
        {
            if (Renderable is null)
            {
                return TextStatus.Error;
            }

            return TextStatus.Normal;
        }
    }

    #endregion

    #region IPreviewDisplay

    /// <inheritdoc/>
    string IPreviewDisplay.PreviewText
    {
        get
        {
            if (Target is IDataInputOwner owner)
            {
                return $"{owner.GetDataInputs().Count()} data source(s)";
            }
            else
            {
                string material = _material.DisplayText ?? string.Empty;
                string code = _userCodeSource.DisplayText ?? string.Empty;

                if (string.IsNullOrEmpty(material))
                {
                    return string.Empty;
                }

                if (!string.IsNullOrEmpty(code))
                {
                    return $"{material} ({code})";
                }
                else
                {
                    return material;
                }
            }
        }
    }

    /// <inheritdoc/>
    object IPreviewDisplay.PreviewIcon => _material.TargetAsset?.Icon;

    #endregion

    #region IReference

    /// <inheritdoc/>
    public void ReferenceSync(SyncPath path, IReferenceSync sync)
    {
        RenderableId = sync.SyncId(path, RenderableId, null);
    }

    #endregion

    #region INavigable

    /// <inheritdoc/>
    object INavigable.GetNavigationTarget()
    {
        return RenderableId;
    }

    #endregion

    #region IDataInput

    /// <summary>
    /// Gets or sets the renderable ID that identifies the target asset.
    /// </summary>
    public Guid RenderableId
    {
        get => _userCodeFilter.AffectedId;
        set => _userCodeFilter.AffectedId = value;
    }

    /// <summary>
    /// Gets the material associated with this data input.
    /// </summary>
    public IMaterial Material => _material.Target;

    /// <summary>
    /// Gets the build configuration from the user code source.
    /// </summary>
    public RenderConfig GetBuildConfig() => _userCodeSource.Target?.CreateBuildConfig();

    /// <summary>
    /// Gets the renderable object associated with this data input.
    /// </summary>
    public IRenderable Renderable => EditorObjectManager.Instance.GetObject(RenderableId) as IRenderable;

    #endregion

    #region AnalysisResult

    /// <summary>
    /// Gets or sets the analysis result for this data input.
    /// </summary>
    public AnalysisResult Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems related to this data input.
    /// </summary>
    /// <param name="problems">The collection to add problems to.</param>
    /// <param name="intent">The analysis intent.</param>
    public void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        if (Target is null)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, "Data resource missing"));
        }
    }

    #endregion

    #region IViewDoubleClickAction

    /// <inheritdoc/>
    void IViewDoubleClickAction.DoubleClick()
    {
        EditorUtility.GotoDefinition(RenderableId);
    }

    #endregion

    #region IViewComment

    /// <inheritdoc/>
    public bool CanComment => true;

    /// <inheritdoc/>
    public bool IsComment
    {
        get => _comment;
        set
        {
            if (_comment != value)
            {
                _comment = value;
            }
        }
    }

    #endregion

    /// <summary>
    /// Updates the parent list reference for this data input.
    /// </summary>
    /// <param name="list">The parent list to set.</param>
    internal void UpdateList(DataInputList list)
    {
        if (_parentList == list)
        {
            return;
        }
        _parentList = list;
    }

    /// <summary>
    /// Gets the synchronization path for this data input within its parent list.
    /// </summary>
    /// <returns>The sync path, or null if no parent list exists.</returns>
    public SyncPath GetPath()
    {
        var parent = _parentList?._parent;
        if (parent != null)
        {
            return parent.GetPath().Append(_parentList._propertyName).Append(_parentList.IndexOf(this));
        }
        else
        {
            return null;
        }
    }
}