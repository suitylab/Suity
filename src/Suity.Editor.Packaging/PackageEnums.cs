namespace Suity.Editor.Packaging;

/// <summary>
/// Specifies the direction of a package operation.
/// </summary>
public enum PackageDirection
{
    /// <summary>
    /// Exporting files from the project into a package.
    /// </summary>
    Export,

    /// <summary>
    /// Importing files from a package into the project.
    /// </summary>
    Import,
}

/// <summary>
/// Specifies the type of package being created or imported.
/// </summary>
public enum PackageTypes
{
    /// <summary>
    /// A standard Suity package containing assets and workspace files.
    /// </summary>
    SuityPackage,

    /// <summary>
    /// A Suity library package with restricted content and manifest.
    /// </summary>
    SuityLibrary,
}

/// <summary>
/// Specifies the location category of a file within the project.
/// </summary>
public enum FileLocations
{
    /// <summary>
    /// File resides in the project asset directory.
    /// </summary>
    Asset,

    /// <summary>
    /// File resides in a workspace directory.
    /// </summary>
    WorkSpace,
}
