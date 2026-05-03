using Suity.Drawing;
using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using Suity.Views.Named;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Editable asset document with named items and analysis support.
/// </summary>
[KnowledgeGenerate(VectorEnabled = true, FeatureEnabled = false)]
public abstract class SNamedDocument : SAssetDocument,
    ISupportAnalysis,
    IMemberContainer
{
    private readonly SNamedRootCollection _items;
    private INamedUsingList _usingList;
    private INamedRenderTargetList _renderTargetList;
    private string _description;
    private AssetSelection<ImageAsset> _iconSelection = new();
    private Color _color = Color.Empty;

    public SNamedDocument()
    {
        _items = new SNamedRootCollection(this);
    }

    public SNamedDocument(AssetBuilder builder)
        : base(builder)
    {
        _items = new SNamedRootCollection(this, builder);
    }

    /// <summary>
    /// Gets or sets the description of the document.
    /// </summary>
    public string Description
    {
        get => _description;
        set
        {
            value ??= string.Empty;

            if (_description == value)
            {
                return;
            }

            _description = value;

            AssetBuilder?.SetDescription(value);
        }
    }

    /// <summary>
    /// Gets or sets the icon selection for the document.
    /// </summary>
    protected AssetSelection<ImageAsset> IconSelection
    {
        get => _iconSelection;
        set
        {
            value ??= new AssetSelection<ImageAsset>();

            if (_iconSelection.Id != value.Id)
            {
                _iconSelection = value;
                AssetBuilder?.SetIconId(_iconSelection.Id);

                RaiseIconChanged();
            }
        }
    }

    public override ImageDef DefaultIcon
        => AssetBuilder?.TargetAsset?.Icon ??
        this.GetType().ToDisplayIcon() ??
        CoreIconCache.DataGrid;

    public override ImageDef Icon => _iconSelection.Target?.Icon ?? DefaultIcon;

    /// <summary>
    /// Gets or sets the color for the document.
    /// </summary>
    public Color Color
    {
        get => _color;
        set
        {
            if (_color == value)
            {
                return;
            }

            _color = value;
            AssetBuilder.SetColor(_color != Color.Empty ? _color : (Color?)null);
        }
    }

    /// <summary>
    /// Gets the root collection of named items.
    /// </summary>
    public SNamedRootCollection ItemCollection => _items;

    /// <summary>
    /// Gets or sets whether to show render targets in the view.
    /// </summary>
    protected bool ShowRenderTargets { get; set; } = true;

    /// <summary>
    /// Gets or sets whether to analyze namespace.
    /// </summary>
    protected bool AnalyzeNameSpace { get; set; }

    #region ISyncObject IViewObject

    internal override void OnSyncInternal(IPropertySync sync, ISyncContext context)
    {
        ImportName = sync.Sync("ImportName", ImportName);
        NameSpace = sync.Sync("NameSpace", NameSpace);

        Description = sync.Sync("Description", Description);
        IconSelection = sync.Sync("Icon", IconSelection, SyncFlag.NotNull);
        Color = sync.Sync("Color", Color, SyncFlag.None, Color.Empty);

        sync.Sync(_items.FieldName, _items, SyncFlag.GetOnly | SyncFlag.Element);

        if (sync.Intent == SyncIntent.View)
        {
            sync.Sync("Usings", _usingList, SyncFlag.GetOnly | SyncFlag.ByRef | SyncFlag.NoSerialize);

            if (ShowRenderTargets && Analysis?.RenderTargets.Count > 0 && _renderTargetList != null)
            {
                sync.Sync("RenderTargets", _renderTargetList, SyncFlag.GetOnly | SyncFlag.ByRef | SyncFlag.NoSerialize);
            }
        }

        OnSync(sync, context);
    }

    internal override void OnSetupViewInternal(IViewObjectSetup setup)
    {
        setup.InspectorField(NameSpace, new ViewProperty("NameSpace", "Namespace"));

        OnSetupView(setup);
        OnSetupViewAppearance(setup);
        OnSetupViewContent(setup);

        if (setup.IsViewIdSupported(ViewIds.MainTreeView))
        {
            if (Analysis != null)
            {
                //if (_usingList is null && RefHost != null)
                //{
                //    var files = ReferenceManager.Current.GetDependencies(RefHost)
                //        .Select(id => EditorObjectManager.Instance.GetObject(id))
                //        .SkipNull()
                //        .Select(o => o.GetStorageLocation())
                //        .Where(o => o != FileName)
                //        .SkipNull()
                //        .Distinct().ToArray();

                //    if (files.Length > 0)
                //    {
                //        _usingList = NamedExternal._external.CreateUsingList("Using", files, RefHost);
                //    }
                //}

                if (_usingList is null)
                {
                    if (Analysis.DependencyObjects.Count > 0)
                    {
                        var ids = Analysis.DependencyFileAssets;
                        _usingList = NamedExternal._external.CreateUsingList("Using", ids);
                    }
                }

                if (_usingList != null)
                {
                    var prop = new ViewProperty("Usings", "Using")
                    {
                        ReadOnly = true,
                        Expand = false,
                        Status = TextStatus.Reference
                    };
                    setup.MainTreeViewField(_usingList, prop);
                }
            }

            if (ShowRenderTargets && Analysis?.RenderTargets.Count > 0)
            {
                _renderTargetList ??= NamedExternal._external.CreateRenderTargetList(this);
                var prop = new ViewProperty("RenderTargets", "Render Targets")
                {
                    ReadOnly = true,
                    Expand = false,
                    Status = TextStatus.Reference
                };
                setup.MainTreeViewField(_renderTargetList, prop);
            }
        }
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        setup.MainTreeViewField(_items, new ViewProperty(_items.FieldName, _items.FieldDescription));
    }

    protected virtual void OnSetupViewAppearance(IViewObjectSetup setup)
    {
        setup.Label(new ViewProperty("#Appearance", "Appearance", CoreIconCache.View));

        setup.InspectorField(_description, new ViewProperty("Description", "Description"));
        setup.InspectorField(_iconSelection, new ViewProperty("Icon", "Icon"));
        setup.InspectorField(_color, new ViewProperty("Color", "Color", CoreIconCache.Color)
            .WithColor(_color != Color.Empty ? _color : (Color?)null));
    }

    protected virtual void OnSetupViewContent(IViewObjectSetup setup)
    {
    }

    #endregion

    #region IMemberContainer

    int IMemberContainer.MemberCount => _items.Count;

    IEnumerable<IMember> IMemberContainer.Members => _items.AllItemsSorted.OfType<IMember>();

    IMember IMemberContainer.GetMember(string name)
    {
        return _items.GetItemAll(name) as IMember;
    }

    #endregion

    #region ISupportAnalysis

    private AnalysisResult _analysisResult;

    /// <summary>
    /// Gets or sets the analysis result for the document.
    /// </summary>
    public AnalysisResult Analysis
    {
        get => _analysisResult;
        set
        {
            _analysisResult = value;
            AssetBuilder?.SetProblem(_analysisResult?.Problems);

            _usingList = null;
            _renderTargetList = null;
        }
    }

    /// <summary>
    /// Collects analysis problems for the document.
    /// </summary>
    /// <param name="problems">The analysis problem collector.</param>
    /// <param name="intent">The analysis intent.</param>
    public virtual void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        if (AnalyzeNameSpace && string.IsNullOrEmpty(NameSpace))
        {
            problems.Add(new AnalysisProblem(TextStatus.Warning, L("Namespace not filled")));
        }
    }

    #endregion

    #region Virtual

    /// <summary>
    /// Called when the document is reset.
    /// </summary>
    protected internal override void OnReset()
    {
        // Detach AssetBuilder
        base.OnReset();

        _items.Clear();
    }

    /// <summary>
    /// Called when the document is destroyed.
    /// </summary>
    protected internal override void OnDestroy()
    {
        // Detach AssetBuilder
        base.OnDestroy();

        _items.Clear();
    }

    /// <summary>
    /// Loads the document from storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <param name="loaderObject">The loader object.</param>
    /// <returns>True if load was successful.</returns>
    protected internal override bool LoadDocument(IStorageItem op, object loaderObject, DocumentLoadingIntent intent)
    {
        if (loaderObject is not INodeReader reader || !reader.Exist)
        {
            reader = XmlNodeReader.FromStream(op.GetInputStream(), false);
        }

        if (!reader.Exist)
        {
            return false;
        }

        Serializer.Deserialize(this, reader, SyncTypeResolver, this);

        return true;
    }

    /// <summary>
    /// Saves the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if save was successful.</returns>
    protected internal override bool SaveDocument(IStorageItem op)
    {
        var writer = new XmlNodeWriter("SuityAsset");
        writer.SetAttribute("version", "1.0");
        writer.SetAttribute("format", Format.FormatName);

        Serializer.Serialize(this, writer, SyncTypeResolver, this);
        writer.SaveToStream(op.GetOutputStream());
        return true;
    }

    /// <summary>
    /// Exports the document to storage.
    /// </summary>
    /// <param name="op">The storage item.</param>
    /// <returns>True if export was successful.</returns>
    protected internal override bool ExportDocument(IStorageItem op)
    {
        var writer = new XmlNodeWriter("SuityAsset");
        writer.SetAttribute("version", "1.0");
        writer.SetAttribute("format", Format.FormatName);

        Serializer.Serialize(this, writer, SyncTypeResolver, this, SyncIntent.DataExport);
        writer.SaveToStream(op.GetOutputStream());
        return true;
    }

    public override object GetElement(string name) => _items.GetItemAll(name);

    /// <summary>
    /// Called when an item is added to the collection.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="item">The added item.</param>
    /// <param name="isNew">Whether the item is new.</param>
    protected internal virtual void OnItemAdded(SNamedRootCollection collection, NamedItem item, bool isNew)
    {
    }

    /// <summary>
    /// Called when an item is removed from the collection.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="item">The removed item.</param>
    protected internal virtual void OnItemRemoved(SNamedRootCollection collection, NamedItem item)
    {
    }

    /// <summary>
    /// Called when an item is renamed.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="item">The renamed item.</param>
    /// <param name="oldName">The old name.</param>
    protected internal virtual void OnItemRenamed(SNamedRootCollection collection, NamedItem item, string oldName)
    {
    }

    /// <summary>
    /// Called to create a default item.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <returns>The created item, or null.</returns>
    protected internal virtual NamedItem OnCreateDefaultItem(SNamedRootCollection collection) => null;

    /// <summary>
    /// Called to create items using GUI.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="type">The type to create.</param>
    /// <returns>The created items.</returns>
    protected internal virtual async Task<NamedItem[]> OnGuiCreateItems(SNamedRootCollection collection, Type type)
    {
        if (type is null)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Type is not specified.");
            return null;
        }

        try
        {
            NamedItem item = Activator.CreateInstance(type) as NamedItem;
            if (item is null)
            {
                await DialogUtility.ShowMessageBoxAsyncL("Type is not supported.");
            }

            return [item];
        }
        catch (Exception)
        {
            await DialogUtility.ShowMessageBoxAsyncL("Type is not supported.");
            return null;
        }
    }


    /// <summary>
    /// Called to configure a new item via GUI.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="parentNode">The parent node.</param>
    /// <param name="item">The item to configure.</param>
    /// <returns>True if configuration was successful.</returns>
    protected internal virtual Task<bool> OnGuiConfigNewItem(SNamedRootCollection collection, INamedNode parentNode, NamedItem item)
    {
        return Task.FromResult<bool>(true);
    }

    /// <summary>
    /// Checks if a drop-in value is valid.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="value">The value to check.</param>
    /// <returns>True if the value is valid.</returns>
    protected internal virtual bool OnDropInCheck(SNamedRootCollection collection, object value)
    {
        return collection.IsItemTypeMatched(value);
    }

    /// <summary>
    /// Converts a drop-in value.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="value">The value to convert.</param>
    /// <returns>The converted value.</returns>
    protected internal virtual object OnDropInConvert(SNamedRootCollection collection, object value)
    {
        return value;
    }

    /// <summary>
    /// Gets a suggested name for a new item.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="prefix">The name prefix.</param>
    /// <param name="digiLen">The number of digits.</param>
    /// <returns>The suggested name, or null.</returns>
    protected internal virtual string OnGetSuggestedName(SNamedRootCollection collection, string prefix, int digiLen = 2) => null;

    /// <summary>
    /// Resolves a conflicting name.
    /// </summary>
    /// <param name="collection">The root collection.</param>
    /// <param name="name">The conflicting name.</param>
    /// <returns>The resolved name, or null.</returns>
    protected internal virtual string OnResolveConflictName(SNamedRootCollection collection, string name) => null;

    /// <summary>
    /// Handles inspection of objects.
    /// </summary>
    /// <param name="objs">The objects to inspect.</param>
    /// <param name="context">The inspector context.</param>
    /// <returns>True if handled.</returns>
    public virtual bool HandleInspect(IEnumerable<object> objs, IInspectorContext context)
    {
        return false;
    }

    /// <summary>
    /// Creates a new group.
    /// </summary>
    /// <returns>The created group.</returns>
    public virtual SNamedGroup CreateGroup() => new();

    /// <summary>
    /// Searches for text within the document.
    /// </summary>
    /// <param name="context">The validation context.</param>
    /// <param name="findStr">The search string.</param>
    /// <param name="findOption">The search option.</param>
    public override void Find(ValidationContext context, string findStr, Synchonizing.Core.SearchOption findOption)
    {
        base.Find(context, findStr, findOption);

        if (Validator.Compare(_description, findStr, findOption))
        {
            context.Report(_description, this);
        }
    }

    #endregion
}

/// <summary>
/// Generic SNamedDocument with a specific asset builder type.
/// </summary>
public abstract class SNamedDocument<TAssetBuilder> : SNamedDocument
    where TAssetBuilder : AssetBuilder, new()
{
    public SNamedDocument()
        : base(new TAssetBuilder())
    {
    }

    protected SNamedDocument(TAssetBuilder builder)
        : base(builder)
    {
    }

    protected internal new TAssetBuilder AssetBuilder => (TAssetBuilder)base.AssetBuilder;
}