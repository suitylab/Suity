using Suity.Drawing;
using Suity.Editor.Services;
using Suity.Views;
using Suity.Views.Graphics;
using Suity.Views.Im;
using Suity.Views.Menu;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace Suity.Editor.Conversation;

#region DialogMenuRootCommand
/// <summary>
/// Root menu command for conversation dialog context menu.
/// </summary>
internal class DialogMenuRootCommand : RootMenuCommand
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DialogMenuRootCommand"/> class with a default "Copy" command.
    /// </summary>
    public DialogMenuRootCommand()
        : base(":Conversation")
    {
        AddCommand("Copy", cmd =>
        {
            string text = (this.Sender as ImGuiNode)?.Text;
            if (text != null)
            {
                EditorUtility.SetSystemClipboardText(text);
            }
        });
    }
}
#endregion

#region DialogItem
/// <summary>
/// Represents a single dialog item (message) in a conversation, supporting text, code, buttons, progress bars, and other elements.
/// </summary>
public class DialogItem : IDialogMessage
{
    private static readonly Color ColorUser = EditorColorScheme.Default.EditorBG;  //ColorTranslator.FromHtml("#3F8ABF");
    private static readonly Color ColorRemote = EditorColorScheme.Default.EditorBG;
    private static readonly Color ColorSystem = EditorColorScheme.Default.EditorBG;
    private static readonly Color ColorDebug = ColorTranslators.FromHtml("#963FBF");
    private static readonly Dictionary<Color, ConsoleColor> _consoleColorCache = [];

    private List<DialogElement> _elements;

