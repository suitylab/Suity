using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;

namespace Suity.Editor.Services;

internal class CliImGuiService : IImGuiService
{
    public static CliImGuiService Instance { get; } = new();

    public IUndoableViewObjectImGui CreateColumnTreeImGui(ColumnTreeOptions option) => null;

    public IConversationImGui CreateConversationImGui(string id, bool disableOldMessage = true) => null;

    public IDrawExpandedImGui CreateExpandedView(Type objectType) => null;

    public ImGui CreateImGui(IGraphicContext context, ImGuiConfig config) => null;

    public object CreateImGuiControl(IDrawImGui imGui) => null;

    public Task CreateImGuiDialog(IDrawImGui imGui, DialogOptions option) => Task.CompletedTask;

    public IUndoableViewObjectImGui CreateSimpleTreeImGui(HeaderlessTreeOptions option) => null;

    public bool DrawItem(ImGui gui, object item, EditorImGuiPipeline pipeline, IDrawContext context, bool allDrawers = true)
    {
        return false;
    }

    public ImGuiTheme GetEditorTheme(bool preview)
    {
        if (preview)
        {
            return PropertyGridTheme.Preview;
        }
        else
        {
            return PropertyGridTheme.Default;
        }
    }
}
