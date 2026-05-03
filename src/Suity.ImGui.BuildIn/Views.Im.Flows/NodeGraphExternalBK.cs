using Suity.Drawing;
using Suity.Editor;
using Suity.Editor.Flows;
using Suity.Views.NodeGraph;
using System;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Views.Im.Flows;

/// <summary>
/// External backend implementation for node graph rendering, providing ImGui-based UI construction.
/// </summary>
internal class NodeGraphExternalBK : NodeGraphExternal
{
    /// <summary>
    /// Gets the singleton instance of this backend.
    /// </summary>
    public static NodeGraphExternalBK Instance { get; } = new NodeGraphExternalBK();

    /// <inheritdoc/>
    public override ImGuiNode FittedNodeFrame(ImGui gui, IDrawNodeContext context)
    {
        return gui.NodeFrame(context.Id + "#fit")
        .OnInitialize(n =>
        {
            n.InitClass("node");
            n.InitFit();
            n.InitValue(this);
        })
        .OverrideColor(context.BackgroundColor)
        //.SetSize(Width, Height)
        .SetPosition(context.X, context.Y);
    }

    /// <inheritdoc/>
    public override ImGuiNode ResizableNodeFrame(ImGui gui, IDrawNodeContext context)
    {
        return gui.NodeFrame(context.Id + "#resize")
        .OnInitialize(n =>
        {
            n.InitClass("node");
            n.InitValue(this);
            n.InitFit(GuiOrientation.None);

            //n.InitInputFunctionChain(ImGraphExtensions.NodeResizeInput);
        })
        .OverrideColor(context.BackgroundColor)
        //.SetSize(Width, Height)
        .SetPosition(context.X, context.Y);
    }

    /// <inheritdoc/>
    public override ImGuiNode HeaderFrame(ImGui gui, IDrawNodeContext context, string id, bool fit)
    {
        return gui.HorizontalFrame(id)
        .OnInitialize(n =>
        {
            if (fit)
            {
                n.InitClass("headerFrameFit", "left-right", "debug_draw");
            }
            else
            {
                n.InitClass("headerFrame", "left-right", "debug_draw");
            }
        })
        .OverrideColor(context.TitleColor);
    }

    /// <inheritdoc/>
    public override void TitleTextSection(ImGui gui, IDrawNodeContext context)
    {
        if (context.DisplayStatus is { } status && status.ToStatusIcon() is { } statusIcon)
        {
            gui.Image("statusIcon", statusIcon)
            .InitClass("icon", "scaleHiddenMedium");
        }

        if (context.DisplayIcon is ImageDef icon)
        {
            gui.Image("icon", icon)
            .InitClass("icon", "scaleHiddenMedium");
        }

        // Prevent text from being too long
        string text = L(context.DisplayText ?? string.Empty);
        if (text.Length > 30)
        {
            text = text[..15] + "..." + text[^15..];
        }

        gui.Text("text", text)
         .InitClass("titleText");
         //.OverrideFont(null, context.TitleColor);
    }

    /// <inheritdoc/>
    public override void TitlePreviewSection(ImGui gui, IDrawNodeContext context)
    {
        if (context.PreviewText is { } preview && !string.IsNullOrWhiteSpace(preview))
        {
            gui.NumberBox("preview", L(preview));
        }

        // InvalidIcon(gui, context.IsValid());
    }

