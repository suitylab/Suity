using Suity.Synchonizing;

namespace Suity.Views;

public interface ITextEdit
{
    bool CanEditText { get; }


    string GetTextEdit();

    void SetTextEdit(string text, ISyncContext setup);
}