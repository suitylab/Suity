namespace Suity.Editor.DataModel;

public class DataModelPlugin : BackendPlugin
{
    public static DataModelPlugin Instance { get; private set; }

    public DataModelPlugin()
    {
        Instance ??= this;
    }
}
