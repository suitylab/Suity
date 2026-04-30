using System.IO;
using System.Text;

namespace Suity;

/// <summary>
/// Provides extension methods for stream manipulation.
/// </summary>
public static class StreamExtensions
{
    public static void CopyTo(this Stream input, Stream output)
    {
        byte[] buffer = new byte[4096];
        int read;

        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
        {
            output.Write(buffer, 0, read);
        }
    }

    public static string ToUTF8Text(this Stream input)
    {
        using var reader = new StreamReader(input, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    public static string ToText(this Stream input, Encoding encoding)
    {
        using var reader = new StreamReader(input, encoding);
        return reader.ReadToEnd();
    }

    public static byte[] ToBytes(this Stream input)
    {
        using var stream = new MemoryStream();
        input.CopyTo(stream, 4096);
        return stream.ToArray();
    }

    public static Stream ToStream(this string s, Encoding encoding)
    {
        var memoryStream = new MemoryStream();

        using (var writer = new StreamWriter(memoryStream, encoding, bufferSize: 4096, leaveOpen: true))
        {
            writer.Write(s);
            writer.Flush();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        return memoryStream;
    }

    public static Stream ToStreamUTF8(this string s) => ToStream(s, Encoding.UTF8);
}