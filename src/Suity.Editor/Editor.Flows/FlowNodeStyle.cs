using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow chart node style
/// </summary>
/// <remarks>
/// <seealso cref="IFlowDataStyle"/>
/// </remarks>
public abstract class FlowNodeStyle
{
    internal FlowNodeDrawDelegate _customDraw;

    private readonly Type[] _types;

    public FlowNodeStyle()
    {
    }
    public FlowNodeStyle(params Type[] types)
    {
        _types = types;
    }

    /// <summary>
    /// Flow node types that need configuration
    /// </summary>
    public virtual Type[] NodeTypes => _types?.SkipNull().ToArray() ?? [];

    /// <summary>
    /// Icon
    /// </summary>
    public virtual Image Icon => null;

    /// <summary>
    /// Whether to display header
    /// </summary>
    public virtual bool HasHeader => true;

    /// <summary>
    /// Width
    /// </summary>
    public virtual int? Width => null;

    /// <summary>
    /// Height
    /// </summary>
    public virtual int? Height => null;

    public virtual Color? BackgroundColor => null;

    public virtual Brush NodeFillBrush => null;
    public virtual Brush NodeHeaderFillBrush => null;
    public virtual Pen NodeOutlinePen => null;

    public virtual FlowNodeDrawDelegate CustomDraw
    {
        get => _customDraw;
        set => _customDraw = value;
    }

    public virtual bool RenderInputMultiple => false;
    public virtual bool RenderOutputMultiple => false;

    #region Static

    private static Dictionary<Type, FlowNodeStyle> _styles;

    public static FlowNodeStyle GetStyle(FlowNode node)
    {
        if (_styles is null)
        {
            InitializeStyles();
        }

        if (node is null)
        {
            return null;
        }

        return _styles.GetValueSafe(node.GetType());
    }

    public static FlowNodeStyle GetStyle(Type type)
    {
        if (_styles is null)
        {
            InitializeStyles();
        }

        if (type is null)
        {
            return null;
        }

        return _styles.GetValueSafe(type);
    }

    private static void InitializeStyles()
    {
        var styles = new Dictionary<Type, FlowNodeStyle>();

        foreach (var styleType in typeof(FlowNodeStyle).GetDerivedTypes().Where(o => !o.IsAbstract))
        {
            FlowNodeStyle style = (FlowNodeStyle)styleType.CreateInstanceOf();

            foreach (var nodeType in style.NodeTypes.SkipNull())
            {
                styles[nodeType] = style;
            }
        }

        foreach (var nodeType in typeof(FlowNode).GetDerivedTypes().Where(o => !o.IsAbstract))
        {
            var attr = nodeType.GetAttributeCached<SimpleFlowNodeStyleAttribute>();
            if (attr != null)
            {
                var style = new AttributedFlowNodeStyle(attr);
                styles[nodeType] = style;
            }
        }

        _styles = styles;
    } 

    #endregion
}

public abstract class FlowNodeBaseStyle<TBase> : FlowNodeStyle
     where TBase : FlowNode
{
    public FlowNodeBaseStyle()
        : base(typeof(TBase).GetDerivedTypes().ToArray())
    {
    }
}


#region FlowNodeStyle<T>
    /// <summary>
    /// Flow node style with type support. This style supports automatic recognition and application.
    /// </summary>
    /// <typeparam name="T"></typeparam>
public abstract class FlowNodeStyle<T> : FlowNodeStyle
    where T : FlowNode
{
    public FlowNodeStyle()
        : base(typeof(T))
    {
    }
}

public abstract class FlowNodeStyle<T1, T2> : FlowNodeStyle
    where T1 : FlowNode
    where T2 : FlowNode
{
    public FlowNodeStyle()
        : base(typeof(T1), typeof(T2))
    {
    }
}

public abstract class FlowNodeStyle<T1, T2, T3> : FlowNodeStyle
    where T1 : FlowNode
    where T2 : FlowNode
    where T3 : FlowNode
{
    public FlowNodeStyle()
        : base(typeof(T1), typeof(T2), typeof(T3))
    {
    }
}

public abstract class FlowNodeStyle<T1, T2, T3, T4> : FlowNodeStyle
    where T1 : FlowNode
    where T2 : FlowNode
    where T3 : FlowNode
    where T4 : FlowNode
{
    public FlowNodeStyle()
        : base(typeof(T1), typeof(T2), typeof(T3), typeof(T4))
    {
    }
}
#endregion


#region AttributedFlowNodeStyle
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class SimpleFlowNodeStyleAttribute : Attribute
{
    public string Icon { get; set; }
    public bool HasHeader { get; set; } = true;
    public int Width { get; set; }
    public int Height { get; set; }
    public string Color { get; set; }
    public bool RenderInputMultiple { get; set; }
    public bool RenderOutputMultiple { get; set; }
} 

class AttributedFlowNodeStyle : FlowNodeStyle
{
    readonly SimpleFlowNodeStyleAttribute _attr;

    readonly Color? _color;
    readonly Image _icon;
    readonly Brush _brush;
    readonly int? _width;
    readonly int? _height;
    readonly bool _inputMultiple;
    readonly bool _outputMultiple;

    public AttributedFlowNodeStyle(SimpleFlowNodeStyleAttribute attr)
    {
        _attr = attr;

        if (!string.IsNullOrWhiteSpace(attr.Icon))
        {
            _icon = EditorUtility.GetIcon(attr.Icon);
        }

        if (!string.IsNullOrWhiteSpace(attr.Color))
        {
            try
            {
                Color color = ColorTranslators.FromHtml(attr.Color);
                _brush = new SolidBrush(color);
                _color = color;
            }
            catch (Exception)
            {
            }
        }

        _width = attr.Width > 0 ? _attr.Width : (int?)null;
        _height = attr.Height > 0 ? _attr.Height : (int?)null;

        _inputMultiple = attr.RenderInputMultiple;
        _outputMultiple = attr.RenderOutputMultiple;
    }

    public override Image Icon => _icon;

    public override bool HasHeader => _attr.HasHeader;

    public override int? Width => _width;

    public override int? Height => _height;

    public override Color? BackgroundColor => _color;

    public override Brush NodeFillBrush => _brush;

    public override bool RenderInputMultiple => _inputMultiple;

    public override bool RenderOutputMultiple => _outputMultiple;
}

#endregion
