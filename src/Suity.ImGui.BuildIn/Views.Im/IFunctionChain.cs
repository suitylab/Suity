using Suity.Views.Graphics;
using System;

namespace Suity.Views.Im;

/// <summary>
/// Function chain
/// </summary>
/// <typeparam name="T"></typeparam>
public interface IFunctionChain<T> where T : class
{
    /// <summary>
    /// Function main entry
    /// </summary>
    T Entry { get; }

    /// <summary>
    /// Override function
    /// </summary>
    T OverrideFunction { get; }

    /// <summary>
    /// Base function
    /// </summary>
    T? BaseFunction { get; }
}

/// <summary>
/// Chains an input function with a base function, allowing the override to call through to the base.
/// </summary>
public class InputFunctionChain : IFunctionChain<InputFunction>
{
    private readonly InputFunction _func;
    private readonly Func<InputFunction?> _baseFuncGetter;

    /// <inheritdoc/>
    public InputFunction Entry => Handle;

    /// <inheritdoc/>
    public InputFunction OverrideFunction => _func;

    /// <inheritdoc/>
    public InputFunction? BaseFunction => _baseFuncGetter();

    /// <summary>
    /// Initializes a new input function chain with a function getter for the base.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFuncGetter">A function that returns the base function.</param>
    public InputFunctionChain(InputFunction func, Func<InputFunction?> baseFuncGetter)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = baseFuncGetter ?? throw new ArgumentNullException(nameof(baseFuncGetter));
    }

    /// <summary>
    /// Initializes a new input function chain with a direct base function.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFunc">The base function.</param>
    public InputFunctionChain(InputFunction func, InputFunction baseFunc)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = () => baseFunc;
    }

    private GuiInputState Handle(GuiPipeline pipeLine, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction)
    {
        var baseFunc = _baseFuncGetter();

        bool baseCall = false;

        var state = _func(pipeLine, node, input, (pipeLineChild, selector) => 
        {
            baseCall = true;

            if (pipeLineChild == GuiPipeline.Blocked)
            {
                baseAction(GuiPipeline.Blocked, selector);
                return GuiInputState.None;
            }

            if (baseFunc != null)
            {
                return baseFunc(pipeLineChild, node, input, baseAction);
            }
            else
            {
                return baseAction(pipeLineChild, selector);
            }
        });

        //if (!baseCall)
        //{
        //    GuiInputState baseState;

        //    if (baseFunc != null)
        //    {
        //        baseState = baseFunc(pipeLine, node, input, baseAction);
        //    }
        //    else
        //    {
        //        baseState = baseAction(pipeLine);
        //    }

        //    ImGui.MergeState(ref state, baseState);
        //}

        return state;
    }
}

/// <summary>
/// Chains a layout function with a base function, allowing the override to call through to the base.
/// </summary>
public class LayoutFunctionChain : IFunctionChain<LayoutFunction>
{
    private readonly LayoutFunction _func;
    private readonly Func<LayoutFunction?> _baseFuncGetter;

    /// <inheritdoc/>
    public LayoutFunction Entry => Handle;

    /// <inheritdoc/>
    public LayoutFunction OverrideFunction => _func;

    /// <inheritdoc/>
    public LayoutFunction? BaseFunction => _baseFuncGetter();

    /// <summary>
    /// Initializes a new layout function chain with a function getter for the base.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFuncGetter">A function that returns the base function.</param>
    public LayoutFunctionChain(LayoutFunction func, Func<LayoutFunction?> baseFuncGetter)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = baseFuncGetter ?? throw new ArgumentNullException(nameof(baseFuncGetter));
    }

    /// <summary>
    /// Initializes a new layout function chain with a direct base function.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFunc">The base function.</param>
    public LayoutFunctionChain(LayoutFunction func, LayoutFunction baseFunc)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = () => baseFunc;
    }

    private void Handle(GuiPipeline pipeLine, ImGuiNode node, GuiLayoutPosition position, ChildLayoutFunction baseAction)
    {
        var baseFunc = _baseFuncGetter();

        bool baseCall = false;

        _func(pipeLine, node, position, pipeLineChild =>
        {
            baseCall = true;

            if (pipeLineChild == GuiPipeline.Blocked)
            {
                baseAction(GuiPipeline.Blocked);
                return;
            }

            if (baseFunc != null)
            {
                baseFunc(pipeLineChild, node, position, baseAction);
            }
            else
            {
                baseAction(pipeLineChild);
            }
        });

        //if (!baseCall)
        //{
        //    if (baseFunc != null)
        //    {
        //        baseFunc(pipeLine, node, position, baseAction);
        //    }
        //    else
        //    {
        //        baseAction(pipeLine);
        //    }
        //}
    }
}

