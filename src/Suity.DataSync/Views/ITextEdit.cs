using Suity.Synchonizing;

namespace Suity.Views;

public interface ITextEdit : ITextDisplay
{
    bool CanEditText { get; }

    void SetTextEdit(string text, ISyncContext setup);
}