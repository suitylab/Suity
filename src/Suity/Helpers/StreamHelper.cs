using System.IO;

namespace Suity.Helpers;

/// <summary>
/// Provides helper methods for stream manipulation.
/// </summary>
public static class StreamHelper
{
    /// <summary>
    /// Convert Stream to byte[]
    /// </summary>
    public static byte[] StreamToBytes(this Stream stream)
    {
        byte[] bytes = new byte[stream.Length];

        // Set the current stream position to the beginning of the stream
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bytes, 0, bytes.Length);

        return bytes;
    }

    /// <summary>
    /// Convert byte[] to Stream
    /// </summary>
    public static Stream BytesToStream(this byte[] bytes)
    {
        Stream stream = new MemoryStream(bytes);
        return stream;
    }

    /// <summary>
    /// Write a Stream to a file
    /// </summary>
    public static void StreamToFile(this Stream stream, string fileName)
    {
        // Convert Stream to byte[]
        byte[] bytes = new byte[stream.Length];

        // Set the current stream position to the beginning of the stream
        stream.Seek(0, SeekOrigin.Begin);
        stream.Read(bytes, 0, bytes.Length);

        // Write byte[] to file
        using var fs = new FileStream(fileName, FileMode.Create);
        using var bw = new BinaryWriter(fs);
        bw.Write(bytes);
        bw.Close();
        fs.Close();
    }

    /// <summary>
    /// Reading a Stream from a File
    /// </summary>
    public static Stream FileToStream(string fileName)
    {
        // Opening a file
        using var fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        // Read byte[] from file
        byte[] bytes = new byte[fileStream.Length];
        fileStream.Read(bytes, 0, bytes.Length);
        fileStream.Close();

        // Convert byte[] to Stream
        return new MemoryStream(bytes);
    }

    public static string ReadAllText(this Stream input)
    {
        using var streamReader = new StreamReader(input);
        return streamReader.ReadToEnd();
    }

    public static void CopyTo(this Stream input, Stream output)
    {
        byte[] array = new byte[32768];
        int count;
        while ((count = input.Read(array, 0, array.Length)) > 0)
        {
            output.Write(array, 0, count);
        }
    }
}