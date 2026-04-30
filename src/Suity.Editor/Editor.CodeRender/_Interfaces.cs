using Suity.Editor.Expressions;
using Suity.Editor.Types;
using Suity.Editor.WorkSpaces;
using System;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Specifies the storage mode for code libraries.
/// </summary>
public enum CodeLibraryStorageModes
{
    /// <summary>
    /// Database storage.
    /// </summary>
    DB,

    /// <summary>
    /// XML storage.
    /// </summary>
    Xml,

    /// <summary>
    /// Grouping storage.
    /// </summary>
    Grouping,
}

/// <summary>
/// Code library.
/// </summary>
public interface ICodeLibrary : IHasId
{
    /// <summary>
    /// File name where code library is located.
    /// </summary>
    StorageLocation FileName { get; }

    /// <summary>
    /// Storage mode.
    /// </summary>
    CodeLibraryStorageModes StorageMode { get; }

    /// <summary>
    /// Included file names.
    /// </summary>
    IEnumerable<string> IncludedFiles { get; }

    /// <summary>
    /// Whether contains Id dependency.
    /// </summary>
    /// <param name="id">The id to check.</param>
    /// <returns>True if contains the dependency.</returns>
    bool ContainsDependency(Guid id);

    /// <summary>
    /// Creates the build configuration.
    /// </summary>
    /// <returns>The render config.</returns>
    RenderConfig CreateBuildConfig();

    /// <summary>
    /// Default storage object.
    /// </summary>
    object DefaultStorage { get; }
}

/// <summary>
/// Owner of data inputs.
/// </summary>
public interface IDataInputOwner
{
    /// <summary>
    /// Gets all data inputs.
    /// </summary>
    IEnumerable<IDataInput> GetDataInputs();

    /// <summary>
    /// Checks if the owner contains a data input with the specified id.
    /// </summary>
    /// <param name="renderableId">The renderable id.</param>
    /// <returns>True if contains the data input, otherwise false.</returns>
    bool ContainsDataInput(Guid renderableId);
}

/// <summary>
/// List of data inputs with add/remove events.
/// </summary>
public interface IDataInputList : IDataInputOwner
{
    /// <summary>
    /// Occurs when a data input is added.
    /// </summary>
    event Action<IDataInputItem> DataInputAdded;

    /// <summary>
    /// Occurs when a data input is removed.
    /// </summary>
    event Action<IDataInputItem> DataInputRemoved;
}

/// <summary>
/// Data input for rendering.
/// </summary>
public interface IDataInput
{
    /// <summary>
    /// The renderable id.
    /// </summary>
    Guid RenderableId { get; }

    /// <summary>
    /// The material.
    /// </summary>
    IMaterial Material { get; }

    /// <summary>
    /// Creates the build configuration.
    /// </summary>
    /// <returns>The render config.</returns>
    RenderConfig GetBuildConfig();
}

/// <summary>
/// Data input item.
/// </summary>
public interface IDataInputItem : IDataInput
{

}

/// <summary>
/// Code template.
/// </summary>
public interface ICodeTemplate : IHasId
{
    /// <summary>
    /// File name.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Render type.
    /// </summary>
    RenderType RenderType { get; }

    /// <summary>
    /// Render language.
    /// </summary>
    IRenderLanguage Language { get; }

    /// <summary>
    /// Attribute include.
    /// </summary>
    DStruct AttributeInclude { get; }

    /// <summary>
    /// Execute rendering.
    /// </summary>
    /// <param name="context">Expression context.</param>
    /// <param name="target">Render target.</param>
    /// <returns>Returns render result.</returns>
    RenderResult RenderText(ExpressionContext context, RenderTarget target);
}

/// <summary>
/// File bunch for managing multiple files.
/// </summary>
public interface IFileBunch : IHasId
{
    /// <summary>
    /// File name.
    /// </summary>
    string FileName { get; }

    /// <summary>
    /// Last update time.
    /// </summary>
    DateTime LastUpdateTime { get; }

    /// <summary>
    /// Gets all render targets.
    /// </summary>
    /// <param name="basePath">Base path.</param>
    /// <param name="uploadMode">Upload mode.</param>
    /// <returns>Render targets.</returns>
    IEnumerable<RenderTarget> GetRenderTargets(RenderFileName basePath, bool uploadMode);