    /// <inheritdoc/>
    public TextStatus Status { get; set; } = TextStatus.Normal;
    /// <inheritdoc/>
    public ConversationRole Role { get; set; }
    /// <inheritdoc/>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the width percentage for user messages.
    /// </summary>
    public int WidthPercentage { get; set; } = 80;
    /// <inheritdoc/>
    public string Id { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogItem"/> class.
    /// </summary>
    public DialogItem()
    {
    }

    /// <inheritdoc/>
    public void AddText(string text)
    {
        var elements = _elements ??= [];
        elements.Add(new DialogElement_Text(text));
    }

    /// <inheritdoc/>
    public void AddCode(string code)
    {
        var elements = _elements ??= [];
        elements.Add(new DialogElement_Code(code));
    }

    /// <inheritdoc/>
    public void AddButton(string key, string text, Action callBack)
    {
        var elements = _elements ??= [];
        elements.Add(new DialogElement_Button(key, text, callBack));
    }

    /// <inheritdoc/>
    public void AddButtons(string title, IEnumerable<ConversationButton> buttons)
    {
        if (buttons is null)
        {
            throw new ArgumentNullException(nameof(buttons));
        }

        var elements = _elements ??= [];
        elements.Add(new DialogElement_ButtonGroup(title ?? string.Empty, buttons));
    }

    /// <inheritdoc/>
    public void AddProgressBar(float progress, float max)
    {
        var elements = _elements ??= [];
        elements.Add(new DialogElement_ProgressBar(progress, max));
    }

    /// <inheritdoc/>
    public void AddLine()
    {
        var elements = _elements ??= [];
        elements.Add(new DialogElement_Line());
    }

    /// <summary>
    /// Renders this dialog item in the ImGui interface.
    /// </summary>
    /// <param name="gui">The ImGui rendering context.</param>
    /// <param name="index">The index of this item in the conversation.</param>
    /// <param name="enabled">Whether the item is enabled for interaction.</param>
    /// <param name="menu">The root menu command for context menu.</param>
    /// <param name="host">The conversation host.</param>
    public void OnGui(ImGui gui, int index, bool enabled, RootMenuCommand menu, IConversationHost host)
    {
        var role = Role;

        ImGuiNode layoutNode;
        if (role == ConversationRole.User)
        {
            layoutNode = gui.HorizontalReverseLayout($"dialog#_{index}_{Id}")
                .InitFullWidth();
        }
        else
        {
            layoutNode = gui.HorizontalLayout($"dialog#_{index}_{Id}")
                .InitFullWidth();
        }

        layoutNode.OnContent(() =>
        {
            var color = role switch
            {
                ConversationRole.System => ColorSystem,
                ConversationRole.Remote => ColorRemote,
                ConversationRole.User => ColorUser,
                _ => ColorDebug,
            };

            if (Status != TextStatus.Normal)
            {
                color = EditorServices.ColorConfig.GetStatusColor(Status);
            }

            ImGuiNode frameNode;

            frameNode = gui.Frame()
            .SetColor(color);

            if (role == ConversationRole.User)
            {
                frameNode.InitWidthPercentage(WidthPercentage);
            }
            else
            {
                frameNode.InitFullWidth();
            }

            frameNode.OnContent(() =>
            {
                bool hasMessage = false;

                if (!string.IsNullOrEmpty(Message))
                {
                    gui.TextArea("text", Message)
                    .InitFullWidth()
                    .SetEnabled(enabled)
                    .InitInputMouseUp((n, btn) =>
                    {
                        if (btn == GuiMouseButtons.Right)
                        {
                            menu.ApplySender(n);
                            (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu);
                        }

                        return GuiInputState.None;
                    });

                    hasMessage = true;
                }

                if (_elements != null)
                {
                    for (int i = 0; i < _elements.Count; i++)
                    {
                        if (hasMessage)
                        {
                            gui.HorizontalLayout($"#empty_frame_{i}")
                            .InitFullWidth()
                            .InitHeight(5);
                        }
                        else
                        {
                            hasMessage = true;
                        }

                        _elements[i].OnGui(gui, i, menu, host)
                        .SetEnabled(enabled);
                    }
                }
            });
        });
    }

    public void WriteConsole()
    {
        var color = EditorServices.ColorConfig.GetStatusColor(Status);

        ConsoleColor prevColor = default;

        if (EditorServices.PlatformService.IsConsoleColorSupported)
        {
            prevColor = Console.ForegroundColor;
            Console.ForegroundColor = ToConsoleColor(color);
        }

        Console.WriteLine(Message);

        if (_elements != null)
        {
            for (int i = 0; i < _elements.Count; i++)
            {
                Console.Write("  - ");
                _elements[i].WriteConsole();
            }
        }

        if (EditorServices.PlatformService.IsConsoleColorSupported)
        {
            Console.ForegroundColor = prevColor;
        }
    }

    public void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("DialogItem");
        writer.Node("Content").WriteString(Message);
        writer.Node("Status").WriteString(Status.ToString());

        if (_elements != null)
        {
            foreach (var element in _elements)
            {
                element.WriteData(writer);
            }
        }
    }

    private static ConsoleColor ToConsoleColor(Color color)
    {
        if (_consoleColorCache.TryGetValue(color, out var cached))
            return cached;

        int r = color.R, g = color.G, b = color.B;
        ConsoleColor result;
        if (r < 64 && g < 64 && b < 64) result = ConsoleColor.Black;
        else if (r > 192 && g > 192 && b > 192) result = ConsoleColor.White;
        else if (r > 192 && g < 64 && b < 64) result = ConsoleColor.Red;
        else if (r < 64 && g > 192 && b < 64) result = ConsoleColor.Green;
        else if (r < 64 && g < 64 && b > 192) result = ConsoleColor.Blue;
        else if (r > 192 && g > 192 && b < 64) result = ConsoleColor.Yellow;
        else if (r < 64 && g > 192 && b > 192) result = ConsoleColor.Cyan;
        else if (r > 192 && g < 64 && b > 192) result = ConsoleColor.Magenta;
        else if (r > 128 && g < 64) result = ConsoleColor.DarkRed;
        else if (g > 128 && r < 64) result = ConsoleColor.DarkGreen;
        else if (b > 128 && r < 64) result = ConsoleColor.DarkBlue;
        else if (r > 128 && g > 64 && b < 64) result = ConsoleColor.DarkYellow;
        else if (r < 64 && g > 128 && b > 128) result = ConsoleColor.DarkCyan;
        else if (r > 128 && g < 64 && b > 128) result = ConsoleColor.DarkMagenta;
        else if (r > 128 && g > 128 && b > 128) result = ConsoleColor.Gray;
        else result = ConsoleColor.White;

        _consoleColorCache[color] = result;
        return result;
    }
}
#endregion

