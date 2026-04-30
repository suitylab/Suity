namespace Suity.Editor;

public class GuiStatePlugin : BackendPlugin
{
    public override string Description => "GUI";


    public void SetGuiState<T>(Asset asset, T config) where T : class
    {
        SetAssetState(asset, config);
    }

    public T GetGuiState<T>(Asset asset) where T : class
    {
        return GetAssetState(asset) as T;
    }
}
