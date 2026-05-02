using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Flows;

/// <summary>
/// Type definition flow diagram data style
/// </summary>
public class TypeDefinitionFlowDataStyle : IFlowDataStyle
{
    public static bool USE_ARRAY_COLOR = false;
    public static bool USE_LINK_COLOR = false;

    public event EventHandler StyleUpdated;

    private readonly TypeDefinition _dataType;
    private readonly EditorObjectRef<DType> _typeRef;

    public TypeDefinitionFlowDataStyle(TypeDefinition dataType)
        : this(dataType, false, false)
    {
    }

    public TypeDefinitionFlowDataStyle(TypeDefinition dataType, bool multipleFromConnection, bool multipleToConnection)
    {
        _dataType = dataType ?? TypeDefinition.Unknown;

        MultipleFromConnection = multipleFromConnection;
        MultipleToConnection = multipleToConnection;

        UpdateStyle();

        DType target = dataType.Target;
        if (target != null)
        {
            _typeRef = new EditorObjectRef<DType>(target);
            _typeRef.TargetUpdated += _typeRef_TargetUpdated;
            _typeRef.ListenEnabled = true;
        }
    }

    public TypeDefinition DataType => _dataType;

    public virtual int PenWidth { get; protected set; } = 3;

    public string TypeName { get; private set; }

    public bool IsArray { get; private set; }

    public bool IsKey { get; private set; }

    public string DisplayName { get; private set; }

    public bool MultipleFromConnection { get; protected set; }

    public bool MultipleToConnection { get; protected set; }

    public Pen LinkPen { get; protected set; }

    public SolidBrush LinkArrowBrush { get; protected set; }

    public Pen ConnectorOutlinePen { get; protected set; }

    public SolidBrush ConnectorFillBrush { get; protected set; }

    private void _typeRef_TargetUpdated(object sender, EntryEventArgs e, ref bool handled)
    {
        UpdateStyle();
    }

    public void UpdateStyle()
    {
        TypeName = _dataType.ToTypeName();
        IsArray = _dataType.IsArray;
        IsKey = _dataType.IsLink || (_dataType.IsArray && _dataType.ElementType.IsLink);
        DisplayName = _dataType.ToDisplayString();

        OnUpdateStyle();

        StyleUpdated?.Invoke(this, EventArgs.Empty);
    }

    protected virtual void OnUpdateStyle()
    {
        UpdateStyle(_dataType);
    }

    protected virtual void UpdateStyle(TypeDefinition dataType)
    {
        if (dataType.Target?.ViewColor is Color color)
        {
            // Custom color
            UpdateStyleByColor(color);
        }
        else if (dataType.IsDataLink || dataType.IsAssetLink)
        {
            if (USE_LINK_COLOR)
            {
                if (dataType.ElementType?.IsAbstract == true)
                {
                    UpdateStyleDataLink();
                }
                else
                {
                    UpdateStyleDataLink();
                }
            }
            else
            {
                UpdateStyle(dataType.ElementType);
            }
        }
        else if (dataType.IsArray)
        {
            if (USE_ARRAY_COLOR)
            {
                UpdateStyleArray();
            }
            else
            {
                UpdateStyle(dataType.ElementType);
            }
        }
        else if (dataType.IsStruct)
        {
            UpdateStyleNormalStruct();
        }
        else if (dataType.IsAbstract)
        {
            UpdateStyleAbstractStruct();
        }
        else if (dataType.IsValue)
        {
            UpdateStyleValue();
        }
        else if (dataType.IsEnum)
        {
            UpdateStyleEnum();
        }
        else if (dataType.IsEmpty || dataType.IsBroken)
        {
            UpdateStyleError();
        }
        else
        {
            UpdateStyleNormalStruct();
        }
    }


    public readonly Color ColorValue = Color.FromArgb(148, 216, 45);
    public readonly Color ColorNormalStruct = Color.FromArgb(16, 128, 163);
    public readonly Color ColorAbstractStruct = Color.FromArgb(204, 93, 232);
    public readonly Color ColorLink = Color.FromArgb(92, 124, 250);
    public readonly Color ColorArray = Color.FromArgb(255, 186, 112);

    protected void UpdateStyleNormalStruct() => UpdateStyleByColor(ColorNormalStruct);

    protected void UpdateStyleAbstractStruct() => UpdateStyleByColor(ColorAbstractStruct);

    protected void UpdateStyleValue() => UpdateStyleByColor(ColorValue);

    protected void UpdateStyleEnum() => UpdateStyleByColor(ColorValue);

    protected void UpdateStyleDataLink() => UpdateStyleByColor(ColorLink);

    protected void UpdateStyleAbstractLink() => UpdateStyleByColor(ColorAbstractStruct);

