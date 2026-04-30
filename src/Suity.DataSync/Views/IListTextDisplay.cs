namespace Suity.Views;

public interface IListTextDisplay
{
    string GetText(int index);

    object GetIcon(int index);

    TextStatus GetTextStatus(int index);
}