    /// <summary>
    /// Deletes a file.
    /// </summary>
    /// <param name="fileId">File id.</param>
    /// <returns>True if deleted.</returns>
    bool DeleteFile(string fileId);

    /// <summary>
    /// Commits files to the workspace.
    /// </summary>
    /// <param name="workSpace">The workspace.</param>
    void CommitFiles(WorkSpace workSpace);

    /// <summary>
    /// Files in the bunch.
    /// </summary>
    IEnumerable<IFileBunchElement> Files { get; }

    /// <summary>
    /// Saves files to a directory.
    /// </summary>
    /// <param name="files">Files to save.</param>
    /// <param name="directory">Target directory.</param>
    void SaveToFiles(IEnumerable<IFileBunchElement> files, string directory);

    /// <summary>
    /// Rebuilds the bunch.
    /// </summary>
    /// <returns>Number of changes.</returns>
    long Rebuild();

    /// <summary>
    /// Gets the default code library.
    /// </summary>
    /// <returns>The code library.</returns>
    ICodeLibrary GetCodeLibrary();
}

/// <summary>
/// File bunch element.
/// </summary>
public interface IFileBunchElement
{
    /// <summary>
    /// Element id.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// The file bunch.
    /// </summary>
    IFileBunch FileBunch { get; }

    /// <summary>
    /// File id.
    /// </summary>
    string FileId { get; }

    /// <summary>
    /// Saves the element to a file.
    /// </summary>
    /// <param name="fileName">Target file name.</param>
    void SaveToFile(string fileName);
}

/// <summary>
/// Renderable target.
/// </summary>
public interface IRenderable : IHasId
{
    /// <summary>
    /// Whether rendering is allowed.
    /// </summary>
    bool RenderEnabled { get; }

    /// <summary>
    /// Gets all render items.
    /// </summary>
    /// <returns>Returns all render items.</returns>
    IEnumerable<RenderItem> GetRenderItems();

    /// <summary>
    /// Gets all render targets.
    /// </summary>
    /// <param name="material">Material.</param>
    /// <param name="basePath">Base path, namespace path needs to be set additionally.</param>
    /// <returns>Render targets.</returns>
    IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath);

    /// <summary>
    /// Gets the default code library.
    /// </summary>
    /// <returns>The code library.</returns>
    ICodeLibrary GetCodeLibrary();

    /// <summary>
    /// Default material.
    /// </summary>
    IMaterial DefaultMaterial { get; }
}

/// <summary>
/// Empty renderable implementation.
/// </summary>
public class EmptyRenderable : IRenderable
{
    /// <summary>
    /// Empty instance.
    /// </summary>
    public static readonly EmptyRenderable Empty = new();

    /// <inheritdoc/>
    public bool RenderEnabled => false;

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public IEnumerable<RenderItem> GetRenderItems() => [];

    /// <inheritdoc/>
    public IEnumerable<RenderTarget> GetRenderTargets(IMaterial material, RenderFileName basePath) => [];

    /// <inheritdoc/>
    public ICodeLibrary GetCodeLibrary()
    {
        return null;
    }

    /// <inheritdoc/>
    public IMaterial DefaultMaterial => null;
}

/// <summary>
/// Render host for executing rendering operations.
/// </summary>
public interface IRenderHost
{
    /// <summary>
    /// Executes the render process.
    /// </summary>
    /// <param name="incremental">Whether to use incremental rendering.</param>
    /// <returns>True if successful.</returns>
    bool ExecuteRender(bool incremental);

    /// <summary>
    /// Restores the render.
    /// </summary>
    /// <param name="userCode">User code library.</param>
    /// <returns>True if successful.</returns>
    bool RestoreRender(ICodeLibrary userCode = null);

    /// <summary>
    /// Gets all render targets.
    /// </summary>
    /// <returns>Render targets.</returns>
    IEnumerable<RenderTarget> GetRenderTargets();
}

/// <summary>
/// Code material.
/// </summary>
public interface IMaterial : IHasId
{
    /// <summary>
    /// Local non-UTC time.
    /// </summary>
    DateTime LastUpdateTime { get; }

    /// <summary>
    /// Gets all renderable targets.
    /// </summary>
    /// <param name="item">Render item.</param>
    /// <param name="basePath">Base path.</param>
    /// <returns>Returns all renderable targets.</returns>
    IEnumerable<RenderTarget> GetRenderTargets(RenderItem item, RenderFileName basePath);
}