    protected void UpdateStyleArray() => UpdateStyleByColor(ColorArray);

    protected void UpdateStyleError() => UpdateStyleByColor(Color.Red);

    protected void UpdateStyleByColor(Color color)
    {
        this.LinkPen = new Pen(color, PenWidth);
        this.LinkArrowBrush = new SolidBrush(color);
        this.ConnectorOutlinePen = new Pen(color, PenWidth);
        this.ConnectorFillBrush = new SolidBrush(color);
    }

    public override string ToString()
    {
        return _dataType.ToDisplayString();
    }
}

internal class ExampleDataStyle : TypeDefinitionFlowDataStyle
{
    public static readonly Color DefaultColorAbstract = Color.FromArgb(224, 132, 56);
    public static readonly Color DefaultColor = Color.FromArgb(132, 56, 224);

    private readonly Pen DefaultLinkPen = new(DefaultColor, 3);
    private readonly SolidBrush DefaultLinkArrowBrush = new(DefaultColor);
    private readonly Pen DefaultConnectorOutlinePen = new(DefaultColor, 3);
    private readonly SolidBrush DefaultConnectorFillBrush = new(DefaultColor);

    private readonly Pen DefaultLinkPenA = new(DefaultColorAbstract, 3);
    private readonly SolidBrush DefaultLinkArrowBrushA = new(DefaultColorAbstract);
    private readonly Pen DefaultConnectorOutlinePenA = new(DefaultColorAbstract, 3);
    private readonly SolidBrush DefaultConnectorFillBrushA = new(DefaultColorAbstract);

    // Single source, multiple targets
    public ExampleDataStyle(TypeDefinition dataType)
        : base(dataType, false, true)
    {
    }

    protected override void OnUpdateStyle()
    {
        if (!TypeDefinition.IsNullOrBroken(DataType))
        {
            if (DataType.IsAbstract)
            {
                LinkPen = DefaultLinkPenA;
                LinkArrowBrush = DefaultLinkArrowBrushA;
                ConnectorOutlinePen = DefaultConnectorOutlinePenA;
                ConnectorFillBrush = DefaultConnectorFillBrushA;
            }
            else
            {
                LinkPen = DefaultLinkPen;
                LinkArrowBrush = DefaultLinkArrowBrush;
                ConnectorOutlinePen = DefaultConnectorOutlinePen;
                ConnectorFillBrush = DefaultConnectorFillBrush;
            }
        }
        else
        {
            base.OnUpdateStyle();
        }
    }
}

public class TypeFlowDataStyle : IFlowDataStyle
{
    private TypeFlowDataStyle(Type type)
    {
        TargetType = type ?? throw new ArgumentNullException(nameof(type));

        string colorCode = type.GetAttributeCached<NativeTypeAttribute>()?.Color;

        Color color;

        if (!string.IsNullOrWhiteSpace(colorCode))
        {
            color = ColorTranslators.FromHtml(colorCode);
        }
        else
        {
            color = EditorServices.ColorConfig.GetStatusColor(TextStatus.Reference);
        }

        LinkPen = new Pen(color, 3);
        LinkArrowBrush = new SolidBrush(color);
        ConnectorOutlinePen = new Pen(color, 3);
        ConnectorFillBrush = new SolidBrush(color);
    }


    public Type TargetType { get; }

    public string TypeName => TargetType.FullName;

    public bool IsArray => TargetType.IsArray;

    public bool IsKey => false;

    public string DisplayName => TypeName.ToDisplayTextL();

    public bool MultipleFromConnection => true;

    public bool MultipleToConnection => true;

    public Pen LinkPen { get; }

    public SolidBrush LinkArrowBrush { get; }

    public Pen ConnectorOutlinePen { get; }

    public SolidBrush ConnectorFillBrush { get; }


    public event EventHandler StyleUpdated;



    readonly static Dictionary<string, TypeFlowDataStyle> _dataStyles = [];

    public static TypeFlowDataStyle GetDataStyle(string dataType)
    {
        if (dataType is null)
        {
            return null;
        }

        if (_dataStyles.TryGetValue(dataType, out var style))
        {
            return style;
        }

        var type = SyncTypes.GlobalResolver?.ResolveType(dataType, null); /*dataType.ResolveType();*/
        if (type != null)
        {
            style = new TypeFlowDataStyle(type);
            _dataStyles[dataType] = style;

            return style;
        }

        return null;
    }

    public static TypeFlowDataStyle GetDataStyle(Type dataType)
    {
        if (dataType is null)
        {
            return null;
        }

        if (_dataStyles.TryGetValue(dataType.FullName, out var style))
        {
            return style;
        }

        style = new TypeFlowDataStyle(dataType);
        _dataStyles[dataType.FullName] = style;

        return style;
    }
}