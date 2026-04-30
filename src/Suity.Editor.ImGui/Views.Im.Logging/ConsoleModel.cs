using Suity.Editor;
using Suity.Editor.View.ViewModel;
using Suity.Views.PathTree;

namespace Suity.Views.Im.Logging;

/// <summary>
/// Provides a console model for logging with runtime log support and filtering by log level.
/// </summary>
public class ConsoleModel : LoggingImGuiModel, IRuntimeLog
{
    /// <summary>
    /// The maximum string length for log entries.
    /// </summary>
    public const int MaxStringLength = 300;

    private bool _lvDebug = true;
    private bool _lvInfo = true;
    private bool _lvWarning = true;
    private bool _lvError = true;

    /// <summary>
    /// Gets or sets a value indicating whether debug log messages are visible.
    /// </summary>
    public bool LvDebug
    {
        get => _lvDebug;
        set
        {
            if (_lvDebug != value)
            {
                _lvDebug = value;

                RaiseRefresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether info log messages are visible.
    /// </summary>
    public bool LvInfo
    {
        get => _lvInfo;
        set
        {
            if (_lvInfo != value)
            {
                _lvInfo = value;

                RaiseRefresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether warning log messages are visible.
    /// </summary>
    public bool LvWarning
    {
        get => _lvWarning;
        set
        {
            if (_lvWarning != value)
            {
                _lvWarning = value;

                RaiseRefresh();
            }
        }
    }

    /// <summary>
    /// Gets or sets a value indicating whether error log messages are visible.
    /// </summary>
    public bool LvError
    {
        get => _lvError;
        set
        {
            if (_lvError != value)
            {
                _lvError = value;

                RaiseRefresh();
            }
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConsoleModel"/> class.
    /// </summary>
    public ConsoleModel()
    {
        base.Filter = FilterNode;
    }

    #region IRuntimeLog

    /// <summary>
    /// Adds a log message to the console, queued for thread-safe execution.
    /// </summary>
    /// <param name="type">The type of the log message.</param>
    /// <param name="message">The message to log.</param>
    public void AddLog(LogMessageType type, object message)
    {
        message ??= string.Empty;

        QueuedAction.Do(() =>
        {
            AddLogNow(type, message);
        });
    }

    #endregion

    /// <summary>
    /// Immediately adds a log message to the console without queuing.
    /// </summary>
    /// <param name="logLevel">The log level of the message.</param>
    /// <param name="message">The message to log.</param>
    /// <param name="indent">The indentation level for the log entry.</param>
    public void AddLogNow(LogMessageType logLevel, object message, int indent = 0)
    {
        while (Count > MaxEntry)
        {
            RemoveRange(0, 100);
        }

        var node = LogNode.Create(logLevel, message, indent);
        if (node != null)
        {
            AddMonitorNode(node);
        }
    }

    private bool FilterNode(LogNode node) => node.LogLevel switch
    {
        LogMessageType.Debug => _lvDebug,
        LogMessageType.Info => _lvInfo,
        LogMessageType.Warning => _lvWarning,
        LogMessageType.Error => _lvError,
        _ => false,
    };


    /// <summary>
    /// Configures the visual content rendering for a log node in the ImGui view.
    /// </summary>
    /// <param name="node">The ImGui node to configure.</param>
    /// <param name="vNode">The path node to render.</param>
    /// <param name="context">The drawing context.</param>
    public static void ConfigContent(ImGuiNode node, PathNode vNode, IDrawContext context)
    {
        var gui = node.Gui;

        var draw = vNode as IDrawEditorImGui;
        LogNode mNode = vNode as LogNode;

        if (draw is null || !draw.OnEditorGui(gui, EditorImGuiPipeline.Prefix, context))
        {
            if (vNode.CustomImage != null)
            {
                gui.Image("##custom_icon", vNode.CustomImage)
                .InitClass("icon");
            }

            if (vNode.Image != null)
            {
                gui.Image("##icon", vNode.Image)
                .InitClass("icon");
            }

            if (vNode.TextStatusIcon != null)
            {
                gui.Image("##status_icon", vNode.TextStatusIcon)
                .InitClass("icon");
            }

            if (mNode?.Indent > 0)
            {
                gui.VerticalLayout("indent")
                .InitFullHeight()
                .InitWidth(mNode.Indent * 20);
            }
        }

        if (draw is null || !draw.OnEditorGui(gui, EditorImGuiPipeline.Name, context))
        {
            if (!string.IsNullOrWhiteSpace(mNode?.PropertyName))
            {
                gui.Frame("##prop_frame")
                .InitClass("progressBar")
                .InitFullHeight()
                .InitFit()
                .OnContent(() =>
                {
                    gui.Text("##property", mNode.PropertyName)
                    .InitClass("propLabelText")
                    .InitVerticalAlignment(GuiAlignment.Center);
                });
            }
        }

        if (draw is null || !draw.OnEditorGui(gui, EditorImGuiPipeline.Description, context))
        {
            gui.VerticalLayout("frame")
            .InitSizeRest()
            .OnContent(() =>
            {
                if (mNode?.Lines != null)
                {
                    for (int i = 0; i < mNode.Lines.Length; i++)
                    {
                        gui.Text($"##line_{i}", mNode.Lines[i])
                        .InitHeight(14)
                        .SetFontColor(vNode.Color)
                        .InitVerticalAlignment(GuiAlignment.Center);
                    }
                }
                else
                {
                    gui.Text("##title_text", vNode.Text)
                    .SetFontColor(vNode.Color)
                    .InitFullHeight()
                    .InitVerticalAlignment(GuiAlignment.Center);
                }
            });
        }

        draw?.OnEditorGui(gui, EditorImGuiPipeline.Preview, context);

        node.InitInputDoubleClicked(n =>
        {
            bool handled = false;

            // Already handled by OnOpenRequest in HeaderlessPathTreeView
            //if (vNode is IViewDoubleClickAction dbc)
            //{
            //    dbc.DoubleClick();
            //    handled = true;
            //}
            //else 
            if (mNode?.Tag != null)
            {
                var vo = new NavigateVReq { Target = mNode.Tag };
                EditorCommands.Mapper.Handle(vo);
                handled = vo.Successful;

                //if (!vo.Successful)
                //{
                //    DialogUtility.ShowMessageBoxAsync("Unable to navigate to: " + mNode.Tag.ToString());
                //}
            }

            if (!handled)
            {
                //StackTrace stackTrace = new StackTrace();           // get call stack
                //StackFrame[] stackFrames = stackTrace.GetFrames();  // get method calls (frames)

                //string str = string.Join("\r\n", stackFrames.Select(o => o.GetMethod().Name));

                //DialogUtility.ShowTextBlockDialogAsync(string.Empty, str, null);
            }
        });
    }
}