#region DialogElement
/// <summary>
/// Base class for interactive elements within a conversation dialog item.
/// </summary>
public abstract class DialogElement
{
    /// <summary>
    /// Renders this element in the ImGui interface.
    /// </summary>
    /// <param name="gui">The ImGui rendering context.</param>
    /// <param name="index">The index of this element.</param>
    /// <param name="menu">The root menu command for context menu.</param>
    /// <param name="host">The conversation host.</param>
    /// <returns>The rendered ImGui node.</returns>
    public abstract ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host);

    public abstract void WriteConsole();

    public abstract void WriteData(IDataWriter writer);
}
#endregion

#region DialogElement_Text
/// <summary>
/// A text element displayed in a conversation dialog.
/// </summary>
/// <param name="text">The text content.</param>
public class DialogElement_Text(string text) : DialogElement
{
    /// <summary>
    /// Gets the text content.
    /// </summary>
    public string Text { get; } = text ?? string.Empty;

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        return gui.TextArea($"msg#{index}", Text)
        .InitFullWidth()
        .InitInputMouseUp((n, btn) =>
        {
            if (btn == GuiMouseButtons.Right)
            {
                menu.ApplySender(n);
                (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu);
            }

            return GuiInputState.None;
        });
    }

    public override void WriteConsole()
    {
        Console.WriteLine(Text ?? string.Empty);
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("Text");
        writer.Node("Text").WriteString(Text);
    }
}
#endregion

#region DialogElement_Code
/// <summary>
/// A code block element displayed in a conversation dialog.
/// </summary>
/// <param name="text">The code content.</param>
public class DialogElement_Code(string text) : DialogElement
{
    /// <summary>
    /// Gets the code content.
    /// </summary>
    public string Code { get; } = text ?? string.Empty;

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        return gui.TextAreaInput($"msg#{index}", Code, Code)
        .InitFullWidth()
        .InitReadonly(true);

        //.InitInputMouseUp((n, btn) =>
        //{
        //    if (btn == GuiMouseButtons.Right)
        //    {
        //        menu.ApplySender(n);
        //        (gui.Context as IGraphicContextMenu)?.ShowContextMenu(menu);
        //    }

        //    return GuiState.None;
        //});
    }

    public override void WriteConsole()
    {
        Console.WriteLine("```");
        Console.WriteLine(Code ?? string.Empty);
        Console.WriteLine("```");
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("Code");
        writer.Node("Code").WriteString(Code);
    }
}
#endregion

#region DialogElement_Button
/// <summary>
/// A button element displayed in a conversation dialog.
/// </summary>
public class DialogElement_Button : DialogElement
{
    /// <summary>
    /// Gets the button key used for identification.
    /// </summary>
    public string Key { get; }
    /// <summary>
    /// Gets the display text of the button.
    /// </summary>
    public string Text { get; }
    /// <summary>
    /// Gets the callback action invoked when the button is clicked.
    /// </summary>
    public Action Clicked { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogElement_Button"/> class.
    /// </summary>
    /// <param name="key">The button key.</param>
    /// <param name="text">The display text.</param>
    /// <param name="clicked">Optional callback action when clicked.</param>
    public DialogElement_Button(string key, string text, Action clicked = null)
    {
        Key = key;
        Text = text ?? string.Empty;
        Clicked = clicked;
    }

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        return gui.Button($"btn#{Key}_{index}", Text)
        .InitClass("simpleBtn")
        .InitHorizontalAlignment(GuiAlignment.Far)
        .OnClick(() =>
        {
            if (Clicked != null)
            {
                Clicked();
            }
            else if (!string.IsNullOrWhiteSpace(Key))
            {
                host?.HandleButtonClick(Key);
            }
        });
    }

    public override void WriteConsole()
    {
        Console.WriteLine($"Button [/{Key}: {Text}]");
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("Button");
        writer.Node("Key").WriteString(Key);
        writer.Node("Text").WriteString(Text);
    }
}
#endregion

