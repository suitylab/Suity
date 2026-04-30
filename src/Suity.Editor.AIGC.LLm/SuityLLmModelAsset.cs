/*using StandardAssets.Aigc;
using Suity.Editor.Services;
using Suity.Json;
using Suity.Views;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC;

public class SuityLLmModelAsset : InternalLLmModelAsset, IPreviewDisplay, ITextDisplay
{
    private AigcModelInfo _info;
    private readonly string _displayText;
    private readonly Image _icon;

    public string ModelId { get; }

    public string Manufacturer { get; }

    public string ModelIcon { get; }

    public LicenseTypes Subscription { get; }

    public SuityLLmModelAsset(AigcModelInfo info)
        : base("Suity_" + info.ModelId.Replace('-', '_'))
    {
        _info = info ?? throw new System.ArgumentNullException(nameof(info));

        ModelId = info.ModelId;
        Manufacturer = info.Manufacturer;
        Description = info.Description;
        ModelIcon = info.Icon;
        PreviewText = info.ToolTips;
        Subscription = (LicenseTypes)(int)info.Subscription;

        if (!string.IsNullOrWhiteSpace(Description))
        {
            _displayText = "[SuityCloud] " + info.Description;
        }
        else
        {
            _displayText = "[SuityCloud] " + ModelId;
        }

        _icon = EditorUtility.GetIconByAssetKey(ModelIcon);

        base.ContextSizeK = info.ContextSize;
    }

    public override string DisplayText => _displayText;

    public override TextStatus DisplayStatus => (int)ServiceInternals._license.LicenseType >= (int)_info.Subscription ? TextStatus.Normal : TextStatus.Disabled;

    public override Image DefaultIcon => _icon;

    object IPreviewDisplay.PreviewIcon => null;

    public override ILLmCall CreateCall(LLmModelParameter config = null, FunctionContext context = null)
    {
        return new SuityLLmCall(_info, config, context);
    }
}

public class SuityLLmCall : BaseLLmCall
{
    private static readonly ServiceStore<AigcServiceModelSender> _sender = new();

    readonly AigcModelInfo _info;

    readonly ValueStore<RunAigcCompletion> _request = new();

    public SuityLLmCall(AigcModelInfo info, LLmModelParameter config, FunctionContext context = null, string text = null) 
        : base(new BasicLLmModel(info.ModelId), config, context, text)
    {
        _info = info;
    }

    public override void NewMessage()
    {
        base.NewMessage();

        var request = new RunAigcCompletion
        {
            ModelId = _info.ModelId,
            ManufactureId = _info.Manufacturer,
            Temperature = (float)LLmModelPlugin.Instance.DefaultParameters.Temperature,
            TopP = (float)LLmModelPlugin.Instance.DefaultParameters.TopP,
            MaxToken = LLmModelPlugin.Instance.DefaultParameters.MaxTokens,
        };

        _request.Set(request);
    }

    public override void AppendMessage(LLmMessage msg)
    {
        if (string.IsNullOrWhiteSpace(msg?.Message))
        {
            return;
        }

        var request = _request.Get();
        if (request is null)
        {
            return;
        }

        switch (msg.Role)
        {
            case LLmMessageRole.System:
                request.Messages.Add(new AigcMessage { Role = AigcMessageRole.System, Content = msg.Message });
                break;

            case LLmMessageRole.User:
                request.Messages.Add(new AigcMessage { Role = AigcMessageRole.User, Content = msg.Message });
                break;

            case LLmMessageRole.Assistant:
                request.Messages.Add(new AigcMessage { Role = AigcMessageRole.Assistant, Content = msg.Message });
                break;
        }
    }

    public override async Task<string> Call(CancellationToken cancel, LLmModelParameter config, LLmCallOption option = null, string title = null)
    {
        var request = _request.Get();
        if (request is null)
        {
            return null;
        }

        var sender = _sender.Get();
        if (sender is null)
        {
            return null;
        }

        //if (IsFunctionCall)
        //{
        //    CreateFunctionCallRequest();
        //}

        string response = null;

        try
        {
            var result = await sender.SendRunAigcCompletion(request).ToTask(cancel);
            LastTextOutput = response = result.Output.Content;
        }
        finally
        {
            if (LogEnabled)
            {
                JsonDataWriter writer = new JsonDataWriter();
                ObjectType.WriteObject(writer, request);

                string requestJson= writer.ToString();
                
                AddToFileLog(requestJson, response);
            }
        }

        cancel.ThrowIfCancellationRequested();

        return response;
    }

    public override string LogPath
    {
        get
        {
            var request = _request.Get();

            string manufactureId = request.ManufactureId ?? string.Empty;

            manufactureId = manufactureId.Trim();

            if (!string.IsNullOrWhiteSpace(manufactureId))
            {
                return manufactureId;
            }

            return "Suity";
        }
    }

}*/