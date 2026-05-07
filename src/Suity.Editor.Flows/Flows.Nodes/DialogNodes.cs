using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;
using System.Collections;
using System.Threading.Tasks;
using System.Threading;

namespace Suity.Editor.Flows.Nodes;

#region OkDialog

/// <summary>
/// A flow node that displays a chat dialog with a single 'OK' button.
/// Execution continues along the output connector when the user confirms.
/// </summary>
[DisplayText("OK Dialog", "*CoreIcon|Conversation")]
[ToolTipsText("A chat dialog with an 'OK' button")]
public class OkDialog : DialogFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _out;

    private ConnectorTextBlockProperty _message = new("Message", "Message");

    /// <summary>
    /// Initializes a new instance of the <see cref="OkDialog"/> class,
    /// setting up the input connector, message property, and output connector.
    /// </summary>
    public OkDialog()
    {
        _in = AddActionInputConnector("In", "Input");
        _message.AddConnector(this);

        _out = AddActionOutputConnector("Out", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var source = new TaskCompletionSource<object>();

        var conversation = compute.Context.GetArgument<IConversationHandler>();
        if (conversation is null)
        {
            source.SetException(new NullReferenceException($"{nameof(IConversationHandler)} not found."));
            return source.Task;
        }

        DisposableDialogItem dialogItem = null;

        IEnumerator dialogCoroutine()
        {
            string message = _message.GetText(compute, this) ?? string.Empty;
            dialogItem = conversation.AddDebugMessage(message, o =>
            {
                o.AddButton("OK", "Confirm");
            });

            yield return null;

            dialogItem?.Dispose();
            conversation.PopAction();

            switch (conversation.InputButton)
            {
                case "OK":
                    source.SetResult(_out);
                    break;

                default:
                    source.SetResult(null);
                    break;
            }
        }

        var coroutine = dialogCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            dialogItem?.Dispose();
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }
}

#endregion

#region YesNoDialog

/// <summary>
/// A flow node that displays a chat dialog with 'Yes' and 'No' buttons,
/// routing execution to the corresponding output connector based on the user's choice.
/// </summary>
[DisplayText("Yes/No Dialog", "*CoreIcon|Conversation")]
[ToolTipsText("A chat dialog with 'Yes' and 'No' buttons")]
public class YesNoDialog : DialogFlowNode
{
    private readonly FlowNodeConnector _in;
    private readonly FlowNodeConnector _outYes;
    private readonly FlowNodeConnector _outNo;

    private ConnectorTextBlockProperty _question = new("Question", "Question");

    /// <summary>
    /// Initializes a new instance of the <see cref="YesNoDialog"/> class,
    /// setting up the input connector, question property, and yes/no output connectors.
    /// </summary>
    public YesNoDialog()
    {
        _in = AddActionInputConnector("In", "Input");
        _question.AddConnector(this);

        _outYes = AddActionOutputConnector("OutYes", "Yes");
        _outNo = AddActionOutputConnector("OutNo", "No");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _question.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _question.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var source = new TaskCompletionSource<object>();

        var conversation = compute.Context.GetArgument<IConversationHandler>();
        if (conversation is null)
        {
            source.SetException(new NullReferenceException($"{nameof(IConversationHandler)} not found."));
            return source.Task;
        }

        DisposableDialogItem dialogItem = null;

        IEnumerator dialogCoroutine()
        {
            string question = _question.GetText(compute, this) ?? string.Empty;

            dialogItem = conversation.AddDebugMessage(question ?? string.Empty, o =>
            {
                o.AddButtons(string.Empty,
                    new ConversationButton { Key = "Yes", Text = "Yes" },
                    new ConversationButton { Key = "No", Text = "No" }
                    );
            });

            yield return null;

            dialogItem?.Dispose();
            conversation.PopAction();

            switch (conversation.InputButton)
            {
                case "Yes":
                    source.SetResult(_outYes);
                    break;

                case "No":
                    source.SetResult(_outNo);
                    break;

                default:
                    source.SetResult(null);
                    break;
            }
        }

        var coroutine = dialogCoroutine();
        conversation.PushCoroutine(coroutine);

        cancel.Register(() =>
        {
            dialogItem?.Dispose();
            conversation.StopCoroutine(coroutine);
            conversation.PopAction();
            source.TrySetCanceled();
        });

        return source.Task;
    }
}

#endregion

#region OutputMessage

/// <summary>
/// A flow node that outputs a message to the chat box log with a configurable
/// text status and optional delay before display.
/// </summary>
[DisplayText("Output Message Action", "*CoreIcon|Conversation")]
[ToolTipsText("Output a message to the chat box")]
public class OutputMessage : DialogFlowNode
{
    private readonly FlowNodeConnector _input;
    private readonly FlowNodeConnector _output;

    private ConnectorTextBlockProperty _message = new("Message", "Message");

