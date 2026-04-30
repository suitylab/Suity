using Suity.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

/// <summary>
/// A manual LLM model asset that allows users to provide responses manually instead of calling an external LLM.
/// </summary>
[AssetAutoCreate]
[DisplayText("Manual Response")]
public class ManualLLmModelAsset : InternalLLmModelAsset
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ManualLLmModelAsset"/> class.
    /// </summary>
    public ManualLLmModelAsset()
        : base("*AIGC|ManualLLmCall")
    {
        Description = "Manual response";
    }

    /// <inheritdoc/>
    public override bool IsManual => true;

    /// <inheritdoc/>
    public override ILLmCall CreateCall(LLmModelParameter config = null, FunctionContext context = null)
    {
        return new ManualLLmCall(config, context);
    }
}

/// <summary>
/// Internal LLM call implementation that prompts users to manually provide responses via clipboard.
/// </summary>
internal class ManualLLmCall : BaseLLmCall
{
    private readonly List<LLmMessage> _msgs = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="ManualLLmCall"/> class.
    /// </summary>
    /// <param name="config">Optional LLM model parameters.</param>
    /// <param name="context">Optional function context.</param>
    public ManualLLmCall(LLmModelParameter config = null, FunctionContext context = null)
        : base(new BasicLLmModel("Manual") { IsManual = true }, config, context)
    {
    }

    /// <inheritdoc/>
    public override void NewMessage()
    {
        base.NewMessage();

        _msgs.Clear();
    }

    /// <inheritdoc/>
    public override void AppendMessage(LLmMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg?.Message))
        {
            return;
        }

        _msgs.Add(msg);
    }

    /// <inheritdoc/>
    public override async Task<string> Call(CancellationToken cancel, LLmModelParameter config, LLmCallOption option = null, string title = null)
    {
        var c = GetConversation()
            ?? throw new NullReferenceException($"{nameof(IConversationHandler)} not found.");

        if (_msgs.Count == 0)
        {
            return string.Empty;
        }

        AddManualFunctionPrompt();

        string text;
        if (_msgs.Count == 1)
        {
            text = _msgs[0].Message;
        }
        else
        {
            text = LLmMessage.CombineText(_msgs);
        }

        string response = null;
        var msgItem = c.AddSystemMessage(L("Please copy this prompt to an external language model, then paste the result back to the message input box and send."), o =>
        {
            o.AddButton("Copy", L("Copy"), () => EditorUtility.SetSystemClipboardText(text));
            o.AddText(L("After generation is complete, copy the generated text, then click the 'Paste' button below."));
            o.AddButton("Paste", L("Paste"), () =>
            {
                EditorUtility.GetSystemClipboardText().ContinueWith(t => 
                {
                    response = t.Result;
                    var host = Context?.GetArgument<IConversationHostAsync>();
                    host?.HandleButtonClickAsync("Paste", cancel);
                });
            });
        });

        await c.WaitForButtonInput(["Paste"], cancel);
        cancel.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        msgItem.Dispose();

        try
        {
            LastTextOutput = response;
            cancel.ThrowIfCancellationRequested();
            ProcessManualFunctionCall();
        }
        finally
        {
            AddToFileLog(text, response);
        }

        return LastTextOutput;

    }
}