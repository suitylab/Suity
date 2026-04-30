using Suity.Editor.CodeRender;
using Suity.Editor.Services;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Expressions;

/// <summary>
/// A render target that generates code expressions for a single object.
/// </summary>
public class ExpressionRenderTarget : RenderTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ExpressionRenderTarget"/> class.
    /// </summary>
    /// <param name="fileName">The output file name.</param>
    /// <param name="material">The material to use for rendering.</param>
    /// <param name="item">The render item containing the object to render.</param>
    /// <param name="language">The target programming language.</param>
    public ExpressionRenderTarget(RenderFileName fileName, IMaterial material, RenderItem item, string language)
        : base(item, material, fileName, language, null)
    {
    }

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        if (option is not ExpressionContext context)
        {
            return RenderResult.Empty;
        }

        object obj = Item.Object;

        try
        {
            // Force add function context parameter
            context.WithBody = true;
            context.UseFullName = true;
            context.ClassName = $"{EditorUtility.GetNameSpace(obj)}.{Item.Name}";
            context.UserCodeEnabled = this.UserCodeEnabled;

            var expr = ExpressionFactory.BuildNode(obj, context, true)
                ?? throw new NullReferenceException("Failed to get expression: " + Item.Name);

            var fullExpr = ExpressionFactory.BuildFullSource(obj, expr);

            var renderer = Device.Current.GetService<IExpressionRenderService>();
            return renderer.Render(fullExpr, Language, context, obj);
        }
        catch (Exception err)
        {
            err.LogError("Expression render failed", obj);
            Logs.LogError($"Render failed Source:{this.Item} File:{this.FileName}");
            return RenderHelper.CreateTextRenderResult(RenderStatus.ErrorInterrupt, string.Empty);
        }
    }
}

/// <summary>
/// A render target that generates code expressions for multiple objects in a group.
/// </summary>
public class MultipleExpressionRenderTarget : RenderTarget
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MultipleExpressionRenderTarget"/> class.
    /// </summary>
    /// <param name="fileName">The output file name.</param>
    /// <param name="material">The material to use for rendering.</param>
    /// <param name="item">The render item containing the object to render.</param>
    /// <param name="language">The target programming language.</param>
    public MultipleExpressionRenderTarget(RenderFileName fileName, IMaterial material, RenderItem item, string language)
        : base(item, material, fileName, language, null)
    {
    }

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        if (option is not ExpressionContext context)
        {
            return RenderResult.Empty;
        }

        object obj = Item.Object;

        if (obj is not GroupAsset group)
        {
            return RenderResult.Empty;
        }

        try
        {
            List<ExpressionNode> nodes = [];

            // Force add function context parameter
            context.WithBody = true;
            context.UseFullName = true;
            context.ClassName = $"{EditorUtility.GetNameSpace(obj)}.{Item.Name}";
            context.UserCodeEnabled = this.UserCodeEnabled;

            var mainExpr = ExpressionFactory.BuildNode(group, context, true);
            if (mainExpr != null) 
            {
                nodes.Add(mainExpr);
            }

            foreach (var childAsset in group.ChildAssets)
            {
                var childExpr = ExpressionFactory.BuildNode(childAsset, context, true);
                if (childExpr != null)
                {
                    nodes.Add(childExpr);
                }
            }

            var fullExpr = ExpressionFactory.BuildFullSource(obj, nodes);

            var renderer = Device.Current.GetService<IExpressionRenderService>();
            return renderer.Render(fullExpr, Language, context, obj);
        }
        catch (Exception err)
        {
            err.LogError("Expression render failed", obj);
            Logs.LogError($"Render failed Source:{this.Item} File:{this.FileName}");
            return RenderHelper.CreateTextRenderResult(RenderStatus.ErrorInterrupt, string.Empty);
        }
    }
}