/// <summary>
/// Empty material implementation.
/// </summary>
public class EmptyMaterial : IMaterial
{
    /// <summary>
    /// Empty instance.
    /// </summary>
    public static readonly EmptyMaterial Empty = new();

    private EmptyMaterial()
    { }

    /// <inheritdoc/>
    public DateTime LastUpdateTime => DateTime.MinValue;

    /// <inheritdoc/>
    public Guid Id => Guid.Empty;

    /// <inheritdoc/>
    public IEnumerable<RenderTarget> GetRenderTargets(RenderItem item, RenderFileName basePath)
    {
        return [];
    }
}

/// <summary>
/// Base class for materials.
/// </summary>
public abstract class Material : Asset, IMaterial
{
    /// <summary>
    /// Creates a new material with an auto-generated name.
    /// </summary>
    public Material()
    {
        _ex.LocalName = $"*{this.GetType().Name}";

        UpdateAssetTypes(typeof(IMaterial));
        ResolveId();
    }

    /// <summary>
    /// Creates a new material with the specified name.
    /// </summary>
    /// <param name="name">The material name.</param>
    public Material(string name)
        : base(name)
    {
        UpdateAssetTypes(typeof(IMaterial));
        ResolveId();
    }

    /// <inheritdoc/>
    public abstract IEnumerable<RenderTarget> GetRenderTargets(RenderItem item, RenderFileName basePath);
}

/// <summary>
/// Render language.
/// </summary>
public interface IRenderLanguage : IHasId
{
    /// <summary>
    /// The language name.
    /// </summary>
    string LanguageName { get; }

    /// <summary>
    /// Segment configuration.
    /// </summary>
    CodeSegmentConfig SegmentConfig { get; }

    /// <summary>
    /// Gets a comment line for the specified text.
    /// </summary>
    /// <param name="str">The text to comment.</param>
    /// <returns>The commented text.</returns>
    string GetCommentLine(string str);
}

/// <summary>
/// Collection of render segments.
/// </summary>
public interface IRenderSegmentCollection
{
    /// <summary>
    /// Gets a segment by item id and extension.
    /// </summary>
    /// <param name="itemId">Item id.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The segment, or null if not found.</returns>
    IRenderSegment GetSegment(Guid itemId, string extension);

    /// <summary>
    /// Gets a segment by key string and extension.
    /// </summary>
    /// <param name="keyString">Key string.</param>
    /// <param name="extension">File extension.</param>
    /// <returns>The segment, or null if not found.</returns>
    IRenderSegment GetSegment(string keyString, string extension);

    /// <summary>
    /// Gets a segment by key.
    /// </summary>
    /// <param name="key">The key.</param>
    /// <returns>The segment, or null if not found.</returns>
    IRenderSegment GetSegment(string key);

    /// <summary>
    /// Gets all segments with the specified extension.
    /// </summary>
    /// <param name="extension">File extension.</param>
    /// <returns>Segments.</returns>
    IEnumerable<IRenderSegment> GetSegments(string extension);

    /// <summary>
    /// All segments in the collection.
    /// </summary>
    IEnumerable<IRenderSegment> Segments { get; }

    /// <summary>
    /// Rebuilds the key index.
    /// </summary>
    void RebuildKeyIndex();

    /// <summary>
    /// Gets the combined code from all segments.
    /// </summary>
    /// <returns>The code.</returns>
    string GetCode();
}

/// <summary>
/// Render segment.
/// </summary>
public interface IRenderSegment
{
    /// <summary>
    /// The segment key.
    /// </summary>
    string Key { get; }

    /// <summary>
    /// Tag type.
    /// </summary>
    string TagType { get; }

    /// <summary>
    /// Item key.
    /// </summary>
    string ItemKey { get; }

    /// <summary>
    /// File extension.
    /// </summary>
    string Extension { get; }

    /// <summary>
    /// Adds code from another segment.
    /// </summary>
    /// <param name="other">Other segment.</param>
    void AddCode(IRenderSegment other);

    /// <summary>
    /// Gets the code content.
    /// </summary>
    /// <returns>The code.</returns>
    string GetCode();

    /// <summary>
    /// Gets the inner code content.
    /// </summary>
    /// <returns>The inner code.</returns>
    string GetInnerCode();
}