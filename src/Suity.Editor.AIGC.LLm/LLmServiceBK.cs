using Suity.Drawing;
using Suity.Editor.AIGC.Assistants;
using Suity.Editor.Flows;
using Suity.Editor.Flows.AIGC;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

/// <summary>
/// Backend implementation of the LLM service that handles chat operations, model retrieval, and LLM calls.
/// </summary>
internal class LLmServiceBK : LLmService
{
    /// <summary>
    /// Gets the singleton instance of the backend LLM service.
    /// </summary>
    public new static LLmServiceBK Instance { get; } = new();
    private static readonly ServiceStore<IAigcWorkflowRunner> _workflowRunner = new();

    private readonly List<Type> _appenderTypes = [];

    private LLmServiceBK()
    {
        EditorRexes.EditorBeforeAwake.AddActionListener(ScanAssemblies);
    }

    /// <summary>
    /// Initializes the LLM service by registering this instance as the external handler.
    /// </summary>
    public void Initialize()
    {
        LLmService._external = this;
    }

    private void ScanAssemblies()
    {
        foreach (Type appenderType in typeof(LLmStreamUpdater).GetAvailableClassTypes())
        {
            _appenderTypes.Add(appenderType);
        }
    }


    /// <inheritdoc/>
    public override async Task<bool> CheckCurrentModelConfig()
    {
        List<string> msgs = null;
        if (LLmModelPlugin.Instance.GetCurrentModelConfigValid(ref msgs))
        {
            return true;
        }

        string msg = L("AI language model is not configured. Please configure the AI language model first.");
        if (msgs?.Count > 0)
        {
            msg += "\n\n" + string.Join("\n", msgs);
        }

        await DialogUtility.ShowMessageBoxAsync(msg);
        EditorUtility.ShowProjectSetting();

        return false;
    }

    /// <inheritdoc/>
    public override Task<object> StartMainChat(object option)
        => StartChat(MainAssistantChatProvider.Instance, option);

    /// <inheritdoc/>
    public override Task<object> StartChat(ILLmChatProvider provider, object option)
    {
        //EditorUtility.CloseToolWindow(AigcStartupWindow.Instance);
        EditorUtility.ShowToolWindow(AigcChatToolWindow.Instance);

        var chatWnd = AigcChatToolWindow.Instance;

        if (chatWnd != null)
        {
            return chatWnd.StartChat(provider, option);
        }
        else
        {
            return Task.FromResult<object>(null);
        }
    }

    /// <inheritdoc/>
    public override Task<object> InputMainChat(string input, object option = null) 
        => InputChat(MainAssistantChatProvider.Instance, input, option);

    /// <inheritdoc/>
    public override Task<object> InputChat(ILLmChatProvider provider, string input, object option = null)
    {
        //EditorUtility.CloseToolWindow(AigcStartupWindow.Instance);
        EditorUtility.ShowToolWindow(AigcChatToolWindow.Instance);

        var chatWnd = AigcChatToolWindow.Instance;

        if (chatWnd != null)
        {
            return chatWnd.InputChat(provider, input, option);
        }
        else
        {
            return Task.FromResult<object>(null);
        }
    }

    /// <inheritdoc/>
    public override Task<object> StartWorkflowChat(IAigcRunWorkflow runnable, IFlowView view = null, Action<IFlowComputation> config = null)
    {
        //EditorUtility.CloseToolWindow(AigcStartupWindow.Instance);
        EditorUtility.ShowToolWindow(AigcChatToolWindow.Instance);

        return AigcChatToolWindow.Instance.StartWorkflowChat(runnable, view, config);
    }

    /// <inheritdoc/>
    public override Task<object> StartWorkflowTask(AIRequest request, IAigcRunWorkflow runnable, IFlowView view = null, Action<IFlowComputation> config = null)
    {
        var option = new AigcWorkflowOption
        {
            Runnable = runnable,
            View = view,
            Config = config,
        };

        if (_workflowRunner.Get() is { } runner)
        {
            return runner.RunWorkflow(request, option);
        }
        else
        {
            return Task.FromResult<object>(null);
        }
    }

    /// <inheritdoc/>
    public override ILLmModel GetLLmModel(AigcModelLevel level, LLmModelType type)
    {
        if (level != AigcModelLevel.Default)
        {
            return LLmModelPlugin.Instance.GetLLmModel(type, level);
        }
        else
        {
            return LLmModelPlugin.Instance.GetLLmModel(type);
        }
    }

