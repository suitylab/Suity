using Suity.Synchonizing;

namespace Suity.Views;

public interface ITextEdit : ITextDisplay
{
    bool CanEditText { get; }

    void SetText(string text, ISyncContext setup);
}