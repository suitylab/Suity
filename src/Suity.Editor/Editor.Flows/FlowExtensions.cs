using Suity.Drawing;
using Suity.Editor.Analyzing;
using Suity.Editor.Values;
using Suity.Views.Graphics;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.Flows;

/// <summary>
/// Flow extensions
/// </summary>
public static class FlowExtensions
{
    /// <summary>
    /// Gets the flow document from a node.
    /// </summary>
    public static FlowDocument GetFlowDocument(this FlowNode node)
    {
        return node.Diagram?.DocumentContent as FlowDocument;
    }

    /// <summary>
    /// Gets the flow document from a diagram.
    /// </summary>
    public static FlowDocument GetFlowDocument(this IFlowDiagram diagram)
    {
        return diagram?.DocumentContent as FlowDocument;
    }

    /// <summary>
    /// Collects the output network.
    /// </summary>
    public static void CollectOutputNetwork(this FlowDocument document, FlowNode node, HashSet<FlowNode> collection)
        => FlowsExternal._external.CollectOutputNetwork(document, node, collection);

    /// <summary>
    /// Collects the input network.
    /// </summary>
    public static void CollectInputNetwork(this FlowDocument document, FlowNode node, HashSet<FlowNode> collection)
        => FlowsExternal._external.CollectInputNetwork(document, node, collection);

    /// <summary>
    /// Sets auto value for flow.
    /// </summary>
    public static void SetFlowAutoValue(this SObject obj, FlowDiagramItem item, PositionAutomationMode mode)
        => FlowsExternal._external.SetFlowAutoValue(obj, item, mode);

    /// <summary>
    /// Creates a computation for the document.
    /// </summary>
    public static IFlowComputation CreateComputation(this FlowDocument document, FunctionContext context = null)
        => FlowsExternal._external.CreateComputation(document, context);

    /// <summary>
    /// Gets a value from the computation.
    /// </summary>
    public static T GetValue<T>(this IFlowComputation compuate, FlowNodeConnector connector, T defaultValue = default)
    {
        object o = compuate.GetValue(connector);
        if (o is T t)
        {
            return t;
        }

        if (o is SAssetKey sAssetKey && sAssetKey.TargetAsset is T t2)
        {
            return t2;
        }

        if (typeof(T).IsArray && o is SArray sary)
        {
            var ary = sary.ToArray(typeof(T).GetElementType());
            return (T)(object)ary;
        }

        return defaultValue;
    }

    public static T[] GetValues<T>(this IFlowComputation compuate, FlowNodeConnector connector, bool sort = true)
    {
        object[] sourceAry = compuate.GetValues(connector, sort) ?? [];

        static IEnumerable<T> func(object o)
        {
            if (o is not string && o is System.Collections.IEnumerable ary)
            {
                foreach (var item in ary)
                {
                    if (item is T t)
                    {
                        yield return t;
                    }
                    else if (item is SItem sItem && SItem.ResolveValue(sItem) is T t2)
                    {
                        yield return t2;
                    }
                }
            }
            else if (o is T t) // This needs to be placed last because T could be Object, causing wildcard matching
            {
                yield return t;
            }
            else if (o is SObject sObj && sObj.Controller is { } v1 && v1 is T v1t)
            {
                yield return v1t;
            }
            else if (o is SItem sItem && SItem.ResolveValue(sItem) is { } v2 && v2 is T v2t)
            {
                yield return v2t;
            }
        }

        var filter = sourceAry.SelectMany(func).ToArray();

        return filter;
    }

