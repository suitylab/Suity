using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using System.IO;
using System.Text;

namespace Suity.Editor.ViewModels;

public class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel()
    {
    }

    private Encoding GetEncoding(string path)
    {
        using var reader = new StreamReader(path, Encoding.Default, true);
        if (reader.Peek() >= 0)
        {
            reader.Read();
        }
        return reader.CurrentEncoding;
    }


    public void CloseLayout()
    {
    }

    public void FileNew()
    {
    }

    public async void FileOpen()
    {
    }

    public async void FileSave()
    {
    }

    public async void FileSaveAs()
    {
    }

    public void FileExit()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
    }

    public void EditFind()
    {
    }

    public void EditReplace()
    {
    }

    public void EditWrapLines()
    {
    }

    public void EditFindNext()
    {
    }

    public void EditReplaceNext()
    {
    }

    public void EditUndo()
    {
    }

    public async void EditCut()
    {
    }

    public async void EditCopy()
    {
    }

    public async void EditPaste()
    {
    }

    public void EditDelete()
    {
    }

    public void EditSelectAll()
    {
    }

    public void EditTimeDate()
    {
    }

    public void FormatFont()
    {
    }

    public async void HelpGetHelp()
    {
    }

    public async void HelpAbout()
    {
    }

    public void ViewStatusBar()
    {
    }
}
