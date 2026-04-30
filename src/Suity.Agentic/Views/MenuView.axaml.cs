using Avalonia.Controls;
using Suity.Controls;
using Suity.Editor.MenuCommands.AppMenus;

namespace Suity.Editor.Views;

public partial class MenuView : UserControl
{
    public MenuView()
    {
        InitializeComponent();

        BuildAndSetMenu();
    }


    private void BuildAndSetMenu()
    {
        var menu = new RootMainMenu();
        EditorUtility.PrepareMenu(menu);

        // 2. Get physical Menu control via Binder and display
        // Pass empty selection, because main menu is usually displayed globally
        var menuControl = AvaMainMenuBinder.Default.EnsureMainMenu(menu);

        // 3. Set the built Menu as the content of the current UserControl
        this.Content = menuControl;
    }
}
