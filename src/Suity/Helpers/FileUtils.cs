using System.IO;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides file utility methods.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// UTF-8 encoding without BOM.
    /// </summary>
    public static Encoding UTF8NoBom { get; } = new UTF8Encoding(false);


    public static string Read(string fileName)
    {
        return Read(fileName, Encoding.UTF8);
    }

    public static string Read(string fileName, Encoding encoding)
    {
        if (!File.Exists(fileName))
        {
            throw new IOException("File not found:" + fileName);
        }

        using var fs = File.OpenRead(fileName);
        using var sr = new StreamReader(fs, encoding);
        sr.BaseStream.Seek(0, SeekOrigin.Begin);
        string data = sr.ReadToEnd();
        sr.Close();

        return data;
    }

    public static void Write(string fileName, string text)
    {
        Write(fileName, text, Encoding.UTF8);
    }

    public static void Write(string fileName, string text, Encoding encoding)
    {
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        string dir = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        using var stream = File.OpenWrite(fileName);
        using var writer = new StreamWriter(stream, encoding);
        writer.Write(text);
        writer.Close();
    }

    public static string GetAutoNewFileName(string fileName, string extension)
    {
        string newFileName = $"{fileName}.{extension}";

        if (File.Exists(newFileName))
        {
            int index = 1;
            while (true)
            {
                newFileName = $"{fileName}{index}.{extension}";

                if (!File.Exists(newFileName))
                {
                    break;
                }
                else
                {
                    index++;
                }
            }
        }

        return newFileName;
    }
}