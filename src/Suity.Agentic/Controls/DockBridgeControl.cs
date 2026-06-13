using Avalonia.Controls;

namespace Suity.Editor.Controls;

public class DockBridgeControl<T> : UserControl
    where T : Control
{
    public T Target { get; }

    public DockBridgeControl(T target)
    {
        Target = target ?? throw new System.ArgumentNullException(nameof(target));

        this.Content = target;
    }
}
