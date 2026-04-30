using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using AvaloniaEdit.TextMate;
using Suity.Controls;
using Suity.Editor.Documents;
using Suity.Editor.MenuCommands;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;
using System.IO;
using TextMateSharp.Grammars;

namespace Suity.Editor.Views.TextEditing;

[DocumentViewUsage(typeof(BaseTextDocument))]
[DocumentViewUsage(typeof(Documents.Texts.PromptDocument))]
public partial class AvaTextDocumentView : UserControl, 
    IDocumentView,
    IViewUndo,
    IViewClipboard,
    IViewSelectable
{
    private readonly TextMate.Installation _textMateInstallation;
    private RegistryOptions _registryOptions;
    private int _currentTheme = (int)ThemeName.DarkPlus;

    private FoldingManager _foldingManager;
    private object? _foldingStrategy; // Can be XmlFoldingStrategy, etc.
    private Action? _foldingUpdater;

    private DispatcherTimer _foldingUpdateTimer;

    public AvaTextDocumentView()
    {
        InitializeComponent();

        this.Editor.TextChanged += Editor_TextChanged;

        _registryOptions = new RegistryOptions(
            (ThemeName)_currentTheme);

        _textMateInstallation = Editor.InstallTextMate(_registryOptions);

        var menuCommand = new TextEditorMenu();
        EditorUtility.PrepareMenu(menuCommand);
        Editor.ContextFlyout = AvaMenuFlyoutBinder.CreateMenuFlyout(menuCommand, Editor);

        _foldingManager = FoldingManager.Install(this.Editor.TextArea);

        // Initialize timer: trigger update if no new input within 500ms
        _foldingUpdateTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(500)
        };
        _foldingUpdateTimer.Tick += (s, e) =>
        {
            _foldingUpdateTimer.Stop();
            RunFoldingUpdate();
        };
    }


    public BaseTextDocument? Document { get; private set; }

    #region IDocumentView

    public object? TargetObject => Document;

    public void ActivateView(bool focus)
    {
        //simpleEditor1.curso
    }

    public void GetDataFromDocument()
    {
        if (this.Editor is not { } editor)
        {
            return;
        }

        if (Document is not { } doc)
        {
            editor.TextChanged -= Editor_TextChanged;
            _textMateInstallation.SetGrammar(null);
            editor.Text = string.Empty;
            editor.TextChanged += Editor_TextChanged;

            return;
        }

        string ext = Path.GetExtension(Document.FileName.PhysicFileName).ToLowerInvariant();

        if (ext == ".sprompt")
        {
            ext = ".md";
        }

        Language lang = _registryOptions.GetLanguageByExtension(ext);
        string? scopeName = lang != null ? _registryOptions.GetScopeByLanguageId(lang.Id) : null;

        editor.TextChanged -= Editor_TextChanged;
        _textMateInstallation.SetGrammar(null);
        editor.Text = Document?.TextContent ?? string.Empty;
        _textMateInstallation.SetGrammar(scopeName);
        editor.TextChanged += Editor_TextChanged;

        UpdateFoldingStrategy();
    }
    public void SetDataToDocument()
    {
        if (Document is { } doc)
        {
            doc.TextContent = this.Editor.Text;
            doc.MarkDirty(this);
        }
    }

    public object GetUIObject()
    {
        return this;
    }



    public void StartView(Document document, IDocumentViewHost host)
    {
        Document = (BaseTextDocument)document;
        GetDataFromDocument();
    }

    public void StopView()
    {
        _foldingUpdateTimer?.Stop();
        if (_foldingManager != null)
        {
            FoldingManager.Uninstall(_foldingManager);
            _foldingManager = null;
        }
    }

    public void RefreshView()
    {
    }

    #endregion

    #region IViewUndo

    public bool CanUndo => this.Editor.CanUndo;

    public bool CanRedo => this.Editor.CanRedo;

    public string? UndoText => null;

    public string? RedoText => null;

    public void Undo()
    {
        this.Editor.Undo();
    }

    public void Redo()
    {
        this.Editor.Redo();
    }

    #endregion

    #region IViewClipboard
    public void ClipboardCopy()
    {
        Editor.Copy();
    }

    public void ClipboardCut()
    {
        Editor.Cut();
    }

    public void ClipboardPaste()
    {
        Editor.Paste();
    }
    #endregion

    #region IServiceProvider

    object? IServiceProvider.GetService(Type serviceType)
    {
        if (serviceType is null)
        {
            return null;
        }

        if (serviceType.IsInstanceOfType(this))
        {
            return this;
        }

        if ((Document as IServiceProvider)?.GetService(serviceType) is object obj)
        {
            return obj;
        }

        if (Document != null && serviceType.IsInstanceOfType(Document))
        {
            return Document;
        }

        return null;
    }

    #endregion

    #region IViewSelectable

    public ViewSelection GetSelection()
    {
        var caret = Editor.TextArea.Caret;

        var sel = new TextSearchResult(caret.Line, caret.Column, 0, string.Empty);
        return new ViewSelection(sel);
    }

    public bool SetSelection(ViewSelection selection)
    {
        if (selection?.Selection is TextSearchResult sel)
        {
            try
            {
                SelectTextByLineColumn(sel.line, sel.offset + 1, sel.length);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        return false;
    }

    public void SelectTextByLineColumn(int line, int column, int length)
    {
        // 1. 验证输入：AvaloniaEdit 行列从 1 开始
        if (line < 1 || column < 1) return;

        // 2. 将 行/列 转换为 绝对偏移量 (Offset)
        // Document.GetOffset 会计算指定行、列对应的全局索引
        int startOffset = Editor.Document.GetOffset(line, column);

        // 3. 执行选择
        // 参数 1: 起始位置 (Offset)
        // 参数 2: 选择的长度
        Editor.Select(startOffset, length);

        // 4. (可选) 滚动到视图，并获取焦点使高亮可见
        Editor.TextArea.Caret.BringCaretToView();
        Editor.Focus();
    }

    #endregion


    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        UpdateFoldingStrategy();
    }

    #region Folding

    private void UpdateFoldingStrategy()
    {
        if (_foldingManager != null)
        {
            _foldingManager.Clear();
            FoldingManager.Uninstall(_foldingManager);
        }

        string ext = Path.GetExtension(Document?.FileName.PhysicFileName ?? "").ToLowerInvariant();

        switch (ext)
        {
            case ".xml":
            case ".axaml":
            case ".xaml":
                {
                    _foldingManager = FoldingManager.Install(Editor.TextArea);
                    var strategy = new XmlFoldingStrategy();
                    strategy.UpdateFoldings(_foldingManager, Editor.Document);
                    _foldingStrategy = strategy;
                    _foldingUpdater = () => strategy.UpdateFoldings(_foldingManager, Editor.Document);
                }
                break;

            case ".html":
                {
                    _foldingManager = FoldingManager.Install(Editor.TextArea);
                    var strategy = new HtmlFoldingStrategy();
                    strategy.UpdateFoldings(_foldingManager, Editor.Document);
                    _foldingStrategy = strategy;
                    _foldingUpdater = () => strategy.UpdateFoldings(_foldingManager, Editor.Document);
                }
                break;

            case ".cs":
            case ".java":
            case ".cpp":
            case ".ts":
            case ".js":
            case ".tsx":
            case ".jsx":
                {
                    _foldingManager = FoldingManager.Install(Editor.TextArea);
                    var strategy = new BraceFoldingStrategy();
                    strategy.UpdateFoldings(_foldingManager, Editor.Document);
                    _foldingStrategy = strategy;
                    _foldingUpdater = () => strategy.UpdateFoldings(_foldingManager, Editor.Document);
                }
                break;

            case ".json":
                {
                    _foldingManager = FoldingManager.Install(Editor.TextArea);
                    var strategy = new JsonFoldingStrategy();
                    strategy.UpdateFoldings(_foldingManager, Editor.Document);
                    _foldingStrategy = strategy;
                    _foldingUpdater = () => strategy.UpdateFoldings(_foldingManager, Editor.Document);
                }
                break;

            default:
                _foldingStrategy = null;
                break;
        }
    }

    private void RunFoldingUpdate()
    {
        if (_foldingManager == null || Editor.Document == null) return;

        _foldingUpdater?.Invoke();
    } 

    #endregion

    private void Editor_TextChanged(object? sender, EventArgs e)
    {
        Document?.MarkDirty(this);

        // Stop previous timer and restart
        _foldingUpdateTimer.Stop();
        _foldingUpdateTimer.Start();
    }


}