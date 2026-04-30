using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System.Drawing;
using System.Linq;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.Values;

/// <summary>
/// Represents a dynamic link to another structure's data, resolving values from linked data assets.
/// </summary>
[DisplayText("Structure Link")]
public class SDynamicLink : SDynamic, ITextDisplay, ISupportAnalysis
{
    private SKeySelection _key = new();

    /// <summary>
    /// Initializes a new instance of <see cref="SDynamicLink"/>.
    /// </summary>
    public SDynamicLink()
    {
    }

    /// <summary>
    /// Initializes a new instance with the specified value.
    /// </summary>
    /// <param name="value">The initial value.</param>
    public SDynamicLink(object value)
        : base(value)
    {
        UpdateInputType();
    }

    /// <inheritdoc/>
    public override Image Icon => CoreIconCache.Link;

    /// <inheritdoc/>
    public override object GetValue(ICondition condition = null)
    {
        do
        {
            if (InputType is null)
            {
                break;
            }

            if (_key.TargetAsset is not IDataAsset rowAsset)
            {
                break;
            }

            var row = rowAsset.GetData(true);
            if (row is null)
            {
                break;
            }

            if (InputType.IsArray)
            {
                if ((rowAsset as Asset)?.Parent is not IDataTableAsset dataAsset)
                {
                    break;
                }

                var dataTable = dataAsset.GetDataContainer(true);
                if (dataTable is null)
                {
                    break;
                }

                var ary = new SArray(InputType);
                var elementType = InputType.ElementType;

                if (elementType.IsDataLink)
                {
                    ary.Add(new SKey(elementType, row.Id));
                    string incrId = row.DataLocalId;

                    KeyIncrementHelper.ParseKey(incrId, out string prefix, out int digiLen, out ulong digiValue);
                    do
                    {
                        digiValue++;
                        incrId = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue);

                        var incrRow = dataTable.GetData(incrId);
                        if (incrRow is null)
                        {
                            break;
                        }

                        ary.Add(new SKey(elementType, incrRow.Id));
                    } while (true);
                }
                else
                {
                    var obj = row.Components.Where(o => o.ObjectType == elementType).FirstOrDefault();
                    if (obj is null)
                    {
                        break;
                    }

                    ary.Add(Cloner.Clone(obj));
                    string incrId = row.DataLocalId;

                    KeyIncrementHelper.ParseKey(incrId, out string prefix, out int digiLen, out ulong digiValue);
                    do
                    {
                        digiValue++;
                        incrId = KeyIncrementHelper.MakeKey(prefix, digiLen, digiValue);

                        var incrRow = dataTable.GetData(incrId);
                        if (incrRow is null)
                        {
                            break;
                        }

                        obj = incrRow.Components.Where(o => elementType.IsAssignableFrom(o.ObjectType)).FirstOrDefault();
                        ary.Add(Cloner.Clone(obj));
                    } while (true);
                }
                return ary;
            }
            else
            {
                if (InputType.IsDataLink)
                {
                    return new SKey(InputType, row.Id);
                }
                else
                {
                    var obj = row.Components.Where(o => InputType.IsAssignableFrom(o.ObjectType)).FirstOrDefault();
                    if (obj is null)
                    {
                        break;
                    }

                    return Cloner.Clone(obj);
                }
            }
        } while (false);

        return base.GetValue(condition);
    }


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        _key = sync.Sync("LinkedValue", _key, SyncFlag.NotNull | SyncFlag.AffectsOthers);
        if (sync.IsSetterOf("LinkedValue"))
        {
            _key.BaseType = InputType.OriginType;
        }

        base.OnSync(sync, context);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_key, new ViewProperty("LinkedValue", "Link"));

        base.OnSetupView(setup);
    }

    /// <inheritdoc/>
    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();

        UpdateInputType();
    }

    /// <summary>
    /// Updates the input type of the key selection based on the current input type.
    /// </summary>
    private void UpdateInputType()
    {
        if (InputType is null)
        {
            _key.BaseType = null;
            return;
        }

        if (InputType.IsArray)
        {
            _key.BaseType = InputType.ElementType;
        }
        else
        {
            _key.BaseType = InputType;
        }
    }

    #region ISupportAnalysis

    /// <inheritdoc/>
    public AnalysisResult Analysis { get; set; }

    /// <inheritdoc/>
    public void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
    {
        do
        {
            if (InputType is null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, L("Input type not set")));
                break;
            }

            if (_key.TargetAsset is not IDataAsset rowAsset)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, L("Link not set")));
                break;
            }

            var row = rowAsset.GetData(true);
            if (row is null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, L("Data not found")));
                break;
            }

            if (InputType.IsArray)
            {
                if (InputType.ElementType.IsDataLink)
                {
                    break;
                }

                var elementType = InputType.ElementType ?? TypeDefinition.Empty;
                var obj = row.Components.FirstOrDefault(o => elementType.IsAssignableFrom(o.ObjectType));
                if (obj is null)
                {
                    problems.Add(new AnalysisProblem(TextStatus.Error, L("Data not found")));
                    break;
                }
            }
            else
            {
                if (InputType.IsDataLink)
                {
                    break;
                }

                var obj = row.Components.FirstOrDefault(o => InputType.IsAssignableFrom(o.ObjectType));
                if (obj is null)
                {
                    problems.Add(new AnalysisProblem(TextStatus.Error, L("Data not found")));
                    break;
                }
            }
        } while (false);
    } 

    #endregion

    #region ITextDisplay

    /// <inheritdoc/>
    public string DisplayText => _key.DisplayText;

    /// <inheritdoc/>
    object ITextDisplay.DisplayIcon => _key.TargetAsset?.Icon ?? Icon;

    /// <inheritdoc/>
    public TextStatus DisplayStatus => _key.IsValid ? TextStatus.Reference : TextStatus.Error;

    #endregion

    /// <inheritdoc/>
    public override string ToString()
    {
        var asset = _key.TargetAsset;
        string s;

        if (asset != null)
        {
            s = asset.ToDisplayText() ?? "null";
        }
        else
        {
            s = base.ToString();
        }

        return L("Structure Link") + " " + s;
    }
}
