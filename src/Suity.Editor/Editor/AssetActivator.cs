namespace Suity.Editor;

public enum LoadingIterations
{
    [DisplayText("System")]
    System,

    [DisplayText("Work space")]
    WorkSpace,


    [DisplayText("Iteration 1")]
    Iteration1,

    [DisplayText("Iteration 2")]
    Iteration2,

    [DisplayText("Iteration 3")]
    Iteration3,

    [DisplayText("Iteration 4")]
    Iteration4,
}

/// <summary>
/// Asset activator
/// </summary>
public abstract class AssetActivator
{
    internal bool _initialized;

    public AssetInstanceMode InstanceMode { get; internal set; }

    public abstract Asset CreateAsset(string fileName, string assetKey);

    public abstract string[] GetExtensions();

    public virtual bool RequireInFileResolve => false;

    public virtual LoadingIterations Iteration => LoadingIterations.Iteration1;

    /// <summary>
    /// Gets the resource ID to see if it has been recorded in the file. If it has been recorded, there is no need to record the ID of this resource in the global ID table.
    /// </summary>
    public virtual bool IsIdDocumented => false;

    public virtual bool IsAttached => false;
}