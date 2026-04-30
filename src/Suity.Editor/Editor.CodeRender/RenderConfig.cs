namespace Suity.Editor.CodeRender;

/// <summary>
/// Represents a user code rename operation.
/// </summary>
public class UserCodeRename
{
    /// <summary>
    /// Old key string.
    /// </summary>
    public string OldKeyString { get; set; }

    /// <summary>
    /// New key string.
    /// </summary>
    public string NewKeyString { get; set; }

    /// <summary>
    /// Creates a new user code rename.
    /// </summary>
    public UserCodeRename()
    { }

    /// <inheritdoc/>
    public override string ToString() => $"{OldKeyString} -> {NewKeyString}";
}

/// <summary>
/// Configuration for code rendering.
/// </summary>
public class RenderConfig
{
    /// <summary>
    /// Creates a new render config.
    /// </summary>
    public RenderConfig()
    { }

    /// <summary>
    /// Workspace.
    /// </summary>
    public string WorkSpace { get; set; }

    /// <summary>
    /// Base path.
    /// </summary>
    public string BasePath { get; set; }

    /// <summary>
    /// Condition.
    /// </summary>
    public ICondition Condition { get; set; }

    /// <summary>
    /// User code library.
    /// </summary>
    public ICodeLibrary UserCode { get; set; }

    /// <summary>
    /// Naming options.
    /// </summary>
    public SystemNamingOption Naming { get; set; }

    /// <summary>
    /// Whether rendering is disabled.
    /// </summary>
    public bool Disabled { get; set; }
}