    /// <inheritdoc/>
    [Obsolete]
    public override ImGuiNode? InvalidIcon(ImGui gui, bool isValid)
    {
        if (!isValid)
        {
            return gui.Image("invalid", CoreIconCache.Warning)
            .InitClass("icon", "scaleHiddenMedium");
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public override ImGuiNode HorizontalBox(ImGui gui, string id, bool fit)
    {
        return gui.HorizontalLayout(id)
        .OnInitialize(n =>
        {
            if (fit)
            {
                n.InitClass("sideBoxFit", "debug_draw");
            }
            else
            {
                n.InitClass("sideBoxHori", "debug_draw");
            }
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode VerticalBox(ImGui gui, string id, bool fit)
    {
        return gui.VerticalLayout(id)
        .OnInitialize(n =>
        {
            if (fit)
            {
                n.InitClass("sideBoxFit", "debug_draw");
            }
            else
            {
                n.InitClass("sideBoxVert", "debug_draw");
            }
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode BodyFrame(ImGui gui, string id)
    {
        return gui.HorizontalLayout("body")
            .InitClass("bodyFit", "left-right", "debug_draw");
    }

    /// <inheritdoc/>
    public override ImGuiNode OverlayBodyFrame(ImGui gui, string id)
    {
        return gui.OverlayLayout("body")
        .OnInitialize(n =>
        {
            n.InitClass("body", "left-right", "debug_draw");
            n.InitFit(GuiOrientation.Vertical);
            n.InitFullWidth();
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode ConnectorBox(ImGui gui, FlowNodeConnector connector, IDrawContext context)
    {
        bool input = connector.Direction == FlowDirections.Input;

        ImGuiNode node = input ?
            gui.HorizontalLayout(connector.Name) :
            gui.HorizontalReverseLayout(connector.Name);

        string cls = input ? "inputRow" : "outputRow";

        node
        .InitClass(cls, "debug_draw");

        return node;
    }

    /// <summary>
    /// Creates a connector row for a view-side graph connector.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="connector">The graph connector to display.</param>
    /// <param name="context">The draw context.</param>
    /// <param name="multipleIcon">Indicates whether to show the multiple connection icon.</param>
    /// <param name="innerDraw">Optional custom drawing function for the connector row content.</param>
    /// <returns>The rendered ImGui node for the connector row.</returns>
    public ImGuiNode ConnectorRow(ImGui gui, GraphConnector connector, IDrawContext context, bool multipleIcon, DrawImGui? innerDraw = null)
    {
        bool input = connector.Direction == GraphDirection.Input;

        ImGuiNode node = input ?
            gui.HorizontalLayout(connector.Name) :
            gui.HorizontalReverseLayout(connector.Name);

        string cls = input ? "inputRow" : "outputRow";

        node
        .InitClass(cls, "debug_draw")
        .OnContent(() =>
        {
            ConnectorPoint(gui, connector, "connector");

            if (multipleIcon)
            {
                gui.DrawMultipleIcon(connector);
            }

            innerDraw?.Invoke(gui);

            ConnectorTextSection(gui, connector);
        });

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? ConnectorRow(ImGui gui, FlowNodeConnector connector, IDrawContext context, bool multipleIcon, DrawImGui? innerDraw = null)
    {
        if (connector.ResolveViewConnector(context) is { } c)
        {
            return ConnectorRow(gui, c, context, multipleIcon, innerDraw);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Creates a connector point for a view-side graph connector.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="connector">The graph connector to display.</param>
    /// <param name="id">The unique identifier for the connector point.</param>
    /// <returns>The rendered ImGui node for the connector point.</returns>
    public ImGuiNode ConnectorPoint(ImGui gui, GraphConnector connector, string id)
    {
        string multipleText = connector.ShowMultiple() ? " " + L("Multiple connections allowed") : string.Empty;

        var node = gui.ConnectorPoint(id)
            .InitClass("connectorPoint", "debug_draw")
            .InitValue(connector)
            .SetToolTips($"{L(connector.DisplayName)} {connector.DataType}{multipleText}");

        if (connector.Tag is not ImGuiNodeRef nodeRef)
        {
            connector.Tag = nodeRef = new ImGuiNodeRef();
        }

        nodeRef.Node = node;

        return node;
    }

    /// <inheritdoc/>
    public override ImGuiNode? ConnectorPoint(ImGui gui, FlowNodeConnector connector, IDrawContext context, string id)
    {
        //TODO: ViewTag mechanism causes issues with multiple views.
        if (connector.ResolveViewConnector(context) is GraphConnector c)
        {
            return ConnectorPoint(gui, c, id);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Draws the text label for a graph connector.
    /// </summary>
    /// <param name="gui">The ImGui instance for drawing.</param>
    /// <param name="connector">The graph connector to display text for.</param>
    public void ConnectorTextSection(ImGui gui, GraphConnector connector)
    {
        gui.Text(connector.DisplayName)
            .InitClass("connectorText");
    }

    /// <inheritdoc/>
    public override void ConnectorTextSection(ImGui gui, FlowNodeConnector connector)
    {
        gui.Text(connector.DisplayName)
            .InitClass("connectorText");
    }

    /// <inheritdoc/>
    public override void FlashingOnce(FlowNodeConnector connector, IDrawContext? context)
    {
        EditorUtility.InvokeInMainThread(() =>
        {
            if (context != null)
            {
                var c = connector.ResolveViewConnector(context);
                var node = (c?.Tag as ImGuiNodeRef)?.Node;
                node?.StartAnimation(ConnectorPointFlashingOnce.Instance, true);
            }
            else
            {
                var cs = connector.ResolveViewConnectors();
                foreach (var c in cs)
                {
                    var node = (c?.Tag as ImGuiNodeRef)?.Node;
                    node?.StartAnimation(ConnectorPointFlashingOnce.Instance, true);
                }
            }
        });
    }

    /// <inheritdoc/>
    public override void QueueRefresh(FlowNode node)
    {
        if (node is null)
        {
            throw new ArgumentNullException(nameof(node));
        }

        foreach (var viewNode in node.ViewNodes)
        {
            viewNode.QueueRefresh();
        }
    }

    /// <inheritdoc/>
    public override ImGuiNode SimpleFrame(ImGui gui, FlowDirections direction, IDrawNodeContext drawContext, string? text = null, ImageDef? image = null, DrawEditorImGui? editorGui = null)
    {
        return FittedNodeFrame(gui, drawContext)
        .OnContent(() =>
        {
            editorGui?.Invoke(gui, EditorImGuiPipeline.Output, drawContext);

            BodyFrame(gui, "body")
            .OnContent(() =>
            {
                var node = direction == FlowDirections.Input ? gui.HorizontalLayout("hori") : gui.HorizontalReverseLayout("hori");

                node
                .InitFit(GuiOrientation.Both)
                .InitHorizontalAlignment(GuiAlignment.Near)
                .OnContent(() => 
                {
                    image ??= drawContext.DisplayIcon as ImageDef;
                    if (image != null)
                    {
                        gui.Image("icon", image)
                        .InitClass("iconSmall", "scaleHiddenMedium");
                    }

                    if (drawContext.DisplayStatus is { } status && status.ToStatusIcon() is { } statusIcon)
                    {
                        gui.Image("statusIcon", statusIcon)
                        .InitClass("icon", "scaleHiddenMedium");
                    }

                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        gui.Text(text.ToShortcutString(30))
                        .InitClass("connectorText");
                    }
                });
            });

            editorGui?.Invoke(gui, EditorImGuiPipeline.Input, drawContext);
        });
    }

    /// <inheritdoc/>
    public override ImGuiNode SingleConnectorFrame(ImGui gui, FlowNodeConnector connector, IDrawNodeContext drawContext,
        string? text = null, ImageDef? image = null, DrawEditorImGui? editorGui = null)
    {
        return FittedNodeFrame(gui, drawContext)
        .OnContent(() =>
        {
            editorGui?.Invoke(gui, EditorImGuiPipeline.Output, drawContext);

            BodyFrame(gui, "body")
            .OnContent(() =>
            {
                ConnectorBox(gui, connector, drawContext)
                .OnContent(() =>
                {
                    ConnectorPoint(gui, connector, drawContext, "connector");
                    string textV = text ?? connector.DisplayName;
                    if (!string.IsNullOrWhiteSpace(textV))
                    {
                        gui.Text(textV.ToShortcutString(30))
                            .InitClass("connectorText");
                    }

                    if (drawContext.DisplayStatus is { } status && status.ToStatusIcon() is { } statusIcon)
                    {
                        gui.Image("statusIcon", statusIcon)
                        .InitClass("icon", "scaleHiddenMedium");
                    }

                    image ??= drawContext.DisplayIcon as ImageDef;
                    if (image != null)
                    {
                        gui.Image("icon", image)
                        .InitClass("iconSmall", "scaleHiddenMedium");
                    }
                });
            });

            editorGui?.Invoke(gui, EditorImGuiPipeline.Input, drawContext);
        });
    }
}
