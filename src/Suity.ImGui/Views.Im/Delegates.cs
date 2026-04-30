
using Suity.Views.Graphics;
using System.Collections.Generic;

namespace Suity.Views.Im;

/// <summary>
/// Delegates used throughout the ImGui rendering pipeline for input, layout, fitting, and rendering operations.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="node">The ImGui node receiving input.</param>
/// <param name="input">The graphic input data.</param>
/// <param name="baseAction">The base action for processing child input.</param>
/// <returns>The resulting GUI input state.</returns>
public delegate GuiInputState InputFunction(GuiPipeline pipeline, ImGuiNode node, IGraphicInput input, ChildInputFunction baseAction);

/// <summary>
/// Delegate for processing child input operations within the pipeline.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="childNodeSelector">Optional selector for specific child nodes to process.</param>
/// <returns>The resulting GUI input state.</returns>
public delegate GuiInputState ChildInputFunction(GuiPipeline pipeline, IEnumerable<ImGuiNode>? childNodeSelector = null);

/// <summary>
/// Delegate for layout operations that position child nodes within a parent.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="node">The ImGui node being laid out.</param>
/// <param name="position">The layout position information.</param>
/// <param name="baseAction">The base action for processing child layout.</param>
public delegate void LayoutFunction(GuiPipeline pipeline, ImGuiNode node, GuiLayoutPosition position, ChildLayoutFunction baseAction);

/// <summary>
/// Delegate for processing child layout operations within the pipeline.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
public delegate void ChildLayoutFunction(GuiPipeline pipeline);

/// <summary>
/// Delegate for fit operations that calculate the desired size of a node.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="node">The ImGui node being fitted.</param>
/// <param name="baseAction">The base action for processing child fit.</param>
public delegate void FitFunction(GuiPipeline pipeline, ImGuiNode node, ChildFitFunction baseAction);

/// <summary>
/// Delegate for processing child fit operations within the pipeline.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
public delegate void ChildFitFunction(GuiPipeline pipeline);

/// <summary>
/// Delegate for render operations that draw a node to the output.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="node">The ImGui node being rendered.</param>
/// <param name="output">The graphic output target.</param>
/// <param name="dirtyMode">Indicates whether the node is in dirty (redraw) mode.</param>
/// <param name="baseAction">The base action for processing child render.</param>
public delegate void RenderFunction(GuiPipeline pipeline, ImGuiNode node, IGraphicOutput output, bool dirtyMode, ChildRenderFunction baseAction);

/// <summary>
/// Delegate for processing child render operations within the pipeline.
/// </summary>
/// <param name="pipeline">The GUI pipeline context.</param>
/// <param name="childNodeSelector">Optional selector for specific child nodes to render.</param>
public delegate void ChildRenderFunction(GuiPipeline pipeline, IEnumerable<ImGuiNode>? childNodeSelector = null);

/// <summary>
/// Delegate for factory methods that create ImGuiNode instances.
/// </summary>
/// <param name="gui">The ImGui context.</param>
/// <param name="id">The unique identifier for the node.</param>
/// <param name="data">Optional data associated with the node.</param>
/// <returns>A new ImGuiNode instance.</returns>
public delegate ImGuiNode NodeFactory(ImGui gui, string id, object? data);

/// <summary>
/// Delegate for content templates that render content within a node for a specific value type.
/// </summary>
/// <typeparam name="T">The type of the value being rendered.</typeparam>
/// <param name="node">The ImGui node containing the content.</param>
/// <param name="value">The value to render.</param>
public delegate void ContentTemplate<in T>(ImGuiNode node, T value);

/// <summary>
/// Delegate for content templates that render content within a node.
/// </summary>
/// <param name="node">The ImGui node containing the content.</param>
public delegate void ContentTemplate(ImGuiNode node);

/// <summary>
/// Delegate for retrieving the length (height or width) of an item in a virtual list.
/// </summary>
/// <typeparam name="T">The type of the item.</typeparam>
/// <param name="value">The item whose length is being measured.</param>
/// <returns>The length of the item.</returns>
public delegate float LengthGetter<in T>(T value);