    public static T GetValueConvert<T>(this IFlowComputation compuate, FlowNodeConnector connector, T defaultValue = default)
    {
        object o = compuate.GetValue(connector);
        if (o is T t)
        {
            return t;
        }
        else if (o is null)
        {
            return defaultValue;
        }
        else
        {
            try
            {
                o = Convert.ChangeType(o, typeof(T));
                if (o is T t2)
                {
                    return t2;
                }
                else
                {
                    return defaultValue;
                }
            }
            catch (Exception)
            {
            }

            // Convert array
            if (typeof(T).IsArray)
            {
                Type elementType = typeof(T).GetElementType();

                if (o is SArray sary)
                {
                    try
                    {
                        Type nType = sary.InputType.Target?.NativeType;
                        if (nType == typeof(T).GetElementType())
                        {
                            return (T)(object)sary.ToArray();
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
                else if (o is Array ary)
                {
                    try
                    {
                        Array newAry = Array.CreateInstance(elementType, ary.Length);
                        for (int i = 0; i < ary.Length; i++)
                        {
                            newAry.SetValue(ary.GetValue(i), i);
                        }

                        return (T)(object)newAry;
                    }
                    catch (Exception)
                    {

                        throw;
                    }
                }
            }

            return defaultValue;
        }
    }

    public static T EnsureValue<T>(this IFlowComputation compuate, FlowNodeConnector connector)
    {
        object o = compuate.GetValue(connector);
        if (o is T t)
        {
            return t;
        }
        else
        {
            throw new InvalidOperationException($"Get data from connector failed : {connector}.");
        }
    }

    public static T EnsureValueConvert<T>(this IFlowComputation compuate, FlowNodeConnector connector)
    {
        object o = compuate.GetValue(connector);
        if (o is T t)
        {
            return t;
        }
        else
        {
            o = Convert.ChangeType(o, typeof(T));
            if (o is T t2)
            {
                return t2;
            }
            else
            {
                throw new InvalidOperationException($"Get data from connector failed : {connector}.");
            }
        }
    }

    public static bool TryGetValue<T>(this IFlowComputation compuate, FlowNodeConnector connector, out T value)
    {
        object o = compuate.GetValue(connector);
        if (o is T t)
        {
            value = t;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }


    public static bool GetIsEnded(this FlowRunningState state)
    {
        if (state is null)
        {
            return false;
        }

        return state.State.GetIsEnded();
    }

    /// <summary>
    /// Gets whether the current state of the node flow computation state is in an end state
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public static bool GetIsEnded(this FlowComputationStates state) => state switch
    {
        FlowComputationStates.Finished or FlowComputationStates.Error or FlowComputationStates.Cancelled => true,
        _ => false,
    };


    public static FunctionContext GetContext(this IFlowComputation compute, FlowContextScopes scope, IFlowDiagram diagram) => scope switch
    {
        FlowContextScopes.Local => compute.LocalContext,
        FlowContextScopes.Diagram => compute.GetDiagramContext(diagram),
        _ => compute.Context,
    };

    public static object GetNodeCache(this IFlowComputation compute, FlowContextScopes scope, FlowNode node)
    {
        var ctx = compute.GetContext(scope, node.Diagram);

        return ctx.GetArgument("##cache:" + node.Name);
    }

    public static void SetNodeCache(this IFlowComputation compute, FlowContextScopes scope, FlowNode node, object obj)
    {
        var ctx = compute.GetContext(scope, node.Diagram);

        ctx.SetArgument("##cache:" + node.Name, obj);
    }


    public static object GetVariable(this IFlowComputation compute, FlowContextScopes scope, IFlowDiagram diagram, string varName)
    {
        var ctx = compute.GetContext(scope, diagram);

        return ctx.GetArgument("##var:" + varName);
    }

    public static void SetVariable(this IFlowComputation compute, FlowContextScopes scope, IFlowDiagram diagram, string varName, object value)
    {
        var ctx = compute.GetContext(scope, diagram);

        ctx.SetArgument("##var:" + varName, value);
    }

    public static object GetFlowVariable(this FunctionContext ctx, string varName)
    {
        return ctx.GetArgument("##var:" + varName);
    }

    public static void SetFlowVariable(this FunctionContext ctx, string varName, object value)
    {
        ctx.SetArgument("##var:" + varName, value);
    }


    #region DrawRef

    private static Color _refColorDark = Color.FromArgb(36, 36, 36);

    private static BrushDef _refBrush;
    private static FontDef _refFont;
    private static BrushDef _refTextBrush;
    private static BrushDef _refBrushDark;

    /// <summary>
    /// Draws the reference count indicator.
    /// </summary>
    public static void DrawRef(this IGraphicOutput output, float zoom, Rectangle rect, AnalysisResult analysis)
    {
        _refBrush ??= new SolidBrushDef(TextStatus.Reference.ToColor());
        _refFont ??= new FontDef(ImGuiTheme.DefaultFont, 12);
        _refBrushDark ??= new SolidBrushDef(_refColorDark);
        _refTextBrush ??= new SolidBrushDef(TextStatus.Normal.ToColor());

        string text = analysis.ReferenceCount.ToString();
        SizeF textSize = output.MeasureString(text, _refFont);
        int textW = (int)textSize.Width + 4;

        int w = 20 + textW;
        int h = 16;
        int m = (int)(4 * zoom);

        var rectRef = new Rectangle(rect.Right - w - m, rect.Y + m, w, h);
        var rectIcon = new Rectangle(rectRef.X + 1, rectRef.Y + 1, 16, 16);

        var rectDark = new Rectangle(rectRef.X + 18, rectRef.Y + 2, textW, 12);
        var rectText = new Rectangle(rectRef.X + 20, rectRef.Y + 13, textW, 16);

        // int textCenter = rectText.X + rectText.Width / 2;

        output.FillRoundRectangle(_refBrush, rectRef, 6);
        output.DrawImage(CoreIconCache.Reference, rectIcon, _refColorDark);
        output.FillRoundRectangle(_refBrushDark, rectDark, 10);
        output.DrawString(text, _refFont, _refTextBrush, rectText.X, rectText.Y);
    }

    #endregion
}