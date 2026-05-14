using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Represents a data key that references a data asset.
/// </summary>
[NativeType(CodeBase = "Suity", Description = "Data Key", Icon = "*CoreIcon|Data")]
[NativeAlias]
public class SKey : SItem,
    ISelection,
    ISyncObject,
    ISupportAnalysis,
    IHasAsset,
    IHasId,
    ITextDisplay
{
    private SKeySelection _selection = new();

    public SKey()
    { }

    public SKey(TypeDefinition inputType)
        : base(inputType)
    {
        OnInputTypeChanged();
    }

    public SKey(TypeDefinition inputType, string key)
        : base(inputType)
    {
        OnInputTypeChanged();
        _selection.SelectedKey = key;
    }

    public SKey(TypeDefinition inputType, Guid id)
        : base(inputType)
    {
        OnInputTypeChanged();
        _selection.Id = id;
    }

    /// <summary>
    /// Gets the target asset referenced by this key.
    /// </summary>
    public virtual Asset TargetAsset => _selection.TargetAsset;

    /// <summary>
    /// Gets the description text for this key.
    /// </summary>
    public virtual string DescriptionText => _selection.ToString();

    /// <summary>
    /// Gets a value indicating whether the asset is valid.
    /// </summary>
    /// <param name="asset">The asset to validate.</param>
    /// <returns>True if valid; otherwise, false.</returns>
    public virtual bool GetIsValid(Asset asset) => _selection.GetIsValid(asset);

    protected override void OnInputTypeChanged()
    {
        _selection.BaseType = InputType.OriginType;
    }

    /// <summary>
    /// Gets the target SObject referenced by this key.
    /// </summary>
    /// <returns>The target SObject, or null if not found.</returns>
    public SObject GetTargetObject()
    {
        if (TargetAsset is not IDataAsset rowAsset)
        {
            return null;
        }

        if (rowAsset.GetData(true) is not IDataItem row)
        {
            return null;
        }

        var type = InputType.OriginType;

        return row.Components.FirstOrDefault(o => o.ObjectType == type);
    }

    #region Judgment

    public override bool ValueEquals(object other)
    {
        if (other is SKey sKey)
        {
            return (_selection?.Id ?? Guid.Empty) == (sKey._selection?.Id ?? Guid.Empty);
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
    /// Gets the ID of the referenced object.
    /// </summary>
    public Guid Id => _selection.Id;

    #endregion

    #region ISelection

    /// <summary>
    /// Gets or sets the selected key.
    /// </summary>
    public virtual string SelectedKey
    {
        get => _selection.SelectedKey;
        set => _selection.SelectedKey = value;
    }

    ISelectionList ISelection.GetSelectionList()
    {
        if (InputType.OriginType?.IsAbstractStruct == true)
        {
            return _selection.GetSelectionList();
        }
        else
        {
            return _selection.GetSelectionList();
        }
    }

    /// <summary>
    /// Gets a value indicating whether the selection is valid.
    /// </summary>
    public virtual bool IsValid => _selection.IsValid;

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

        _selection = sync.Sync("Value", _selection, SyncFlag.NotNull);
        if (sync.IsSetterOf("Value"))
        {
            _selection.BaseType = InputType.OriginType;
        }

        OnSync(sync, context);
    }

    /// <summary>
    /// Override this method to implement custom synchronization logic.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    #endregion

    #region ISupportAnalysis

    /// <summary>
    /// Gets or sets the analysis result.
    /// </summary>
    AnalysisResult ISupportAnalysis.Analysis { get; set; }

    /// <summary>
    /// Collects analysis problems for this key.
    /// </summary>
    /// <param name="problems">The analysis problem collection.</param>
    /// <param name="intent">The analysis intent.</param>
    void ISupportAnalysis.CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        _selection.BaseType = InputType.OriginType;

        Asset asset = _selection.TargetAsset;
        if (asset != null && !_selection.IsValid)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, L($"{InputType.GetFullTypeNameText()} cannot be implemented by {asset.FullTypeName}")));
        }

        if (asset is null && _selection.Id != Guid.Empty)
        {
            problems.Add(new AnalysisProblem(TextStatus.Error, L("Link is lost")));
        }
        else if (GetField() is DStructField field && !field.Optional && asset is null)
        {
            problems.Add(new AnalysisProblem(TextStatus.Warning, L($"Link for field {field.DisplayText} is not set")));
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
/// Converts SKey to text using the selected key.
/// </summary>
public class SKeyToTextConverter : TypeToTextConverter<SKey>
{
    /// <summary>
    /// Converts the SKey to its selected key string.
    /// </summary>
    /// <param name="objFrom">The SKey to convert.</param>
    /// <returns>The selected key string.</returns>
    public override string Convert(SKey objFrom)
    {
        return objFrom.SelectedKey;
    }
}