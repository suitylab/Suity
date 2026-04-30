using Suity.Synchonizing;

namespace Suity.Views;

public class TextBlock : ISyncObject
{
    private string _text;

    public string Text
    {
        get => _text;
        set => _text = value;
    }

    public TextBlock()
    { }

    public TextBlock(string text)
    {
        _text = text;
    }

    public virtual string Format => null;

    public virtual void Sync(IPropertySync sync, ISyncContext context)
    {
        Text = sync.Sync(nameof(Text), Text);
    }

    public override string ToString()
    {
        return _text ?? string.Empty;
    }
}