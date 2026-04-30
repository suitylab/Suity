using Suity.Collections;
using Suity.Editor.CodeRender;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Views;
using System.Drawing;
using System.Linq;

namespace Suity.Editor
{
    /// <summary>
    /// Data row asset
    /// </summary>
    [AssetTypeBinding(AssetDefNames.Data, "Data row asset")]
    public class DataRowAsset : Asset,
        IDataAsset,
        ITextDisplay,
        IPreviewDisplay
    {
        public const string DataFamilySuffix = "Data";

        private TypeDefinition[] _types = [];

        public DataRowAsset()
        {
            UpdateAssetTypes(typeof(IDataAsset));
        }

        public TypeDefinition[] DataTypes
        {
            get => _types;
            protected internal set
            {
                value ??= [];

                if (ArrayHelper.ArrayEquals(_types, value))
                {
                    return;
                }

                _types = value;

                UpdateAssetTypes([.. _types.Select(o => o.TypeCode), typeof(IDataAsset).ResolveAssetTypeName()]);

                NotifyPropertyUpdated();
            }
        }

        public override Image DefaultIcon => CoreIconCache.Row;

        public override RenderType RenderType => RenderType.Data;

        #region IDataTypeContext

        public TypeDefinition[] GetDataTypes() => [.. _types];

        public bool SupportType(TypeDefinition type)
        {
            return _types.Contains(type);
        }

        public virtual IDataItem GetData(bool tryLoadStorage)
        {
            return GetStorageObject(tryLoadStorage) as IDataItem;
        }

        # endregion

        protected internal override string ResolveResourceName() => this.ToDataId();


        #region ITextDisplay

        string ITextDisplay.DisplayText => this.DisplayText;

        object ITextDisplay.DisplayIcon => this.Icon;

        TextStatus ITextDisplay.DisplayStatus => TextStatus.Normal;


        #endregion

        #region IPreviewDisplay

        string IPreviewDisplay.PreviewText
        {
            get
            {
                do
                {
                    var row = GetData(false);
                    if (row is null || !row.Components.Any())
                    {
                        break;
                    }

                    string brief = null;

                    if (row.Components.CountOne())
                    {
                        brief = row.Components.First().GetBrief();
                    }
                    else
                    {
                        var briefs = row.Components.Select(o => o.GetBrief()).ToArray();
                        if (briefs.Any(o => !string.IsNullOrWhiteSpace(o)))
                        {
                            brief = string.Join(", ", briefs);
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(brief))
                    {
                        //return $"{base.DisplayText} ({brief})";
                        return brief;
                    }
                } while (false);

                return string.Empty;
            }
        }

        object IPreviewDisplay.PreviewIcon => null;

        #endregion
    }

    public class DataRowAssetBuilder : AssetBuilder<DataRowAsset>, IDesignBuilder
    {
        public IAssetElementCollector<TypeDefinition> DataTypes { get; }

        public DataRowAssetBuilder()
        {
            DataTypes = AddElementCollector<TypeDefinition>(
                nameof(DataRowAsset.DataTypes),
                (o, col) =>
                {
                    o.DataTypes = [.. col];
                });
        }

        #region IDesignBuilder
        public void SetBindingInfo(object bindingInfo)
        {
            // Not implemented
        }

        public void UpdateAttributes(IAttributeDesign attributes)
        {
            // Not implemented
        }
        #endregion
    }
}