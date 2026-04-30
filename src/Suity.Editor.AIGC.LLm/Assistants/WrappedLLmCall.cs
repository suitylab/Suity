using static Suity.Helpers.GlobalLocalizer;
using Suity.Views;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants;

/// <summary>
/// Used to wrap LLM calls. Display call model and parameters info before calling, delete message box after calling.
/// </summary>
internal class WrappedLLmCall : ILLmCall
{
    private readonly ILLmCall _call;
    private readonly LLmModelPreset _presetType;
    private readonly IConversationHandler _conversation;
    private readonly string _promptId;

    private int _textLength;

    private bool _hasFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="WrappedLLmCall"/> class.
    /// </summary>
    /// <param name="call">The underlying LLM call to wrap.</param>
    /// <param name="presetType">The model preset type.</param>
    /// <param name="conversation">Optional conversation handler for displaying messages.</param>
    /// <param name="promptId">Optional prompt identifier.</param>
    public WrappedLLmCall(ILLmCall call, LLmModelPreset presetType, IConversationHandler conversation = null, string promptId = null)
    {
        _call = call ?? throw new ArgumentNullException(nameof(call));
        _presetType = presetType;
        _conversation = conversation;
        _promptId = promptId;
    }

    private void ResetCounter()
    {
        _textLength = 0;
        _hasFunc = false;
    }

    #region ILLmCall

    /// <inheritdoc/>
    public ILLmModel Model => _call.Model;

    /// <inheritdoc/>
    public FunctionContext Context => _call.Context;

    /// <inheritdoc/>
    public bool HasFunction => _call.HasFunction;

    /// <inheritdoc/>
    public string FunctionCall
    {
        get => _call.FunctionCall;
        set => _call.FunctionCall = value;
    }

    /// <inheritdoc/>
    public string LastTextOutput => _call.LastTextOutput;

    /// <inheritdoc/>
    public string LastFunctionName => _call.LastFunctionName;

    /// <inheritdoc/>
    public string LastFunctionOutput => _call.LastFunctionOutput;

    /// <inheritdoc/>
    public LLmStreamUpdater Appender => _call.Appender;

    /// <inheritdoc/>
    public void AddFunction(string name, object type, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(nameof(name));
        }

        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        _call.AddFunction(name, type, description);
        _hasFunc = true;
    }

    /// <inheritdoc/>
    public void AppendMessage(LLmMessage msg)
    {
        _call.AppendMessage(msg);
        _textLength += msg?.Message?.Length ?? 0;
    }

    /// <inheritdoc/>
    public void Clear()
    {
        _call.Clear();
        ResetCounter();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _call.Dispose();
    }

    /// <inheritdoc/>
    public async Task<string> Call(CancellationToken cancel, LLmModelParameter parameter = null, LLmCallOption option = null, string title = null)
    {
        double? temperation = BaseLLmCall.GetValidParamValue(parameter?.Temperature ?? (_call as BaseLLmCall)?.Temperature);

        string msg = L("Calling model: ") + title;

        string code = BuildCallInfo(title, temperation);

        DisposableDialogItem msgItem = null;
        LoopedSymbol symbol = null;

        try
        {
            if (_conversation is not null)
            {
                msgItem = _conversation.AddRunningMessage(msg, m =>
                {
                    m.AddCode(code);
                });

                bool supportStreaming = _call.Model?.SupportStreaming == true;

                if (!supportStreaming || _call.HasFunction)
                {
                    symbol = new LoopedSymbol(_conversation);
                }
            }

            var startTime = DateTime.Now;

            var resp = await _call.Call(cancel, parameter, option, title);

            var timeSpan = DateTime.Now - startTime;
            var timeStr = LLmService.FormatTimeSpan(timeSpan);
            _conversation?.AddSystemMessage(L($"Model {L(_presetType.ToDisplayText())} call completed, duration: {timeStr}.")).RemoveOn(3);

            return resp;
        }
        catch (Exception err)
        {
            throw;
        }
        finally
        {
            msgItem?.Dispose();
            symbol?.Dispose();
        }
    }

    private string BuildCallInfo(string title, double? temperation)
    {
        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(title))
        {
            builder.AppendLine(title);
        }

        string modelId = _call.Model?.ModelId ?? _call.ToString();

        builder.AppendLine(L("Model Preset: ") + L(_presetType.ToDisplayText()));
        builder.AppendLine(L("Model Id: ") + modelId);
        if (!string.IsNullOrWhiteSpace(_promptId))
        {
            builder.AppendLine(L("Prompt Id: ") + _promptId);
        }
        builder.AppendLine(L("Request Text Length: ") + _textLength);

        if (temperation.HasValue)
        {
            builder.AppendLine(L("Temperature: ") + temperation.Value);
        }

        if (_hasFunc)
        {
            builder.Append(L("Require format return"));
        }
        else
        {
            builder.Append(L("Require text return"));
        }


        string code = builder.ToString();
        return code;
    }

    /// <inheritdoc/>
    public object GetFunction(string name) => _call.GetFunction(name);

    /// <inheritdoc/>
    public void NewMessage()
    {
        _call.NewMessage();
        ResetCounter();
    }

    #endregion

    // ToString returns _call.ToString

    /// <inheritdoc/>
    public override string ToString() => _call.ToString();
}

/// <summary>
/// Displays a looping animation with elapsed time in the conversation while an LLM call is in progress.
/// </summary>
internal class LoopedSymbol : IDisposable
{
    private static readonly string[] _aniStrAry = [
        ".    ",
        " .   ",
        "  .  ",
        "   . ",
        "    .",
        ];

    private readonly DateTime _startTime;
    private readonly IConversationHandler _conversation;
    private CancellationTokenSource _cancelSource;
    private IDisposable _currentMsg;
    private int _currentIndex = 0;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoopedSymbol"/> class and starts the animation loop.
    /// </summary>
    /// <param name="conversation">The conversation handler to display the animation in.</param>
    public LoopedSymbol(IConversationHandler conversation)
    {
        _conversation = conversation;
        _cancelSource = new CancellationTokenSource();
        _startTime = DateTime.Now;

        Run(_cancelSource.Token);
    }

    private async Task Run(CancellationToken cancel)
    {
        do
        {
            string aniStr = _aniStrAry[_currentIndex];
            _currentIndex++;
            if (_currentIndex >= _aniStrAry.Length)
            {
                _currentIndex = 0;
            }

            var timeSpan = DateTime.Now - _startTime;
            string timeStr = LLmService.FormatTimeSpan(timeSpan);

            _currentMsg?.Dispose();
            _currentMsg = _conversation?.AddSystemMessage($"[{aniStr}] {timeStr}");
            
            await Task.Delay(200);

            if (cancel.IsCancellationRequested)
            {
                break;
            }
        } while (true);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _cancelSource?.Cancel();
        _cancelSource = null;

        _currentMsg?.Dispose();
        _currentMsg = null;
    }
}
