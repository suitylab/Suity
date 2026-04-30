namespace Suity.Synchonizing.Preset;

/// <summary>
/// Extension methods for ISyncObject property operations
/// </summary>
public static class SinglePropertySyncExtensions
{
    public static object GetProperty(this ISyncObject obj, string name, ISyncContext context = null)
    {
        SinglePropertySync sync = SinglePropertySync.CreateGetter(name);
        obj.Sync(sync, context ?? SyncContext.Empty);

        return sync.Value;
    }

    public static T GetProperty<T>(this ISyncObject obj, string name, ISyncContext context = null)
    {
        SinglePropertySync sync = SinglePropertySync.CreateGetter(name);
        obj.Sync(sync, context ?? SyncContext.Empty);

        return (T)sync.Value;
    }

    public static SinglePropertySync SetProperty(this ISyncObject obj, string name, object value, ISyncContext context = null)
    {
        SinglePropertySync sync = SinglePropertySync.CreateSetter(name, value);
        obj.Sync(sync, context ?? SyncContext.Empty);

        return sync;
    }

    public static SinglePropertySync SetProperty<T>(this ISyncObject obj, string name, T value, ISyncContext context = null)
    {
        SinglePropertySync sync = SinglePropertySync.CreateSetter(name, value);
        obj.Sync(sync, context ?? SyncContext.Empty);

        return sync;
    }
}