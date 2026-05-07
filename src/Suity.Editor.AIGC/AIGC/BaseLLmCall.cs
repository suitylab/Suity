using ComputerBeacon.Json;
using Newtonsoft.Json;
using Suity.Collections;
using Suity.Editor.AIGC.Helpers;
using Suity.Editor.Services;
using Suity.Editor.Types;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Views;
using Suity.Views.Im;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Suity.Helpers.GlobalLocalizer;

namespace Suity.Editor.AIGC;

/// <summary>
/// Represents a function item for LLM calls, containing the function name, description, and type schema.
/// </summary>
public record LLmFunctionItem(string Name, string Description, object Type);

/// <summary>
/// Abstract base class for LLM call implementations, providing common functionality for managing functions, messages, and model interactions.
/// </summary>
public abstract class BaseLLmCall : ILLmCall
{
    #region static config

    internal static bool _aiCallLog;

    internal static string _funcCallPrefix = @"
Call the function. Function schema is described below:
";

    internal static string _funcCallList = @"
Select and call an suitable functions. Function schema is described below:
";

    internal static string _funcCallSuffix = @"
Export pure json code using the function schema without any '//' comments.
";

    internal static LLmModelParameter DefaultLLmConfig;

    #endregion

    private readonly ILLmModel _model;
    private readonly string _name;
    private readonly string _text;

    private readonly FunctionContext _context;
    private readonly LLmModelParameter _config;

    private Dictionary<string, LLmFunctionItem> _functions;
    private string _functionCall;

    private string _lastTextOutput;
    private string _lastFunctionName;
    private string _lastFunctionOutput;

    /// <summary>
    /// Gets the temperature setting for LLM generation.
    /// </summary>
    public virtual double? Temperature => _config?.Temperature ?? DefaultLLmConfig.Temperature;

    /// <summary>
    /// Gets the top-p (nucleus sampling) setting for LLM generation.
    /// </summary>
    public virtual double? TopP => _config?.TopP ?? DefaultLLmConfig.TopP;

    /// <summary>
    /// Gets the presence penalty setting for LLM generation.
    /// </summary>
    public virtual double? PresencePenalty => _config?.PresencePenalty ?? DefaultLLmConfig.PresencePenalty;

    /// <summary>
    /// Gets the frequency penalty setting for LLM generation.
    /// </summary>
    public virtual double? FrequencyPenalty => _config?.FrequencyPenalty ?? DefaultLLmConfig.FrequencyPenalty;

    /// <summary>
    /// Gets the maximum number of tokens for LLM generation.
    /// </summary>
    public virtual int? MaxTokens => _config?.MaxTokens ?? DefaultLLmConfig.MaxTokens;

    /// <summary>
    /// Gets the function context associated with this LLM call.
    /// </summary>
    public FunctionContext Context => _context;

    /// <summary>
    /// Gets the LLM model parameter configuration.
    /// </summary>
    public LLmModelParameter Config => _config;

    /// <summary>
    /// Gets the number of registered functions.
    /// </summary>
    public int FunctionCount => _functions?.Count ?? 0;

    /// <summary>
    /// Gets a value indicating whether any functions are registered.
    /// </summary>
    public bool HasFunction => _functions?.Count > 0;

    /// <summary>
    /// Gets or sets the name of the function to call.
    /// </summary>
    public string FunctionCall { get => _functionCall; set => SetFunctionCall(value); }

    /// <summary>
    /// Gets the last text output from the LLM call.
    /// </summary>
    public string LastTextOutput { get => _lastTextOutput; protected set => _lastTextOutput = value; }

    /// <summary>
    /// Gets the name of the last function that was called.
    /// </summary>
    public string LastFunctionName { get => _lastFunctionName; protected set => _lastFunctionName = value; }

    /// <summary>
    /// Gets the output from the last function call.
    /// </summary>
    public string LastFunctionOutput { get => _lastFunctionOutput; protected set => _lastFunctionOutput = value; }

    /// <summary>
    /// Gets the stream appender for streaming LLM responses.
    /// </summary>
    public LLmStreamUpdater Appender { get; protected set; }

    /// <summary>
    /// Gets a value indicating whether logging is enabled.
    /// </summary>
    public virtual bool LogEnabled => _aiCallLog;

    /// <summary>
    /// Gets the path used for log file storage.
    /// </summary>
    public virtual string LogPath => this.GetType().Name;

