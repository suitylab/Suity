using Suity.Collections;
using Suity.Editor.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Suity.Editor.CodeRender;

/// <summary>
/// A render target that wraps another render target and delegates operations to it.
/// </summary>
public abstract class WrapRenderTarget : RenderTarget
{
    /// <summary>
    /// Creates a new wrapped render target.
    /// </summary>
    /// <param name="inner">The inner render target to wrap.</param>
    /// <param name="item">The render item.</param>
    /// <param name="material">The material.</param>
    /// <param name="fileName">The file name.</param>
    /// <param name="ownerId">The owner ID.</param>
    /// <param name="language">The language.</param>
    /// <param name="location">The location.</param>
    protected WrapRenderTarget(RenderTarget inner, RenderItem item = null, IMaterial material = null, RenderFileName fileName = null, Guid? ownerId = null, string language = null, string location = null)
        : base(inner, item, material, fileName, ownerId, language, location)
    {
        Inner = inner ?? throw new ArgumentNullException(nameof(inner));
    }

    /// <summary>
    /// Gets the inner render target.
    /// </summary>
    public RenderTarget Inner { get; }

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        Inner.UserCodeEnabled = this.UserCodeEnabled;

        return Inner.Render(option);
    }

    /// <inheritdoc/>
    public override void Inject(IRenderSegmentCollection segments, object option)
    {
        Inner.Inject(segments, option);
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> AffectedIds => Inner.AffectedIds;

    /// <inheritdoc/>
    public override IFileBunch FileBunch => Inner.FileBunch;
}

/// <summary>
/// A render target that combines multiple render targets into a single output.
/// </summary>
public class CombinedRenderTarget : RenderTarget
{
    readonly List<RenderTarget> _renderTargets = [];

    /// <summary>
    /// Creates a combined render target from multiple render targets.
    /// </summary>
    /// <param name="fileName">The output file name.</param>
    /// <param name="renderTargets">The collection of render targets to combine.</param>
    public CombinedRenderTarget(RenderFileName fileName, IEnumerable<RenderTarget> renderTargets)
        : base(fileName)
    {
        _renderTargets.AddRange(renderTargets.SkipNull());
    }

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        List<RenderResult> results = [];

        RenderStatus status = RenderStatus.Success;

        foreach (var target in _renderTargets)
        {
            var subOption = option;
            if (subOption is ExpressionContext context)
            {
                // Since ExpressionContext is a rewritable structure, a new context must be cloned for each child item being rendered
                subOption = context.Clone();
            }

            target.UserCodeEnabled = this.UserCodeEnabled;

            try
            {
                var result = target.Render(subOption);
                if (result.Status == RenderStatus.ErrorContinue || result.Status == RenderStatus.ErrorInterrupt)
                {
                    status = RenderStatus.ErrorContinue;
                }
                else
                {
                    results.Add(result);
                }
            }
            catch (Exception err)
            {
                err.LogError();
                status = RenderStatus.ErrorContinue;
            }
        }

        return new CombinedRenderResult(status, results);
    }
}

/// <summary>
/// A render result that combines multiple render results into a single text output.
/// </summary>
public class CombinedRenderResult : RenderResult
{
    readonly List<RenderResult> _results = [];

    /// <summary>
    /// Creates a combined render result from multiple results.
    /// </summary>
    /// <param name="status">The render status.</param>
    /// <param name="results">The collection of render results to combine.</param>
    public CombinedRenderResult(RenderStatus status, IEnumerable<RenderResult> results)
        : base(status)
    {
        _results.AddRange(results.SkipNull());
    }

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override Stream GetStream() => new MemoryStream(Encoding.UTF8.GetBytes(GetText()));

    /// <inheritdoc/>
    public override string GetText()
    {
        var builder = new StringBuilder();
        bool append = false;

        foreach (var result in _results)
        {
            if (append)
            {
                builder.AppendLine();
            }
            else
            {
                append = true;
            }

            builder.Append(result.GetText());
        }

        return builder.ToString();
    }
}