using Suity.Editor.Services;
using Suity.Helpers;
using System;
using System.IO;
using System.Text;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Render result that handles binary data.
/// </summary>
public class BinaryRenderResult : RenderResult
{
    private readonly byte[] _data;
    private readonly bool _binary;

    /// <summary>
    /// Creates a binary render result.
    /// </summary>
    /// <param name="result">The render status.</param>
    /// <param name="data">The binary data.</param>
    public BinaryRenderResult(RenderStatus result, byte[] data)
        : base(result)
    {
        _data = data;
        _binary = !TextFileHelper.IsText(out _, GetStream(), 100);
    }

    /// <inheritdoc/>
    public override bool IsBinary => _binary;

    /// <inheritdoc/>
    public override string GetText()
    {
        if (IsBinary)
        {
            return null;
        }
        else
        {
            // Do not use Encoding.UTF8.GetString because it won't recognize BOM header, treating header as characters.
            //return Encoding.UTF8.GetString(_data.Array, _data.Offset, _data.Count);

            // UTF8 encoding required
            //using (MemoryStream stream = new MemoryStream(_data.Array, _data.Offset, _data.Count, false))
            //{
            //    StreamReader sr = new StreamReader(stream, Encoding.UTF8);
            //    string data = sr.ReadToEnd();
            //    sr.Close();
            //    return data;
            //}

            // Support multiple encodings
            return TextFileHelper.ReadFile(_data);
        }
    }

    /// <inheritdoc/>
    public override Stream GetStream()
    {
        return new MemoryStream(_data, false);
    }

    /// <inheritdoc/>
    public override RenderStatus WriteTo(string fileName)
    {
        if (File.Exists(fileName))
        {
            uint myCrc = EditorServices.EditorSystem.ComputeCrc32(_data);

            byte[] currentBytes = File.ReadAllBytes(fileName);
            uint currentCrc = EditorServices.EditorSystem.ComputeCrc32(currentBytes);

            if (_data.Length == currentBytes.Length && myCrc == currentCrc)
            {
                return RenderStatus.Same;
            }
        }

        var dirName = Path.GetDirectoryName(fileName);
        if (!Directory.Exists(dirName))
        {
            Directory.CreateDirectory(dirName);
        }

        using var fs = new FileStream(fileName, FileMode.Create);
        GetStream().CopyTo(fs);

        return RenderStatus.Success;
    }
}

/// <summary>
/// Render result that indicates the content should be uploaded rather than written to a file.
/// </summary>
public sealed class UploadRenderResult : RenderResult
{
    /// <summary>
    /// Singleton instance of the upload render result.
    /// </summary>
    public static readonly UploadRenderResult Instance = new();

    private UploadRenderResult()
        : base(RenderStatus.SameAndDbUpdated)
    {
    }

    /// <inheritdoc/>
    public override bool IsBinary => true;

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
        return RenderStatus.SameAndDbUpdated;
    }
}

/// <summary>
/// Render result that wraps a segment collection for incremental code updates.
/// </summary>
public sealed class SegmentRenderResult : RenderResult
{
    /// <summary>
    /// Gets the render segment collection.
    /// </summary>
    public IRenderSegmentCollection Segments { get; }

    /// <inheritdoc/>
    public override bool IsBinary => false;

    /// <summary>
    /// Creates a segment render result.
    /// </summary>
    /// <param name="segments">The render segment collection.</param>
    /// <param name="status">The render status.</param>
    public SegmentRenderResult(IRenderSegmentCollection segments, RenderStatus status)
        : base(status)
    {
        Segments = segments ?? throw new ArgumentNullException(nameof(segments));
    }

    /// <inheritdoc/>
    public override string GetText() => Segments.GetCode();

    /// <inheritdoc/>
    public override Stream GetStream()
    {
        return new MemoryStream(Encoding.UTF8.GetBytes(Segments.GetCode()));
    }
}