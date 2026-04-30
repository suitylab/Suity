using Suity.Helpers;
using System.IO;
using System.Text;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Code render result.
/// </summary>
public abstract class RenderResult
{
    /// <summary>
    /// Empty render result.
    /// </summary>
    public static readonly RenderResult Empty = new EmptyRenderResult(RenderStatus.None);

    /// <summary>
    /// Empty successful render result.
    /// </summary>
    public static readonly RenderResult EmptySuccess = new EmptyRenderResult(RenderStatus.Success);

    /// <summary>
    /// Error continue render result.
    /// </summary>
    public static readonly RenderResult ErrorContinue = new EmptyRenderResult(RenderStatus.ErrorContinue);

    /// <summary>
    /// Error interrupt render result.
    /// </summary>
    public static readonly RenderResult ErrorInterrupt = new EmptyRenderResult(RenderStatus.ErrorInterrupt);

    private readonly RenderStatus _status;

    /// <summary>
    /// Creates a new render result.
    /// </summary>
    /// <param name="status">Render status.</param>
    public RenderResult(RenderStatus status)
    {
        _status = status;
    }

    /// <summary>
    /// Whether the result is binary data.
    /// </summary>
    public abstract bool IsBinary { get; }

    /// <summary>
    /// Gets the text content.
    /// </summary>
    /// <returns>The text.</returns>
    public abstract string GetText();

    /// <summary>
    /// Gets the stream content.
    /// </summary>
    /// <returns>The stream.</returns>
    public abstract Stream GetStream();

    /// <summary>
    /// Render status.
    /// </summary>
    public RenderStatus Status => _status;

    /// <summary>
    /// Old file name.
    /// </summary>
    public string OldFileName { get; set; }

    /// <summary>
    /// Writes the result to a file.
    /// </summary>
    /// <param name="fileName">File name.</param>
    /// <returns>Render status.</returns>
    public virtual RenderStatus WriteTo(string fileName)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        if (IsBinary)
        {
            using var fs = new FileStream(fileName, FileMode.Create);
            GetStream().CopyTo(fs);
        }
        else
        {
            TextFileHelper.WriteFile(fileName, GetText());
        }

        return RenderStatus.Success;
    }
}

/// <summary>
/// Empty render result implementation.
/// </summary>
internal sealed class EmptyRenderResult : RenderResult
{
    /// <summary>
    /// Creates an empty render result.
    /// </summary>
    /// <param name="status">Render status.</param>
    internal EmptyRenderResult(RenderStatus status)
        : base(status)
    {
    }

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override string GetText() => string.Empty;

    /// <inheritdoc/>
    public override Stream GetStream()
    {
        return new MemoryStream(0);
    }

    /// <inheritdoc/>
    public override RenderStatus WriteTo(string fileName)
    {
        return RenderStatus.None;
    }
}

/// <summary>
/// Render result that reads from a file.
/// </summary>
public sealed class TargetFileRenderResult : RenderResult
{
    /// <summary>
    /// The render target.
    /// </summary>
    public RenderTarget Target { get; }

    /// <summary>
    /// Creates a target file render result.
    /// </summary>
    /// <param name="target">Render target.</param>
    /// <param name="status">Render status.</param>
    public TargetFileRenderResult(RenderTarget target, RenderStatus status)
        : base(status)
    {
        Target = target;
    }

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override string GetText()
    {
        return TextFileHelper.TryReadAllText(Target.FileName.PhysicFullPath, Encoding.UTF8);
    }

    /// <inheritdoc/>
    public override Stream GetStream()
    {
        if (File.Exists(Target.FileName.PhysicFullPath))
        {
            return File.Open(Target.FileName.PhysicFullPath, FileMode.Open);
        }
        else
        {
            return new MemoryStream(0);
        }
    }
}

/// <summary>
/// Text render result.
/// </summary>
public class TextRenderResult : RenderResult
{
    private readonly string _text;

    /// <summary>
    /// Creates a text render result.
    /// </summary>
    /// <param name="result">Render status.</param>
    /// <param name="text">Text content.</param>
    public TextRenderResult(RenderStatus result, string text)
        : base(result)
    {
        _text = text;
    }

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <inheritdoc/>
    public override string GetText() => _text;

    /// <inheritdoc/>
    public override Stream GetStream()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(_text));
    }
}