    /// <inheritdoc/>
    public override IImageGenModel GetImageGenModel(AigcModelLevel level)
    {
        if (level != AigcModelLevel.Default)
        {
            return LLmModelPlugin.Instance.GetImageGenModel(level);
        }
        else
        {
            return LLmModelPlugin.Instance.GetImageGenModel();
        }
    }

    /// <inheritdoc/>
    public override IEmbeddingModel GetEmbedding() => LLmModelPlugin.Instance.DefaultEmbedding;


    /// <inheritdoc/>
    public override void SetInput(string msg, IEnumerable<AttachmentSet> attachments = null)
    {
        AigcChatToolWindow.Instance.SetInput(msg, attachments);
    }


    /// <inheritdoc/>
    public override Task<string> Call(ILLmCall call, LLmCallRequest callRequest)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;

        if (r > 1)
        {
            var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

            return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Attempting to call LLM"), async () =>
            {
                NewMessage(call, callRequest);

                return await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
            }, false, r, conversation, callRequest.Cancel);
        }
        else
        {
            NewMessage(call, callRequest);

            return call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
        }
    }


    /// <inheritdoc/>
    public override Task<string> Call(ILLmCall call, LLmCallRequest callRequest, DCompond type)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;

        if (r > 1)
        {
            var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

            return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Attempting to call LLM"), async () =>
            {
                NewMessage(call, callRequest);
                call.AddFunction(type.FullName, type, type.Description);
                // Practice has proved that explicitly specifying the tool type can avoid the problem of incorrect function names returned when LLM calls tools
                call.AppendSystemMessage("Please call tool : " + type.FullName);

                return await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
            }, false, r, conversation, callRequest.Cancel);
        }
        else
        {
            NewMessage(call, callRequest);
            call.AddFunction(type.FullName, type, type.Description);

            return call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
        }
    }

    /// <inheritdoc/>
    public override Task<string> Call(ILLmCall call, LLmCallRequest callRequest, IFunctionCallType type)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;

        if (r > 1)
        {
            var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

            return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Attempting to call LLM"), async () =>
            {
                NewMessage(call, callRequest);
                call.AddFunction(type.FullName, type, type.Description);
                // Practice has proved that explicitly specifying the tool type can avoid the problem of incorrect function names returned when LLM calls tools
                call.AppendSystemMessage("Please call tool : " + type.FullName);

                return await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
            }, false, r, conversation, callRequest.Cancel);
        }
        else
        {
            NewMessage(call, callRequest);
            call.AddFunction(type.FullName, type, type.Description);

            return call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
        }
    }

    /// <inheritdoc/>
    public override Task<string> Call(ILLmCall call, LLmCallRequest callRequest, Type type)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;

        if (r > 1)
        {
            var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

            return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Attempting to call LLM"), async () =>
            {
                NewMessage(call, callRequest);
                call.AddFunction(type.FullName, type);
                // Practice has proved that explicitly specifying the tool type can avoid the problem of incorrect function names returned when LLM calls tools
                call.AppendSystemMessage("Please call tool : " + type.FullName);

                return await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
            }, false, r, conversation, callRequest.Cancel);
        }
        else
        {
            NewMessage(call, callRequest);
            call.AddFunction(type.FullName, type);

            return call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
        }
    }

    static string fixProblemTemplate => @"
{{PROBLEM}}

