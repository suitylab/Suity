using Suity.Drawing;
using Suity.Editor.Flows.SubFlows;
using Suity.Editor.Types;
using Suity.Views;
using System.Threading.Tasks;

namespace Suity.Editor;

[NotAvailable]
public class ToolCommandAsset<TInput, TOutput> : ToolAsset<TInput, TOutput>
    where TInput : ToolCommand<TOutput>
    where TOutput : class, IViewObject
{
    public ToolCommandAsset()
        : base(false)
    {
        var typeDef = TypeDefinition.FromNative<TInput>();
        string typeName;
        if (!TypeDefinition.IsNullOrEmpty(typeDef))
        {
            typeName = typeDef.Target?.AssetKey ?? typeof(TInput).FullName;
        }
        else
        {
            typeName = typeof(TInput).FullName;
        }

        this.LocalName = $"*PageTool|{typeName.TrimStart('*')}";
        this.Description = typeof(TInput).ToDisplayText();

        ResolveId();
    }

    protected override string GetName() => typeof(TInput).Name;

    public override ImageDef GetIcon() => TypeDefinition.FromNative<TInput>()?.Target?.Icon;

    protected override Task<TOutput> RunTask(TInput input, ToolCallContext context)
    {
        context.ToolInstance.Conversation?.AddSystemMessage("Run tool", msg =>
        {
            msg.AddCode(this.ToDisplayTextL());
        });

        return input.Run(context);
    }

    public override string DisplayText => typeof(TInput).ToDisplayText();
}