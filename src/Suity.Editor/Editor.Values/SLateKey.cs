using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Analyzing;
using Suity.Editor.Types;
using Suity.Selecting;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Values
{
    /// <summary>
    /// Represents a late binding link that resolves to a data key at runtime.
    /// </summary>
    [DisplayText("Late Binding Link")]
    public class SLateKey : SDynamic, ITextDisplay, ISupportAnalysis
    {
        private string _dataId = string.Empty;
        private SKeySelection _key = new();

        public SLateKey()
        {
        }

        public SLateKey(string lateDataId)
        {
            _dataId = lateDataId ?? string.Empty;
        }

        public SLateKey(TypeDefinition type, string lateDataId)
            : base(type?.OriginType.MakeDataLinkType())
        {
            _dataId = lateDataId ?? string.Empty;
            UpdateInputType();
        }

/// <summary>
    /// Gets the display icon.
    /// </summary>
    public override Image Icon => CoreIconCache.Clock;

    /// <summary>
    /// Gets the resolved value as an SKey.
    /// </summary>
    /// <param name="condition">The condition context.</param>
    public override object GetValue(ICondition condition = null)
    {
        UpdateLateBinding();
        UpdateBaseValue();

        return new SKey(InputType, _key.Id);
    }

/// <summary>
    /// Override this method to implement custom synchronization logic.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The synchronization context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        _dataId = sync.Sync("DataId", _dataId, SyncFlag.NotNull);

        _key = sync.Sync("LinkedValue", _key, SyncFlag.NotNull) ?? new();
        if (sync.IsSetterOf("LinkedValue"))
        {
            _key.BaseType = InputType.OriginType;
        }

        //if (sync.IsSetterOf("DataId"))
        //{
        //    UpdateLateBinding();
        //    UpdateBaseValue();
        //}

        base.OnSync(sync, context);
    }

    /// <summary>
    /// Override this method to implement custom view setup.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(_dataId, new ViewProperty("DataId", "Data Id"));

        base.OnSetupView(setup);
    }

    /// <summary>
    /// Called when the input type changes.
    /// </summary>
    protected override void OnInputTypeChanged()
    {
        base.OnInputTypeChanged();

        UpdateInputType();
    }

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

        private void UpdateLateBinding()
        {
            if (string.IsNullOrWhiteSpace(_dataId))
            {
                _key.Id = Guid.Empty;
                return;
            }

            var target = _key.TargetAsset;
            if (target != null && (target.ToDataId() == _dataId || target.Name == _dataId))
            {
                return;
            }

            var list = _key.GetList();

            var id = UpdateLateBinding(list, true);
            if (id == Guid.Empty)
            {
                id = UpdateLateBinding(list, false);
            }

            _key.Id = id;

            //Always update the original value so that it can be restored when dynamic binding is canceled.
            base.Value = new SKey(InputType, id);
        }
        private void UpdateBaseValue()
        {
            if (Value is not SKey key)
            {
                key = new SKey(InputType);
                Value = key;
            }

            key.TargetId = _key.Id;
        }

        private Guid UpdateLateBinding(ISelectionList list, bool exactMode, int depth = 0)
        {
            if (depth > 10)
            {
                return Guid.Empty;
            }

            foreach (var item in list.GetItems().Where(o => o is not ISelectionList).OfType<Asset>())
            {
                if (exactMode)
                {
                    if (item.ToDataId() == _dataId)
                    {
                        return item.Id;
                    }
                }
                else
                {
                    if (item.Name == _dataId)
                    {
                        return item.Id;
                    }
                }
            }

            depth++;
            foreach (var subList in list.GetItems().OfType<ISelectionList>())
            {
                var id = UpdateLateBinding(subList, exactMode, depth);
                if (id != Guid.Empty)
                {
                    return id;
                }
            }

            return Guid.Empty;
        }

        #region ISupportAnalysis

        /// <summary>
        /// Gets or sets the analysis result.
        /// </summary>
        public AnalysisResult Analysis { get; set; }

        /// <summary>
        /// Collects analysis problems.
        /// </summary>
        /// <param name="problems">The analysis problem collection.</param>
        /// <param name="intent">The analysis intent.</param>
        public void CollectProblem(AnalysisProblem problems, AnalysisIntents intent)
        {
            do
            {
                if (InputType is null)
                {
                    problems.Add(new AnalysisProblem(TextStatus.Error, L("Input type is not set")));
                    break;
                }

                if (_key.TargetAsset is not IDataAsset rowAsset)
                {
                    problems.Add(new AnalysisProblem(TextStatus.Error, L("Link is not set")));
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
                    var obj = row.Components.Where(o => elementType.IsAssignableFrom(o.ObjectType)).FirstOrDefault();
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

                    var obj = row.Components.Where(o => InputType.IsAssignableFrom(o.ObjectType)).FirstOrDefault();
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

        /// <summary>
        /// Gets the display text.
        /// </summary>
        public string DisplayText
        {
            get
            {
                string str = _key.DisplayText;
                if (string.IsNullOrWhiteSpace(str))
                {
                    str = _dataId;
                }
                else
                {
                    str = $"[{str}]";
                }

                return str;
            }
        }

        /// <summary>
        /// Gets the display icon.
        /// </summary>
        object ITextDisplay.DisplayIcon => _key.TargetAsset?.Icon ?? Icon;

        /// <summary>
        /// Gets the display status.
        /// </summary>
        public TextStatus DisplayStatus => _key.IsValid ? TextStatus.Reference : TextStatus.Error;

        #endregion

        public override string ToString()
        {
            return L("Late Binding") + " " + DisplayText;
        }
    }
}