-----------------------------------
The last generation is as follows:
{{LAST}}
";
    /// <inheritdoc/>
    public override async Task<TConvert> CallConvert<TConvert>(ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, string, TConvert> converter) where TConvert : class
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;
        var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

        string lastResult = null;
        List<string> lastProblems = null;
        string lastRepair = null;

        var final = await AIAssistantService.Instance.DoRetryAction<TConvert>(callRequest.Title ?? L("Attempting to call LLM"), async () =>
        {
            NewMessage(call, callRequest);
            if (lastRepair != null)
            {
                var builder = new PromptBuilder(fixProblemTemplate);
                builder.Replace(TAG.PROBLEM, lastRepair);
                builder.Replace(TAG.LAST, lastResult);
                string problemPrompt = builder.ToString();

                call.AppendUserMessage(problemPrompt);
            }

            string result = await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);
            if (string.IsNullOrWhiteSpace(result))
            {
                throw new AigcException(L("No result returned."));
            }

            TConvert convert = null;

            try
            {
                if (converter != null)
                {
                    convert = converter(callRequest, result);
                }
            }
            catch (AigcRepairException repair)
            {
                if (repair.Problems?.Count > 0)
                {
                    conversation?.AddWarningMessage(L($"{repair.Problems.Count} items need repair.")).RemoveOn(3);
                }
                else
                {
                    conversation?.AddWarningMessage(L("Content needs repair.")).RemoveOn(3);
                }

                lastResult = repair.MergedResult ?? result;
                lastProblems = repair.Problems;
                lastRepair = repair.RepairPrompt;
                throw;
            }
            catch (Exception err)
            {
                conversation.AddException(err);
                throw new AigcException(L("LLM return result validation failed."));
            }

            lastProblems = null;
            lastRepair = null;
            return convert;

        }, false, r, conversation, callRequest.Cancel);

        if (lastProblems != null)
        {
            conversation?.AddErrorMessage(L("Content cannot be repaired"), m => m.AddCode(string.Join("\r\n", lastProblems)));
        }

        return final;
    }


    /// <inheritdoc/>
    public override Task<T> CallFunction<T>(ILLmCall call, LLmCallRequest callRequest, Predicate<T> verifier = null) where T : class
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;
        var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

        return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Call LLM with format return"), async () =>
        {
            NewMessage(call, callRequest);
            call.AddFunction(typeof(T).FullName, typeof(T), null);
            call.FunctionCall = typeof(T).FullName;
            // Practice has proved that explicitly specifying the tool type can avoid the problem of incorrect function names returned when LLM calls tools
            call.AppendSystemMessage("Please call tool : " + typeof(T).FullName);

            var textResult = await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);

            string funcName = call.LastFunctionName;
            if (string.IsNullOrWhiteSpace(funcName))
            {
                throw new AigcException(L("LLM did not return tool type info: ") + call.ToString());
            }

            string funcResult = ExtractCodeBlock(call.LastFunctionOutput);
            if (string.IsNullOrWhiteSpace(funcResult))
            {
                throw new AigcException(L("LLM did not return tool info: ") + call.ToString());
            }

            T result = null;

            try
            {
                result = EditorServices.JsonSchemaService.GetObject<T>(funcResult);
            }
            catch (Exception err)
            {
                var request = new AIRequest
                {
                    Conversation = conversation,
                    FuncContext = call.Context,
                };

                // Try to repair
                funcResult = await AIAssistantService.Instance.RepairJson(request, funcResult);
            }

            if (result is null)
            {
                try
                {
                    result = EditorServices.JsonSchemaService.GetObject<T>(funcResult);
                }
                catch (Exception err)
                {
                    conversation.AddException(err);
                    throw new AigcException(L("Failed to parse LLM result Json: ") + call.ToString(), err);
                }
            }

            if (result is null)
            {
                throw new AigcException(L("LLM did not return valid info: ") + call.ToString());
            }

            try
            {
                if (verifier != null && !verifier(result))
                {
                    throw new AigcException(L("LLM return result validation failed: ") + call.ToString());
                }
            }
            catch (Exception err)
            {
                conversation.AddException(err);
                throw new AigcException(L("LLM return result validation failed: ") + call.ToString());
            }

            return result;

        }, false, r, conversation, callRequest.Cancel);
    }

    /// <inheritdoc/>
    public override async Task<TConvert> CallFunctionConvert<T, TConvert>(ILLmCall call, LLmCallRequest callRequest, Func<LLmCallRequest, T, TConvert> converter)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;
        var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

        string lastResult = null;
        List<string> lastProblems = null;
        string lastRepair = null;

        var final = await AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Call LLM with format return"), async () =>
        {
            NewMessage(call, callRequest);
            if (lastRepair != null)
            {
                var builder = new PromptBuilder(fixProblemTemplate);
                builder.Replace(TAG.PROBLEM, lastRepair);
                builder.Replace(TAG.LAST, lastResult); //TODO: ToJson
                string problemPrompt = builder.ToString();

                call.AppendUserMessage(problemPrompt);
            }

            call.AddFunction(typeof(T).FullName, typeof(T), null);
            call.FunctionCall = typeof(T).FullName;
            // Practice has proved that explicitly specifying the tool type can avoid the problem of incorrect function names returned when LLM calls tools
            call.AppendSystemMessage("Please call tool : " + typeof(T).FullName);

            var textResult = await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);

            string funcName = call.LastFunctionName;
            if (string.IsNullOrWhiteSpace(funcName))
            {
                throw new AigcException(L("LLM did not return tool type info: ") + call.ToString());
            }

            string funcResult = ExtractCodeBlock(call.LastFunctionOutput);
            if (string.IsNullOrWhiteSpace(funcResult))
            {
                throw new AigcException(L("LLM did not return tool info: ") + call.ToString());
            }

            T result = null;

            try
            {
                result = EditorServices.JsonSchemaService.GetObject<T>(funcResult);
            }
            catch (Exception err)
            {
                var request = new AIRequest
                {
                    Conversation = conversation,
                    FuncContext = call.Context,
                };

                // Try to repair
                funcResult = await AIAssistantService.Instance.RepairJson(request, funcResult);
            }

            if (result is null)
            {
                try
                {
                    result = EditorServices.JsonSchemaService.GetObject<T>(funcResult);
                }
                catch (Exception err)
                {
                    conversation.AddException(err);
                    throw new AigcException(L("Failed to parse LLM result Json: ") + call.ToString(), err);
                }
            }

            if (result is null)
            {
                throw new AigcException(L("LLM did not return valid info: ") + call.ToString());
            }

            TConvert convert = null;

            try
            {
                if (converter != null)
                {
                    convert = converter(callRequest, result);
                }
            }
            catch (AigcRepairException repair)
            {
                if (repair.Problems?.Count > 0)
                {
                    conversation?.AddWarningMessage(L($"{repair.Problems.Count} items need repair.")).RemoveOn(3);
                }
                else
                {
                    conversation?.AddWarningMessage(L("Content needs repair")).RemoveOn(3);
                }
                    
                lastResult = repair.MergedResult ?? funcResult;
                lastProblems = repair.Problems;
                lastRepair = repair.RepairPrompt;
                throw;
            }
            catch (Exception err)
            {
                conversation.AddException(err);
                throw new AigcException(L("LLM return result validation failed."));
            }

            lastProblems = null;
            lastRepair = null;
            return convert;

        }, false, r, conversation, callRequest.Cancel);

        if (lastProblems != null)
        {
            conversation?.AddErrorMessage(L("Content cannot be repaired"), m => m.AddCode(string.Join("\r\n", lastProblems)));
        }

        return final;
    }

    /// <inheritdoc/>
    public override Task<object> CallFunction(ILLmCall call, LLmCallRequest callRequest, Type[] types, Predicate<object> verifier = null)
    {
        int r = callRequest.RetryCount ?? AIAssistantService.Config.RetryCount;
        var conversation = callRequest.Conversation ?? call.Context?.GetArgument<IConversationHandler>();

        return AIAssistantService.Instance.DoRetryAction(callRequest.Title ?? L("Call LLM with format return"), async () =>
        {
            NewMessage(call, callRequest);

            foreach (var type in types)
            {
                call.AddFunction(type.FullName, type, null);
            }


            var textResult = await call.Call(callRequest.Cancel, callRequest.Parameter, callRequest.Option, callRequest.Title);

            string funcName = call.LastFunctionName;
            if (string.IsNullOrWhiteSpace(funcName))
            {
                throw new AigcException((L("LLM did not return tool type info: ")) + call.ToString());
            }

            string funcResult = ExtractCodeBlock(call.LastFunctionOutput);
            if (string.IsNullOrWhiteSpace(funcResult))
            {
                throw new AigcException(L("LLM did not return tool info: ") + call.ToString());
            }

            var toolType = types.Where(o => o.FullName == funcName).FirstOrDefault();
            if (toolType is null)
            {
                throw new AigcException(L("Cannot get tool type: ") + funcName);
            }

            object result = null;

            try
            {
                // TODO: Implement dynamic type parsing
                result = EditorServices.JsonSchemaService.GetObject(toolType, funcResult);
            }
            catch (Exception err)
            {
                //TODO: Try to repair Json

                conversation.AddException(err);
                throw new AigcException(L("Failed to parse LLM result Json: ") + call.ToString(), err);
            }

            if (result is null)
            {
                throw new AigcException(L("LLM did not return valid info: ") + call.ToString());
            }

            try
            {
                if (verifier != null && !verifier(result))
                {
                    throw new AigcException(L("LLM return result validation failed: ") + call.ToString());
                }
            }
            catch (Exception err)
            {
                conversation.AddException(err);
                throw new AigcException(L("LLM return result validation failed: ") + call.ToString());
            }

            return result;

        }, false, r, conversation, callRequest.Cancel);
    }

    /// <inheritdoc/>
    public override async Task<BitmapDef> GenerateImage(string input, AigcModelLevel level = AigcModelLevel.Default, ImageAspectRatio aspectRatio = ImageAspectRatio.Default)
    {
        var option = new ImageGenOptions
        {
            ModelLevel = level,
            AspectRatio = aspectRatio,
        };

        var assistant = new ImageGenAssistant();
        var result = (await LLmService.Instance.InputMainChat(input, assistant, option)) as AICallResult;
        if (result?.Result is BitmapDef img)
        {
            return img;
        }

        return null;
    }

    /// <inheritdoc/>
    public override string ExtractCodeBlock(string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown))
        {
            return string.Empty;
        }

        // Regex to match Markdown code blocks, supporting multi-line code starting with ```
        var match = Regex.Match(markdown, "```(?:[a-zA-Z0-9]*)\n([\\s\\S]+?)\n```", RegexOptions.Multiline);

        return match.Success ? match.Groups[1].Value.Trim() : markdown;
    }

    /// <inheritdoc/>
    public override SObject ResolveSObjectOutput(ILLmCall call)
    {
        string name = call.LastFunctionName;
        string args = call.LastFunctionOutput;

        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        if (string.IsNullOrWhiteSpace(args))
        {
            return null;
        }

        var func = call.GetFunction(name);
        var dcomp = BaseLLmCall.ResolveDCompond(func);
        if (dcomp is null)
        {
            return null;
        }

        try
        {
            var obj = ComputerBeacon.Json.Parser.Parse(args);
            return EditorServices.JsonResource.FromJson(obj, new() { TypeHint = dcomp?.Definition }) as SObject;
        }
        catch (Exception err)
        {
            var c = call.Context?.GetArgument<IConversationHandler>();
            c?.AddErrorMessage(L("Function parsing failed: ") + name);
            //err.LogError($"Function parsing failed: {funcCall.Name}");

            return null;
        }
    }

    /// <inheritdoc/>
    public override LLmStreamUpdater CreateLLmStreamAppender(IConversationHandler conversation)
    {
        if (_appenderTypes.FirstOrDefault() is { } appenderType)
        {
            try
            {
                var appender = Activator.CreateInstance(appenderType) as LLmStreamUpdater;
                if (appender != null)
                {
                    appender.Conversation = conversation;
                }

                return appender;
            }
            catch (Exception err)
            {
                err.LogError();

                return new CounterLLmStreamAppender() { Conversation = conversation };
            }
        }

        return new CounterLLmStreamAppender() { Conversation = conversation };
    }

    /// <inheritdoc/>
    public override IDisposable CreateLoopedSymbol(IConversationHandler conversation)
    {
        if (conversation is null)
        {
            return null;
        }

        return new LoopedSymbol(conversation);
    }

    /// <inheritdoc/>
    public override string LocalizedSpeechLanguage
    {
        get
        {
            string lang = LLmModelPlugin.Instance.LocalizedLanguage?.Trim();
            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = EditorServices.LocalizationService.LanguageName;
            }
            if (string.IsNullOrWhiteSpace(lang))
            {
                lang = "English";
            }

            return lang;
        }
    }

    private static void NewMessage(ILLmCall call, LLmCallRequest req)
    {
        call.NewMessage();

        foreach (var msg in req.Messages)
        {
            call.AppendMessage(msg);
        }
    }
}

/// <summary>
/// A counter-based LLM stream appender that displays the current text length as a system message.
/// </summary>
internal class CounterLLmStreamAppender : LLmStreamUpdater
{
    private DisposableDialogItem _msg;

    /// <inheritdoc/>
    protected override void UpdateConversation(IConversationHandler conversation, string text, StringBuilder fullText)
    {
        _msg?.Dispose();
        _msg = conversation.AddSystemMessage(fullText.Length.ToString());
    }

    /// <inheritdoc/>
    public override void Dispose()
    {
        _msg?.Dispose();
    }
}