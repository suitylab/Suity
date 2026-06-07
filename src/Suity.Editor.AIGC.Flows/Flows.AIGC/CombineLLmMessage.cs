using Suity.Editor.AIGC;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System.Text;

namespace Suity.Editor.Flows.AIGC;

[SimpleFlowNodeStyle(Color = FlowColors.LLm, HasHeader = false)]
[DisplayText("Combine LLM Message", "*CoreIcon|Chat")]
[ToolTipsText("Combine multiple LLM messages into one.")]
public class CombineLLmMessage : AigcFlowNode
{
    private readonly FlowNodeConnector _messages;
    private readonly FlowNodeConnector _output;

    private readonly ValueProperty<LLmMessageRole> _role = new("Role", "Combined Role", LLmMessageRole.Assistant);
    private readonly StringProperty _prefix = new("Prefix", "Prefix", toolTips: "The beginning of the entire message.");
    private readonly StringProperty _suffix = new("Suffix", "Suffix", toolTips: "The end of the entire message.");
    private readonly StringProperty _itemPrefix = new("ItemPrefix", "Item Prefix", toolTips: "The beginning of a single message. Use {ROLE} for role replacement.");
    private readonly StringProperty _itemSuffix = new("ItemSuffix", "Item Suffix", toolTips: "The end of a single message. Use {ROLE} for role replacement.");

    public CombineLLmMessage()
    {
        var msgType = TypeDefinition.FromNative<LLmMessage>();

        _messages = AddConnector("Messages", msgType, FlowDirections.Input, FlowConnectorTypes.Data, true, "Messages");
        _output = AddDataOutputConnector("Output", msgType, "Combined");
    }

    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        _role.Sync(sync);
        _prefix.Sync(sync);
        _suffix.Sync(sync);
        _itemPrefix.Sync(sync);
        _itemSuffix.Sync(sync);
    }

    protected override void OnSetupView(IViewObjectSetup setup)
    {
        base.OnSetupView(setup);

        _role.InspectorField(setup);
        _prefix.InspectorField(setup);
        _suffix.InspectorField(setup);
        _itemPrefix.InspectorField(setup);
        _itemSuffix.InspectorField(setup);
    }

    public override void Compute(IFlowComputation compute)
    {
        LLmMessage[] msgs = compute.GetValues<LLmMessage>(_messages);

        var builder = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(_prefix.Text))
        {
            builder.AppendLine(_prefix.Text);
        }

        string itemPrefix = _itemPrefix.Text;
        string itemSuffix = _itemSuffix.Text;
        
        string resolvePrefix(LLmMessageRole role)
        {
            if (!string.IsNullOrWhiteSpace(itemPrefix))
            {
                return itemPrefix.Replace("{ROLE}", role.ToString());
            }

            return null;
        }

        string resolveSuffix(LLmMessageRole role)
        {
            if (!string.IsNullOrWhiteSpace(itemSuffix))
            {
                return itemSuffix.Replace("{ROLE}", role.ToString());
            }

            return null;
        }

        string msg = LLmMessage.CombineText(msgs, resolvePrefix, resolveSuffix);
        builder.AppendLine(msg);
        
        if (!string.IsNullOrWhiteSpace(_suffix.Text))
        {
            builder.AppendLine(_suffix.Text);
        }

        string text = builder.ToString();
        var output = new LLmMessage { Role = _role.Value, Message = text };

        compute.SetValue(_output, output);
    }
}
