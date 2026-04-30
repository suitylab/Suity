using Avalonia.Input;
using Suity.Collections;
using Suity.Editor.Conversation;
using Suity.Editor.Gui.TreeGui;
using Suity.Editor.Views;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

internal class AvaImGuiService : IImGuiService
{
    public static readonly AvaImGuiService Instance = new();

    readonly List<IDrawItemImGui> _itemImGuis = [];
    readonly UniqueMultiDictionary<Type, IDrawItemImGui> _itemImGuiDict = new();
    readonly Dictionary<Type, IDrawItemImGui[]> _cachedItemImGuiDict = [];

    public ImGui CreateImGui(IGraphicContext context, ImGuiConfig config) => ImGuiServices.CreateImGui(context, config);

    public IConversationImGui CreateConversationImGui(string id, bool disableOldMessage) => new ConversationHandlerImGui(id) { DisableOldMessage = disableOldMessage };

    public IDrawExpandedImGui CreateExpandedView(Type objectType)
        => DrawExpandedImGuiResolver.Instance.CreateView(objectType);

    public object CreateImGuiControl(IDrawImGui imGui) => null;

    public Task CreateImGuiDialog(IDrawImGui imGui, DialogOptions option)
    {
        var mainWindow = SuityApp.Instance.Window;
        if (mainWindow is null)
        {
            return Task.CompletedTask;
        }

        var imGuiWindow = new AvaImguiWindow(imGui);
        imGuiWindow.Width = option.Width;
        imGuiWindow.Height = option.Height;
        imGuiWindow.Title = option.Title ?? string.Empty;
        imGuiWindow.CanResize = !option.FixedSize;
        if (option.Icon is { } icon)
        {
            imGuiWindow.Icon = new Avalonia.Controls.WindowIcon(icon.ToAvaloniaBitmapCached());
        }

        if (option.IsDialog)
        {
            imGuiWindow.KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    imGuiWindow.Close();
                }
            };

            return imGuiWindow.ShowDialog(mainWindow);
        }
        else
        {
            imGuiWindow.Show(mainWindow);
            return Task.CompletedTask;
        }
    }

    public IUndoableViewObjectImGui CreateSimpleTreeImGui(HeaderlessTreeOptions option)
    {
        return new UndoableTreeImGui(option);
    }

    public IUndoableViewObjectImGui CreateColumnTreeImGui(ColumnTreeOptions option)
    {
        return new UndoableTreeImGui(option);
    }

    public bool DrawItem(ImGui gui, object item, EditorImGuiPipeline pipeline, IDrawContext context, bool allDrawers = true)
    {
        if (gui is null)
        {
            throw new ArgumentNullException(nameof(gui));
        }

        if (item is null)
        {
            return false;
        }

        var itemGuis = GetOrCreateCachedItemImGuis(item.GetType());
        if (itemGuis.Length == 0)
        {
            return false;
        }

        if (allDrawers)
        {
            foreach (var itemGui in itemGuis)
            {
                try
                {
                    itemGui.OnEditorGui(gui, item, pipeline, context);
                }
                catch (Exception err)
                {
                    err.LogError();
                }
            }

            return true;
        }
        else
        {
            return itemGuis[0].OnEditorGui(gui, item, pipeline, context);
        }
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


    private IDrawItemImGui[] GetOrCreateCachedItemImGuis(Type type)
    {
        if (_cachedItemImGuiDict.TryGetValue(type, out var imGuis))
        {
            return imGuis;
        }

        List<IDrawItemImGui> imGuiList = [];

        Type aType = type;
        while (aType != null)
        {
            imGuiList.AddRange(_itemImGuiDict[type]);
            aType = aType.BaseType;
        }

        var interfaceTypes = type.GetInterfaces();
        if (interfaceTypes != null)
        {
            foreach (var interfaceType in interfaceTypes)
            {
                imGuiList.AddRange(_itemImGuiDict[interfaceType]);
            }
        }

        imGuis = [.. imGuiList];
        _cachedItemImGuiDict[type] = imGuis;

        return imGuis;
    }
}