    /// <summary>
    /// Gets or sets the status label applied to the output text (e.g., info, warning, error).
    /// </summary>
    public TextStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the delay in seconds before the message is output to the chat box.
    /// </summary>
    public float DelaySecond { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OutputMessage"/> class,
    /// setting up the input connector, message property, and output connector.
    /// </summary>
    public OutputMessage()
    {
        _input = AddActionInputConnector("Input", "Input");
        _message.AddConnector(this);

        _output = AddActionOutputConnector("Output", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
        Status = sync.Sync(nameof(Status), Status);
        DelaySecond = sync.Sync(nameof(DelaySecond), DelaySecond);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
        setup.InspectorField(Status, new ViewProperty(nameof(Status), "Text Status"));
        setup.InspectorField(DelaySecond, new ViewProperty(nameof(DelaySecond), "Delay").WithUnit("seconds"));
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        string message = _message.GetText(compute, this) ?? string.Empty;
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        if (DelaySecond > 0)
        {
            await Task.Delay((int)(DelaySecond * 1000), cancel);
        }

        cancel.ThrowIfCancellationRequested();

        compute.AddLog(Status, message);

        return _output;
    }
}

#endregion

#region CopyMessage

/// <summary>
/// A flow node that outputs a hidden message to the chat box with a 'Copy' button.
/// Clicking the button copies the specified text to the system clipboard.
/// </summary>
[DisplayText("Copy Message Action", "*CoreIcon|Conversation")]
[ToolTipsText("Output text to chat box, this message is hidden, and clicking the 'Copy' button copies the text to clipboard.")]
public class CopyMessage : DialogFlowNode
{
    private readonly FlowNodeConnector _input;
    private readonly FlowNodeConnector _output;

    private ConnectorTextBlockProperty _message = new("Message", "Message");
    private ConnectorTextBlockProperty _textInput = new("TextInput", "Copy Text");

    /// <summary>
    /// Gets or sets the status label applied to the output text (e.g., info, warning, error).
    /// </summary>
    public TextStatus Status { get; set; }

    /// <summary>
    /// Gets or sets the delay in seconds before the message is displayed in the chat box.
    /// </summary>
    public float DelaySecond { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CopyMessage"/> class,
    /// setting up the input connector, message and text input properties, and output connector.
    /// </summary>
    public CopyMessage()
    {
        _input = AddActionInputConnector("Input", "Input");
        _message.AddConnector(this);
        _textInput.AddConnector(this);

        _output = AddActionOutputConnector("Output", "Output");
    }

    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
        _textInput.Sync(sync);
        Status = sync.Sync(nameof(Status), Status);
        DelaySecond = sync.Sync(nameof(DelaySecond), DelaySecond);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
        _textInput.InspectorField(setup, this);
        setup.InspectorField(Status, new ViewProperty(nameof(Status), "Text Status"));
        setup.InspectorField(DelaySecond, new ViewProperty(nameof(DelaySecond), "Delay").WithUnit("seconds"));
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>()
            ?? throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");

        string msg = _message.GetText(compute, this) ?? string.Empty;
        string text = _textInput.GetText(compute, this) ?? string.Empty;

        if (DelaySecond > 0)
        {
            await Task.Delay((int)(DelaySecond * 1000), cancel);
        }

        cancel.ThrowIfCancellationRequested();

        conversation.AddDebugMessage(msg, o =>
        {
            o.AddButton("Copy", "Copy", () => EditorUtility.SetSystemClipboardText(text));
        });

        return _output;
    }
}

#endregion

#region ManualTextInput

/// <summary>
/// A flow node that prompts the user to input text in the chat box
/// and captures the entered content as a data output.
/// </summary>
[DisplayText("Manual Text Input", "*CoreIcon|Conversation")]
[ToolTipsText("Request user to input text in the chat box and read the content.")]
public class ManualTextInput : DialogFlowNode
{
    private FlowNodeConnector _in;
    private ConnectorTextBlockProperty _message = new("Message", "Message");

    private FlowNodeConnector _out;

    private FlowNodeConnector _result;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualTextInput"/> class,
    /// setting up the input connector, prompt message property, action output connector,
    /// and a data output connector for the user's response text.
    /// </summary>
    public ManualTextInput()
    {
        _in = AddActionInputConnector("In", "Input");
        _message.AddConnector(this);
        _out = AddActionOutputConnector("Out", "Output");
        _result = AddDataOutputConnector("Result", "string", "Response Text");
    }


    /// <inheritdoc/>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _message.Sync(sync);
    }

    /// <inheritdoc/>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _message.InspectorField(setup, this);
    }

    /// <inheritdoc/>
    public override async Task<object> ComputeAsync(IFlowComputationAsync compute, CancellationToken cancel)
    {
        var conversation = compute.Context.GetArgument<IConversationHandler>() 
            ?? throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");

        string message = _message.GetText(compute, this);
        if (!string.IsNullOrWhiteSpace(message))
        {
            conversation.AddInfoMessage(message);
        }

        string s = await conversation.WaitForTextInput(cancel);

        cancel.ThrowIfCancellationRequested();

        compute.SetValue(_result, s);

        return _out;
    }
}

#endregion