/// <summary>
/// Chains a fit function with a base function, allowing the override to call through to the base.
/// </summary>
public class FitFunctionChain : IFunctionChain<FitFunction>
{
    private readonly FitFunction _func;
    private readonly Func<FitFunction?> _baseFuncGetter;

    /// <inheritdoc/>
    public FitFunction Entry => Handle;

    /// <inheritdoc/>
    public FitFunction OverrideFunction => _func;

    /// <inheritdoc/>
    public FitFunction? BaseFunction => _baseFuncGetter();

    /// <summary>
    /// Initializes a new fit function chain with a function getter for the base.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFuncGetter">A function that returns the base function.</param>
    public FitFunctionChain(FitFunction func, Func<FitFunction?> baseFuncGetter)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = baseFuncGetter ?? throw new ArgumentNullException(nameof(baseFuncGetter));
    }

    /// <summary>
    /// Initializes a new fit function chain with a direct base function.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFunc">The base function.</param>
    public FitFunctionChain(FitFunction func, FitFunction baseFunc)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = () => baseFunc;
    }

    private void Handle(GuiPipeline pipeLine, ImGuiNode node, ChildFitFunction baseAction)
    {
        var baseFunc = _baseFuncGetter();

        bool baseCall = false;

        _func(pipeLine, node, pipeLineChild =>
        {
            baseCall = true;

            if (pipeLineChild == GuiPipeline.Blocked)
            {
                baseAction(GuiPipeline.Blocked);
                return;
            }

            if (baseFunc != null)
            {
                baseFunc(pipeLineChild, node, baseAction);
            }
            else
            {
                baseAction(pipeLineChild);
            }
        });

        //if (!baseCall)
        //{
        //    if (baseFunc != null)
        //    {
        //        baseFunc(pipeLine, node, baseAction);
        //    }
        //    else
        //    {
        //        baseAction(pipeLine);
        //    }
        //}
    }
}

/// <summary>
/// Chains a render function with a base function, allowing the override to call through to the base.
/// </summary>
public class RenderFunctionChain : IFunctionChain<RenderFunction>
{
    private readonly RenderFunction _func;
    private readonly Func<RenderFunction?> _baseFuncGetter;

    /// <inheritdoc/>
    public RenderFunction Entry => Handle;

    /// <inheritdoc/>
    public RenderFunction OverrideFunction => _func;

    /// <inheritdoc/>
    public RenderFunction? BaseFunction => _baseFuncGetter();

    /// <summary>
    /// Initializes a new render function chain with a function getter for the base.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFuncGetter">A function that returns the base function.</param>
    public RenderFunctionChain(RenderFunction func, Func<RenderFunction?> baseFuncGetter)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = baseFuncGetter ?? throw new ArgumentNullException(nameof(baseFuncGetter));
    }

    /// <summary>
    /// Initializes a new render function chain with a direct base function.
    /// </summary>
    /// <param name="func">The override function.</param>
    /// <param name="baseFunc">The base function.</param>
    public RenderFunctionChain(RenderFunction func, RenderFunction baseFunc)
    {
        _func = func ?? throw new ArgumentNullException(nameof(func));
        _baseFuncGetter = () => baseFunc;
    }

    private void Handle(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction)
    {
        var baseFunc = _baseFuncGetter();

        bool baseCall = false;

        _func(pipeline, node, output, dirtyMode, (pipeLineChild, selector) =>
        {
            baseCall = true;

            if (pipeLineChild == GuiPipeline.Blocked)
            {
                baseAction(GuiPipeline.Blocked);
                return;
            }

            if (baseFunc != null)
            {
                baseFunc(pipeLineChild, node, output, dirtyMode, baseAction);
            }
            else
            {
                baseAction(pipeLineChild);
            }
        });

        //if (!baseCall)
        //{
        //    if (baseFunc != null)
        //    {
        //        baseFunc(pipeline, node, output, dirtyMode, baseAction);
        //    }
        //    else
        //    {
        //        baseAction(pipeline);
        //    }
        //}
    }
}