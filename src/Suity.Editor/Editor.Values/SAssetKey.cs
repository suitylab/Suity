using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Analyzing;
using Suity.Editor.Selecting;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using Suity.Editor.Services;

namespace Suity.Editor.Values;

/// <summary>
/// Represents an asset key that references an asset.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Asset Key", Icon = "*CoreIcon|Asset")]
[NativeAlias]
public class SAssetKey : SItem,
    ISelection,
    ISyncObject,
    ISupportAnalysis,
    IHasAsset,
    IHasId,
    ITextDisplay
{
    private IdAssetSelection _selection = new();

    public SAssetKey()
    { }

    public SAssetKey(TypeDefinition inputType)
        : base(inputType)
    {
        OnInputTypeChanged();
    }

    public SAssetKey(TypeDefinition inputType, string key)
        : base(inputType)
    {
        OnInputTypeChanged();
        _selection.SelectedKey = key;
    }

    public SAssetKey(TypeDefinition inputType, Guid id)
       : base(inputType)
    {
        OnInputTypeChanged();
        _selection.Id = id;
    }

    /// <summary>
    /// Gets or sets the selected key.
    /// </summary>
    public virtual string SelectedKey
    {
        get => _selection.SelectedKey;
        set => _selection.SelectedKey = value;
    }


    /// <summary>
    /// Gets the target asset referenced by this key.
    /// </summary>
    public virtual Asset TargetAsset => _selection.TargetAsset;

    /// <summary>
    /// Gets the description text.
    /// </summary>
    public virtual string DescriptionText => _selection.ToString();

    /// <summary>
    /// Gets the selection list.
    /// </summary>
    ISelectionList ISelection.GetSelectionList() => _selection.GetSelectionList();

    /// <summary>
    /// Gets whether the selection is valid.
    /// </summary>
    public virtual bool IsValid => _selection.IsValid;

    /// <summary>
    /// Gets whether the asset is valid.
    /// </summary>
    /// <param name="asset">The asset to validate.</param>
    public virtual bool GetIsValid(Asset asset) => _selection.GetIsValid(asset);

    protected override void OnInputTypeChanged()
    {
        DAssetLink assetLink = AssetManager.Instance.GetAsset<DAssetLink>(InputType?.Target?.Id ?? Guid.Empty, this.GetAssetFilter());
        _selection.UpdateContentTypeId(assetLink?.Id ?? Guid.Empty);
    }

    #region Comparison

    public override bool ValueEquals(object other)
    {
        if (other is SAssetKey sAssetKey)
        {
            return (_selection?.Id ?? Guid.Empty) == (sAssetKey._selection?.Id ?? Guid.Empty);
        }

        return false;
    }

    public override bool GetIsBlank() => _selection.Id == Guid.Empty;

    #endregion

    #region IHasId

    /// <summary>
    /// Gets or sets the target ID.
    /// </summary>
    public Guid TargetId
    {
        get => _selection.Id;
        set => _selection.Id = value;
    }

    /// <summary>
    /// Gets the ID.
    /// </summary>
    public Guid Id => _selection.Id;

    #endregion

    #region ISyncObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.IsSetter())
        {
            if (sync.SyncSetTypeDefinition(Attribute_InputType, InputType, out TypeDefinition newInputType, out string newTypeId))
            {
                InputType = newInputType;
            }
        }
        else
        {
            sync.SyncGetTypeDefinition(Attribute_InputType, InputType);
        }

        _selection = sync.Sync("Value", _selection);
        if (sync.IsSetterOf("Value"))
        {
            DAssetLink assetLink = AssetManager.Instance.GetAsset<DAssetLink>(InputType?.Target?.Id ?? Guid.Empty, this.GetAssetFilter());
            _selection.UpdateContentTypeId(assetLink?.Id ?? Guid.Empty);
        }

        OnSync(sync, context);
    }

    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    {
    }

    #endregion

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    AnalysisResult ISupportAnalysis.Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems.
    /// </summary>
    /// <param name="problems">The analysis problem collection.</param>
    /// <param name="intent">The analysis intent.</param>
    void ISupportAnalysis.CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        Asset asset = _selection.TargetAsset;
        if (asset != null && !_selection.IsValid)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, L($"{InputType.GetFullTypeNameText()} cannot be implemented by {asset.FullTypeName}")));
        }


        if (asset is null && _selection.Id != Guid.Empty)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, L("Link has been lost")));
        }
        else if (GetField() is DStructField field && !field.Optional && asset is null)
        {
            problems.Add(new AnalysisProblem(TextStatus.Warning, L($"The link of field {field.DisplayText} is not set")));
        }

        /*
        Asset asset = _key.TargetAsset;
        if (asset != null)
        {
            var collection = AssetManager.Instance.GetAssetCollection(_key.ContentTypeName);
            if (collection == null || collection.GetAsset(asset.AssetKey, _key.Filter) == null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"{InputType.GetFullTypeNameText()} cannot be implemented by {asset.FullTypeName}"));
            }
        }
        */
    }

    #endregion

    #region ITextDisplay

    /// <summary>
    /// Gets the display text.
    /// </summary>
    string ITextDisplay.DisplayText => EditorUtility.GetBriefString(TargetAsset);

    /// <summary>
    /// Gets the display icon.
    /// </summary>
    object ITextDisplay.DisplayIcon => TargetAsset?.Icon;

    /// <summary>
    /// Gets the display status.
    /// </summary>
    TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;

    #endregion

    public override string ToString() => L(DescriptionText);
}

/// <summary>
/// Converts SAssetKey to text using the selected key.
/// </summary>
public class SAssetKeyToTextConverter : TypeToTextConverter<SAssetKey>
{
    /// <summary>
    /// Converts the SAssetKey to its selected key string.
    /// </summary>
    /// <param name="objFrom">The SAssetKey to convert.</param>
    /// <returns>The selected key string.</returns>
    public override string Convert(SAssetKey objFrom)
    {
        return objFrom.SelectedKey;
    }
}