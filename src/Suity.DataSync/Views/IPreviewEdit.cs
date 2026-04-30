using Suity.Synchonizing;

namespace Suity.Views;

public interface IPreviewEdit : IPreviewDisplay
{
    bool CanEditPreviewText { get; }

    void SetPreviewText(string text, ISyncContext setup);
}