#region DialogElement_ButtonGroup
/// <summary>
/// A group of buttons displayed in a conversation dialog.
/// </summary>
public class DialogElement_ButtonGroup : DialogElement
{
    private readonly string _title;
    private readonly ConversationButton[] _buttons;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogElement_ButtonGroup"/> class.
    /// </summary>
    /// <param name="title">The title displayed alongside the button group.</param>
    /// <param name="buttons">The collection of buttons.</param>
    public DialogElement_ButtonGroup(string title, IEnumerable<ConversationButton> buttons)
    {
        _title = title ?? string.Empty;
        _buttons = [.. buttons];
    }

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        return gui.VerticalLayout($"btn_group#{index}")
        .InitFullWidth()
        .OnContent(() =>
        {
            gui.HorizontalLayout("horizontal")
            .InitFit(GuiOrientation.Both)
            .InitHorizontalAlignment(GuiAlignment.Far)
            .OnContent(() =>
            {
                if (!string.IsNullOrWhiteSpace(_title))
                {
                    gui.Text("#title", _title)
                    .InitVerticalAlignment(GuiAlignment.Center);
                }

                for (int i = 0; i < _buttons.Length; i++)
                {
                    var button = _buttons[i];
                    gui.Button($"btn#{i}_{button.Key}", button.Text)
                    .InitClass("simpleBtn")
                    .OnClick(() =>
                    {
                        if (button.CallBack != null)
                        {
                            button.CallBack.Invoke();
                        }
                        else if (!string.IsNullOrWhiteSpace(button.Key))
                        {
                            host?.HandleButtonClick(button.Key);
                        }
                    });
                }
            });
        });
    }

    public override void WriteConsole()
    {
        var btnTexts = string.Join(", ", Array.ConvertAll(_buttons, b => $"[/{b.Key}: {b.Text}]"));
        Console.WriteLine($"Buttons {_title}{btnTexts}");
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("ButtonGroup");
        writer.Node("Title").WriteString(_title);

        var aryWriter = writer.Nodes("Buttons", _buttons.Length);
        foreach (var button in _buttons)
        {
            var btnWriter = aryWriter.Item();
            btnWriter.Node("Key").WriteString(button.Key);
            btnWriter.Node("Text").WriteString(button.Text);
        }
        aryWriter.Finish();
    }
}
#endregion

#region DialogElement_Line
/// <summary>
/// A horizontal line separator element displayed in a conversation dialog.
/// </summary>
public class DialogElement_Line : DialogElement
{
    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        return gui.HorizontalLine($"line#{index}").InitFullWidth();
    }

    public override void WriteConsole()
    {
        Console.WriteLine("──────────");
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("Line");
    }
}
#endregion

#region DialogElement_ProgressBar
/// <summary>
/// A progress bar element displayed in a conversation dialog.
/// </summary>
public class DialogElement_ProgressBar : DialogElement
{
    private readonly float _progress;
    private readonly float _max;

    /// <summary>
    /// Initializes a new instance of the <see cref="DialogElement_ProgressBar"/> class.
    /// </summary>
    /// <param name="progress">The current progress value.</param>
    /// <param name="max">The maximum progress value.</param>
    public DialogElement_ProgressBar(float progress, float max)
    {
        _progress = progress;
        _max = max;

        if (_max <= 0)
        {
            _max = 0;
        }
        if (_progress < 0)
        {
            _progress = 0;
        }
        if (_progress > _max)
        {
            _progress = _max;
        }
    }

    /// <inheritdoc/>
    public override ImGuiNode OnGui(ImGui gui, int index, RootMenuCommand menu, IConversationHost host)
    {
        var node = gui.ProgressBar($"progress#{index}", _progress, _max).InitFullWidth();
        node.Text = $"{_progress} / {_max}";

        return node;
    }

    public override void WriteConsole()
    {
        Console.WriteLine($"Progress: {_progress} / {_max}");
    }

    public override void WriteData(IDataWriter writer)
    {
        writer.Node("@type").WriteString("ProgressBar");
        writer.Node("Progress").WriteSingle(_progress);
        writer.Node("Max").WriteSingle(_max);
    }
} 
#endregion