    /// <summary>
    /// Initializes a new instance of the <see cref="BaseLLmCall"/> class.
    /// </summary>
    /// <param name="model">The LLM model to use for calls.</param>
    /// <param name="config">Optional model parameter configuration.</param>
    /// <param name="context">Optional function context for resolving dependencies.</param>
    /// <param name="text">Optional display text for this call.</param>
    protected BaseLLmCall(ILLmModel model, LLmModelParameter config = null, FunctionContext context = null, string text = null)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        _name = _model.ModelId ?? string.Empty;
        _config = config;
        _context = context ?? new();
        _text = text;
    }

    /// <summary>
    /// Gets the LLM model used for calls.
    /// </summary>
    public ILLmModel Model => _model;

    /// <summary>
    /// Starts a new message by clearing function-related state.
    /// </summary>
    public virtual void NewMessage()
    {
        _functions?.Clear();
        _functionCall = null;
        _lastTextOutput = null;
        _lastFunctionName = null;
        _lastFunctionOutput = null;
    }

    /// <summary>
    /// Appends a message to the conversation. Override in derived classes to implement message handling.
    /// </summary>
    /// <param name="msg">The message to append.</param>
    public virtual void AppendMessage(LLmMessage msg)
    { }

    /// <summary>
    /// Registers a function that can be called by the LLM.
    /// </summary>
    /// <param name="name">The name of the function.</param>
    /// <param name="type">The type schema of the function.</param>
    /// <param name="description">A description of what the function does.</param>
    public virtual void AddFunction(string name, object type, string description)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentNullException(name);
        }

        _functions ??= [];

        if (!_functions.ContainsKey(name))
        {
            var item = new LLmFunctionItem(name, description, type);

            _functions.Add(name, item);
        }
    }

    /// <summary>
    /// Executes the LLM call asynchronously. Override in derived classes to implement the actual call logic.
    /// </summary>
    /// <param name="cancel">Cancellation token to cancel the operation.</param>
    /// <param name="config">Model parameter configuration for this call.</param>
    /// <param name="option">Optional call options.</param>
    /// <param name="title">Optional title for the call.</param>
    /// <returns>A task representing the async operation, returning the text output.</returns>
    public virtual Task<string> Call(CancellationToken cancel, LLmModelParameter config, LLmCallOption option = null, string title = null)
    {
        _lastTextOutput = string.Empty;
        return Task.FromResult<string>(string.Empty);
    }

    /// <summary>
    /// Gets the names of all registered functions.
    /// </summary>
    public IEnumerable<string> Functions => _functions?.Keys ?? Enumerable.Empty<string>();

    /// <summary>
    /// Gets the function item by name.
    /// </summary>
    /// <param name="name">The name of the function to retrieve.</param>
    /// <returns>The function item, or null if not found.</returns>
    public object GetFunction(string name) => _functions?.GetValueSafe(name);

    /// <summary>
    /// Renders a settings UI for this LLM call. Override in derived classes to add custom UI.
    /// </summary>
    /// <param name="gui">The ImGui context to render into.</param>
    public virtual void OnSettingGui(ImGui gui)
    {
    }

    /// <summary>
    /// Clears the state of this LLM call. Override in derived classes to implement custom clearing logic.
    /// </summary>
    public virtual void Clear()
    {
    }

    /// <summary>
    /// Releases resources used by this LLM call. Override in derived classes to implement custom disposal logic.
    /// </summary>
    public virtual void Dispose()
    {
    }

    /// <summary>
    /// Gets the conversation handler from the function context.
    /// </summary>
    /// <returns>The conversation handler, or null if not available.</returns>
    protected IConversationHandler GetConversation() => _context.GetArgument<IConversationHandler>();

    /// <summary>
    /// Writes the request and response to log files.
    /// </summary>
    /// <param name="request">The request content to log.</param>
    /// <param name="response">The response content to log.</param>
    /// <param name="json">Whether to use .json extension (true) or .txt (false).</param>
    protected void AddToFileLog(string request, string response, bool json = true)
    {
        if (!LogEnabled)
        {
            return;
        }

        string logPath = LogPath;
        if (string.IsNullOrWhiteSpace(logPath))
        {
            logPath = "Unknown";
        }

        var now = DateTime.Now;
        string nowDate = now.ToString("yyyy-MM-dd");
        string nowStr = now.ToString("yyyy-MM-dd_HH-mm-ss");

        string ext = json ? ".json" : ".txt";

        if (!string.IsNullOrWhiteSpace(request))
        {
            string rFileName = $"LLmLog/{logPath}/{nowDate}/{nowStr}_req.{ext}";
            string fileName = Project.Current.UserDirectory.PathAppend(rFileName);
            try
            {
                FileUtils.Write(fileName, request);
            }
            catch (Exception err)
            {
                err.LogError(L("Write file failed: ") + fileName);
            }
        }

        if (!string.IsNullOrWhiteSpace(response))
        {
            string rFileName = $"LLmLog/{logPath}/{nowDate}/{nowStr}_resp.{ext}";
            string fileName = Project.Current.UserDirectory.PathAppend(rFileName);
            try
            {
                FileUtils.Write(fileName, response);
            }
            catch (Exception err)
            {
                err.LogError(L("Write file failed: ") + fileName);
            }
        }
    }

    [Obsolete]
    protected void AppendSObjectCallMessage()
    {
        var builder = new StringBuilder();

        bool isFuncCall = false;

        if (_functions?.Count > 0)
        {
            string listPrompt = _funcCallList;
            if (string.IsNullOrWhiteSpace(listPrompt))
            {
                listPrompt = _funcCallList;
            }

            builder.AppendLine();
            builder.AppendLine();
            builder.AppendLine(listPrompt);
            foreach (var func in _functions)
            {
                builder.AppendLine(func.ToSchemaOverview());
            }

            isFuncCall = true;
        }

        if (isFuncCall)
        {
            string suffix = _funcCallSuffix;
            if (string.IsNullOrWhiteSpace(suffix))
            {
                suffix = _funcCallSuffix;
            }

            builder.AppendLine(suffix);
        }

        var msg = new LLmMessage
        {
            Role = LLmMessageRole.User,
            Message = builder.ToString(),
        };

        AppendMessage(msg);
    }

    [Obsolete]
    protected SObject ResolveSObjectOutput(string name, string text)
    {
        var functionCall = _functions?.GetValueSafe(name);

        if (EditorServices.JsonResource.TryExtractJson(text, out var obj))
        {
            // Adjust the structure of obj
            if (obj["@type"] is null && obj["parameters"] is JsonObject parameters)
            {
                obj = parameters;
            }

            TypeDefinition typeDef;
            if (functionCall.Type is TypeDefinition t)
            {
                typeDef = t;
            }
            else if (functionCall.Type is DType dtype)
            {
                typeDef = dtype.Definition;
            }
            else
            {
                typeDef = null;
            }

            return EditorServices.JsonResource.FromJson(obj, new SItemResourceOptions() { TypeHint = typeDef }) as SObject;
        }
        else
        {
            var c = GetConversation();
            c?.AddErrorMessage(L("Resolving function failed."));

            return null;
        }
    }

    /// <summary>
    /// Sets the function to call. Throws if the function name is not registered.
    /// </summary>
    /// <param name="name">The name of the function to set.</param>
    protected virtual void SetFunctionCall(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(L($"'{nameof(name)}' cannot be null or white space."), nameof(name));
        }

        _functionCall = name;
    }

    /// <summary>
    /// Adds prompts to the conversation for manual function calling, providing schema information to the LLM.
    /// </summary>
    protected virtual void AddManualFunctionPrompt()
    {
        if (_functions is null || _functions.Count == 0)
        {
            return;
        }

        const string promptFormat = @"
Please output the result using json format and wrapped in a 'call' xml tag, with the following format:
<call name=""function name"">
{
   json content...
}
</call>
";

        const string promptSingle = @"
Please provide the json result of the following function call schema:
";
        const string promptMultiple = @"
Please select one of the following functions and provide the json result with the selected function call schema:
";

        bool singleFunc = !string.IsNullOrWhiteSpace(_functionCall) && _functions?.ContainsKey(_functionCall) == true;

        if (singleFunc)
        {
            var item = _functions[_functionCall];
            string tag = ResolveSchemaTag(item.Type, item.Name, item.Description, _context);
            if (!string.IsNullOrWhiteSpace(tag))
            {
                var msg = new LLmMessage
                {
                    Role = LLmMessageRole.User,
                    Message = promptFormat + promptSingle + tag,
                };

                AppendMessage(msg);
            }
        }
        else
        {
            List<string> tags = [];
            foreach (var item in _functions)
            {
                string tag = ResolveSchemaTag(item.Value.Type, item.Value.Name, item.Value.Description, _context);
                if (!string.IsNullOrWhiteSpace(tag))
                {
                    tags.Add(tag);
                }
            }

            if (tags.Count > 0)
            {
                var msg = new LLmMessage
                {
                    Role = LLmMessageRole.User,
                    Message = promptFormat + promptMultiple + string.Join("\n", tags),
                };

                AppendMessage(msg);
            }
        }
    }

    /// <summary>
    /// Processes the last text output to extract function call information from XML tags.
    /// </summary>
    protected virtual void ProcessManualFunctionCall()
    {
        if (!HasFunction)
        {
            return;
        }

        do
        {
            string result = LastTextOutput?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(result))
            {
                break;
            }

            var nodes = LooseXml.ExtractNodes(result, "call");
            if (nodes is null || nodes.Length == 0)
            {
                break;
            }

            var node = nodes[0];
            LastFunctionName = node.GetAttribute("name");
            LastFunctionOutput = node.InnerText;
        } while (false);
    }

    public override string ToString() => _text ?? _name;

    #region Static

    /// <summary>
    /// Resolves a type to an XML schema tag string for use in LLM prompts.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <param name="name">The function name.</param>
    /// <param name="description">The function description.</param>
    /// <param name="contxt">Optional function context.</param>
    /// <returns>An XML schema tag string, or null if resolution fails.</returns>
    public static string ResolveSchemaTag(object type, string name, string description, FunctionContext contxt = null)
    {
        var schema = ResolveSchema(type, ref name, ref description, contxt);
        if (schema is null)
        {
            return null;
        }

        string schemaStr = JsonConvert.SerializeObject(schema, Formatting.Indented);
        if (string.IsNullOrWhiteSpace(schemaStr))
        {
            return null;
        }

        return $"<schema name=\"{name}\" desc=\"{description}\">\n{schemaStr}\n</schema>";
    }

    /// <summary>
    /// Resolves a type to a JSON schema object for use in LLM function calls.
    /// </summary>
    /// <param name="type">The type to resolve.</param>
    /// <param name="name">The function name (updated if null).</param>
    /// <param name="description">The function description (updated if null).</param>
    /// <param name="contxt">Optional function context.</param>
    /// <returns>A JSON schema object, or null if resolution fails.</returns>
    public static object ResolveSchema(object type, ref string name, ref string description, FunctionContext contxt = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        object prop = null;

        if (type is IFunctionCallType funcCallType)
        {
            prop = funcCallType.GetSchema(contxt);
            name ??= funcCallType.FullName;
            description ??= funcCallType.Description;
        }
        else if (type is Type tType)
        {
            prop = GetSchemaProperty(tType);
            name ??= tType.FullName;
            description ??= tType.GetAttributeCached<DescriptionAttribute>()?.Description;
        }
        else if (ResolveDCompond(type) is { } dcomp)
        {
            prop = EditorServices.JsonSchemaService.CreateSchemaProperty(dcomp);
            name ??= dcomp.FullName;
            description ??= dcomp.Description;
        }
        else if (type is SimpleType simpleType)
        {
            prop = EditorServices.JsonSchemaService.CreateSchemaProperty(simpleType);
            name ??= simpleType.Name;
            description ??= simpleType.Tooltips;
        }
        else if (type is string str)
        {
            prop = str;
        }
        else
        {
            // Can be a JObject of Newtonsoft.Json
            prop = type;
        }

        return prop;
    }

    /// <summary>
    /// Resolves an object to a DCompond type definition.
    /// </summary>
    /// <param name="obj">The object to resolve.</param>
    /// <returns>The resolved DCompond, or null if not resolvable.</returns>
    public static DCompond ResolveDCompond(object obj)
    {
        if (obj is DCompond d)
        {
            return d;
        }
        else if (obj is TypeDefinition t)
        {
            return t.Target as DCompond;
        }
        else if (obj is LLmFunctionItem funcItem && funcItem.Type is { } type)
        {
            return ResolveDCompond(type);
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get the valid parameter and return a valid value only when the parameter is > 0, otherwise return null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static double? GetValidParamValue(double? value)
    {
        if (value is { } vValue && vValue > 0)
        {
            return vValue;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets a valid float parameter value, returning null if the value is not greater than 0.
    /// </summary>
    /// <param name="value">The parameter value to validate.</param>
    /// <returns>The value as a float if greater than 0, otherwise null.</returns>
    public static float? GetValidParamValueF(double? value)
    {
        if (value is { } vValue && vValue > 0)
        {
            return (float)vValue;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Get the valid parameter and return a valid value only when the parameter is > 0, otherwise return null
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    public static int? GetValidParamValue(int? value)
    {
        if (value is { } vValue && vValue > 0)
        {
            return vValue;
        }
        else
        {
            return null;
        }
    }

    private static readonly Dictionary<Type, object> _schemaCache = [];

    /// <summary>
    /// Creates a JSON schema property for the given type, with optional field descriptions.
    /// </summary>
    /// <param name="type">The type to create a schema for.</param>
    /// <param name="fieldDescriptions">Optional dictionary of field names to descriptions.</param>
    /// <returns>A JSON schema property object.</returns>
    public static object GetSchemaProperty(Type type, IDictionary<string, string> fieldDescriptions = null)
    {
        if (type is null)
        {
            throw new ArgumentNullException(nameof(type));
        }

        if (fieldDescriptions != null)
        {
            return EditorServices.JsonSchemaService.CreateSchemaProperty(type);
        }

        return _schemaCache.GetOrAdd(type, t => EditorServices.JsonSchemaService.CreateSchemaProperty(t));
    }

    #endregion
}