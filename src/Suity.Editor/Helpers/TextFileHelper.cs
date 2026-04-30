using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Suity.Helpers;

/// <summary>
/// Provides utility methods for reading, writing, comparing, and managing text files with automatic encoding detection.
/// </summary>
public static class TextFileHelper
{
    /// <summary>
    /// Reads the entire contents of a text file with automatic encoding detection.
    /// </summary>
    /// <param name="fileName">The path to the file to read.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadFile(string fileName)
    {
        EncodingFileInfo info = GetEncodingFileInfo(fileName);
        return info.Contents;
    }

    /// <summary>
    /// Reads the entire contents of a text file using the specified encoding.
    /// </summary>
    /// <param name="fileName">The path to the file to read.</param>
    /// <param name="encoding">The encoding to use when reading the file.</param>
    /// <returns>The contents of the file as a string.</returns>
    public static string ReadFile(string fileName, Encoding encoding)
    {
        using (StreamReader sr = new StreamReader(fileName, encoding))
        {
            String src = sr.ReadToEnd();
            sr.Close();
            return src;
        }
    }

    /// <summary>
    /// Reads the entire contents of a stream using the specified encoding.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The encoding to use when reading the stream.</param>
    /// <returns>The contents of the stream as a string.</returns>
    public static string ReadFile(Stream stream, Encoding encoding)
    {
        using (StreamReader sr = new StreamReader(stream, encoding))
        {
            String src = sr.ReadToEnd();
            sr.Close();
            return src;
        }
    }

    /// <summary>
    /// Reads the entire contents of a byte array with automatic encoding detection.
    /// </summary>
    /// <param name="bytes">The byte array to read from.</param>
    /// <returns>The contents of the byte array as a string.</returns>
    public static string ReadFile(byte[] bytes)
    {
        EncodingFileInfo info = _GetEncodingFileInfo(bytes);
        return info.Contents;
    }

    /// <summary>
    /// Attempts to read a text file, returning null if the file doesn't exist or an error occurs.
    /// </summary>
    /// <param name="fileName">The path to the file to read.</param>
    /// <param name="encoding">The encoding parameter (unused, defaults to UTF-8).</param>
    /// <returns>The file contents, or null if the file doesn't exist or an error occurs.</returns>
    public static string TryReadAllText(string fileName, Encoding encoding)
    {
        if (!File.Exists(fileName))
        {
            return null;
        }

        try
        {
            return ReadFile(fileName, Encoding.UTF8);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to read a stream, returning null if the stream is null or an error occurs.
    /// </summary>
    /// <param name="stream">The stream to read from.</param>
    /// <param name="encoding">The encoding parameter (unused, defaults to UTF-8).</param>
    /// <returns>The stream contents, or null if the stream is null or an error occurs.</returns>
    public static string TryReadAllText(Stream stream, Encoding encoding)
    {
        if (stream == null)
        {
            return null;
        }

        try
        {
            return ReadFile(stream, Encoding.UTF8);
        }
        catch (Exception)
        {
            return null;
        }
    }

    /// <summary>
    /// Writes text to a file using UTF-8 encoding with BOM.
    /// </summary>
    /// <param name="fileName">The path to the file to write.</param>
    /// <param name="text">The text content to write.</param>
    public static void WriteFile(string fileName, string text)
    {
        WriteFile(fileName, text, new UTF8Encoding(true), true);
    }

    /// <summary>
    /// Writes text to a stream using UTF-8 encoding with BOM.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="text">The text content to write.</param>
    public static void WriteFile(Stream stream, string text)
    {
        WriteFile(stream, text, new UTF8Encoding(true), true);
    }

    /// <summary>
    /// Writes text to a file with the specified encoding and BOM option. Creates directories if needed.
    /// </summary>
    /// <param name="fileName">The path to the file to write.</param>
    /// <param name="text">The text content to write.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <param name="saveBOM">Whether to include a byte order mark.</param>
    public static void WriteFile(string fileName, string text, Encoding encoding, bool saveBOM)
    {
        if (text == null)
        {
            text = string.Empty;
        }

        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }

        string dir = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        Boolean useSkipBomWriter = (encoding == Encoding.UTF8 && !saveBOM);
        if (encoding == Encoding.UTF7) encoding = new UTF7EncodingFixed();
        using (StreamWriter sw = useSkipBomWriter ? new StreamWriter(fileName, false) : new StreamWriter(fileName, false, encoding))
        {
            sw.Write(text);
            sw.Close();
        }
    }

    /// <summary>
    /// Writes text to a stream with the specified encoding and BOM option.
    /// </summary>
    /// <param name="stream">The stream to write to.</param>
    /// <param name="text">The text content to write.</param>
    /// <param name="encoding">The encoding to use.</param>
    /// <param name="saveBOM">Whether to include a byte order mark.</param>
    public static void WriteFile(Stream stream, string text, Encoding encoding, bool saveBOM)
    {
        if (text == null)
        {
            text = string.Empty;
        }

        bool useSkipBomWriter = (encoding == Encoding.UTF8 && !saveBOM);
        if (useSkipBomWriter)
        {
            encoding = Encoding.Default;
        }
        else if (encoding == Encoding.UTF7)
        {
            encoding = new UTF7EncodingFixed();
        }

        using (StreamWriter sw = new StreamWriter(stream, Encoding.Default, 1024, true))
        {
            sw.Write(text);
            sw.Close();
        }
    }

    /// <summary>
    /// Normalizes line endings by converting Windows-style CRLF to Unix-style LF.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The text with LF line endings.</returns>
    public static string FixNewline(string text)
    {
        return text.Replace("\r\n", "\n");
    }

    /// <summary>
    /// Writes code to a file only if the content has changed, using UTF-8 encoding.
    /// </summary>
    /// <param name="fileName">The path to the file to write.</param>
    /// <param name="code">The code content to write.</param>
    public static void CompareWrite(string fileName, string code)
    {
        CompareWrite(fileName, code, Encoding.UTF8);
    }

    /// <summary>
    /// Writes code to a file only if the content has changed, using the specified encoding.
    /// </summary>
    /// <param name="fileName">The path to the file to write.</param>
    /// <param name="code">The code content to write.</param>
    /// <param name="encoding">The encoding to use.</param>
    public static void CompareWrite(string fileName, string code, Encoding encoding)
    {
        byte[] data = null;

        using (var stream = new MemoryStream())
        {
            StreamWriter writer = new StreamWriter(stream, encoding);
            writer.Write(code);
            writer.Close();
            data = stream.ToArray();
        }
        if (data == null)
        {
            return;
        }

        CompareWrite(fileName, data);
    }

    /// <summary>
    /// Writes data to a file only if the content differs from the existing file, avoiding unnecessary writes.
    /// </summary>
    /// <param name="fileName">The path to the file to write.</param>
    /// <param name="data">The byte data to write.</param>
    public static void CompareWrite(string fileName, byte[] data)
    {
        if (File.Exists(fileName))
        {
            if (Compare(data, fileName))
            {
                return;
            }
            else
            {
                File.Delete(fileName);
            }
        }

        string dir = Path.GetDirectoryName(fileName);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllBytes(fileName, data);
    }

    /// <summary>
    /// Compares byte data with the contents of a file to determine if they are identical.
    /// </summary>
    /// <param name="data">The byte data to compare.</param>
    /// <param name="fileName">The path to the file to compare against.</param>
    /// <returns>True if the file exists and its contents match the data exactly.</returns>
    public static bool Compare(byte[] data, string fileName)
    {
        if (!File.Exists(fileName))
        {
            return false;
        }

        byte[] other = File.ReadAllBytes(fileName);
        if (data.Length != other.Length)
        {
            return false;
        }
        for (int i = 0; i < data.Length; i++)
        {
            if (data[i] != other[i])
            {
                return false;
            }
        }
        return true;
    }

    /// <summary>
    /// Creates a backup of a file by renaming it with a numbered .bak extension.
    /// </summary>
    /// <param name="path">The path to the file to back up.</param>
    public static void BackupFile(string path)
    {
        FileInfo info = new FileInfo(path);
        string ext = info.Extension;

        int number = 0;

        if (File.Exists(path))
        {
            string backupPath;
            do
            {
                number++;
                backupPath = String.Format("{0}.bak{1}{2}", path, number.ToString("000"), ext);
            }
            while (File.Exists(backupPath));
            File.Move(path, backupPath);
        }
    }

    /// <summary>
    /// Ensures that the file name is unique by adding a number to it
    /// </summary>
    public static String EnsureUniquePath(String original)
    {
        Int32 counter = 0;
        String result = original;
        String folder = Path.GetDirectoryName(original);
        String filename = Path.GetFileNameWithoutExtension(original);
        String extension = Path.GetExtension(original);
        while (File.Exists(result))
        {
            counter++;
            String fullname = filename + " (" + counter + ")" + extension;
            result = Path.Combine(folder, fullname);
        }
        return result;
    }

    /// <summary>
    /// Checks that if the file is read only
    /// </summary>
    public static Boolean FileIsReadOnly(String file)
    {
        try
        {
            if (!File.Exists(file)) return false;
            FileAttributes fileAttr = File.GetAttributes(file);
            if ((fileAttr & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) return true;
            else return false;
        }
        catch (Exception)
        {
            return false;
        }
    }

    /// <summary>
    /// Determines whether two files have identical contents by comparing file size, optionally last write time, and file hash.
    /// </summary>
    /// <param name="fileName1">The path to the first file.</param>
    /// <param name="fileName2">The path to the second file.</param>
    /// <param name="timeCompare">If true, considers files identical when last write times match and sizes are equal.</param>
    /// <returns>True if both files exist and have identical contents.</returns>
    public static bool IsFileSame(string fileName1, string fileName2, bool timeCompare = true)
    {
        if (!File.Exists(fileName1)) return false;
        if (!File.Exists(fileName2)) return false;

        FileInfo fileInfo1 = new FileInfo(fileName1);
        FileInfo fileInfo2 = new FileInfo(fileName2);
        if (fileInfo1.Length != fileInfo2.Length) return false;

        if (timeCompare && fileInfo1.LastWriteTime == fileInfo2.LastWriteTime) return true;

        //Calculate hash value of the first file
        var hash = HashAlgorithm.Create();
        var stream1 = new FileStream(fileName1, FileMode.Open);
        byte[] hashByte1 = hash.ComputeHash(stream1);
        stream1.Close();
        //Calculate hash value of the second file
        var stream2 = new FileStream(fileName2, FileMode.Open);
        byte[] hashByte2 = hash.ComputeHash(stream2);
        stream2.Close();

        if (hashByte1.Length != hashByte2.Length) return false;
        for (int i = 0; i < hashByte1.Length; i++)
        {
            if (hashByte1[i] != hashByte2[i]) return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the last write time of a file, returning DateTime.MinValue if the file doesn't exist.
    /// </summary>
    /// <param name="fileName">The path to the file.</param>
    /// <returns>The last write time of the file, or DateTime.MinValue if not found.</returns>
    public static DateTime GetFileLastWriteTime(string fileName)
    {
        if (!File.Exists(fileName)) return DateTime.MinValue;

        FileInfo fileInfo = new FileInfo(fileName);
        return fileInfo.LastWriteTime;
    }

    /// <summary>
    /// Checks whether a file exists at the specified path.
    /// </summary>
    /// <param name="fullPath">The full path to check.</param>
    /// <returns>True if the file exists, false otherwise.</returns>
    public static bool IsFileExists(string fullPath)
    {
        return File.Exists(fullPath);
    }

    /// <summary>
    /// Checks whether a file exists and verifies that the file name matches the actual case on disk.
    /// </summary>
    /// <param name="fileName">The file path to check.</param>
    /// <returns>True if the file exists and its name matches the case on disk exactly.</returns>
    public static bool IsFileExistAndMatchCase(string fileName)
    {
        fileName = fileName.Replace('/', '\\');
        if (!File.Exists(fileName))
        {
            return false;
        }

        FileInfo fileInfo = new FileInfo(fileName);
        DirectoryInfo parentDirInfo = fileInfo.Directory;
        if (parentDirInfo == null) return true;

        FileInfo[] searchResults = parentDirInfo.GetFiles(fileInfo.Name);
        foreach (FileInfo searchResult in searchResults)
        {
            if (searchResult.FullName == fileName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks whether a directory exists and verifies that the directory name matches the actual case on disk.
    /// </summary>
    /// <param name="dirName">The directory path to check.</param>
    /// <returns>True if the directory exists and its name matches the case on disk exactly.</returns>
    public static bool IsDirectoryExistAndMatchCase(string dirName)
    {
        dirName = dirName.Replace('/', '\\');
        if (!Directory.Exists(dirName))
        {
            return false;
        }
        dirName = dirName.TrimEnd('\\');

        DirectoryInfo dirInfo = new DirectoryInfo(dirName);
        DirectoryInfo parentDirInfo = dirInfo.Parent;
        if (parentDirInfo == null) return true;

        DirectoryInfo[] searchResults = parentDirInfo.GetDirectories(dirInfo.Name);
        foreach (DirectoryInfo searchResult in searchResults)
        {
            if (searchResult.FullName == dirName)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Detect if a file is text and detect the encoding.
    /// </summary>
    /// <param name="encoding">
    /// The detected encoding.
    /// </param>
    /// <param name="fileName">
    /// The file name.
    /// </param>
    /// <param name="windowSize">
    /// The number of characters to use for testing.
    /// </param>
    /// <returns>
    /// true if the file is text.
    /// </returns>
    public static bool IsText(out Encoding encoding, string fileName, int windowSize)
    {
        using (var fileStream = File.OpenRead(fileName))
        {
            return IsText(out encoding, fileStream, windowSize);
        }
    }

    public static bool IsText(out Encoding encoding, Stream stream, int windowSize)
    {
        var rawData = new byte[windowSize];
        var text = new char[windowSize];
        var isText = true;

        // Read raw bytes
        var rawLength = stream.Read(rawData, 0, rawData.Length);
        stream.Seek(0, SeekOrigin.Begin);

        // Detect encoding correctly (from Rick Strahl's blog)
        // http://www.west-wind.com/weblog/posts/2007/Nov/28/Detecting-Text-Encoding-for-StreamReader
        if (rawData[0] == 0xef && rawData[1] == 0xbb && rawData[2] == 0xbf)
        {
            encoding = Encoding.UTF8;
        }
        else if (rawData[0] == 0xfe && rawData[1] == 0xff)
        {
            encoding = Encoding.Unicode;
        }
        else if (rawData[0] == 0 && rawData[1] == 0 && rawData[2] == 0xfe && rawData[3] == 0xff)
        {
            encoding = Encoding.UTF32;
        }
        else if (rawData[0] == 0x2b && rawData[1] == 0x2f && rawData[2] == 0x76)
        {
            encoding = Encoding.UTF7;
        }
        else
        {
            encoding = Encoding.Default;
        }

        // Read text and detect the encoding
        using (var streamReader = new StreamReader(stream))
        {
            streamReader.Read(text, 0, text.Length);
        }

        using (var memoryStream = new MemoryStream())
        {
            using (var streamWriter = new StreamWriter(memoryStream, encoding))
            {
                // Write the text to a buffer
                streamWriter.Write(text);
                streamWriter.Flush();

                // Get the buffer from the memory stream for comparision
                var memoryBuffer = memoryStream.GetBuffer();

                // Compare only bytes read
                for (var i = 0; i < rawLength && isText; i++)
                {
                    isText = rawData[i] == memoryBuffer[i];
                }
            }
        }

        return isText;
    }

    /// <summary>
    /// Opens Windows Explorer to navigate to the specified folder path.
    /// </summary>
    /// <param name="path">The folder path to open in Explorer.</param>
    public static void NavigateFolder(string path)
    {
        try
        {
            Process.Start("explorer.exe", "/e, " + path.Replace('/', '\\'));
        }
        catch (Exception)
        {
        }
    }

    /// <summary>
    /// Opens Windows Explorer and selects the specified file. Falls back to opening the parent folder if access is denied.
    /// </summary>
    /// <param name="path">The file path to select in Explorer.</param>
    public static void NavigateFile(string path)
    {
        try
        {
            Process.Start("explorer.exe", "/select, " + path.Replace('/', '\\'));

            return;
        }
        catch (Exception)
        {
            // Access is prohibited due to permission issues
        }

        // Open the parent folder
        string folderPath = Path.GetDirectoryName(path);
        Process.Start(folderPath);
    }

    /// <summary>
    /// Reads a file and returns an EncodingFileInfo object containing the detected encoding code page and file contents.
    /// </summary>
    /// <param name="file">The path to the file to read.</param>
    /// <returns>An EncodingFileInfo with the detected encoding and file contents.</returns>
    public static EncodingFileInfo GetEncodingFileInfo(String file)
    {
        EncodingFileInfo info;

        try
        {
            if (File.Exists(file))
            {
                var bytes = File.ReadAllBytes(file);
                info = _GetEncodingFileInfo(bytes);
            }
            else
            {
                info = new EncodingFileInfo();
            }
        }
        catch (Exception)
        {
            info = new EncodingFileInfo();
        }
        return info;
    }

    /// <summary>
    /// Processes a byte array and returns an EncodingFileInfo object containing the detected encoding code page and decoded contents.
    /// </summary>
    /// <param name="bytes">The byte array to process.</param>
    /// <returns>An EncodingFileInfo with the detected encoding and decoded contents.</returns>
    public static EncodingFileInfo GetEncodingFileInfo(byte[] bytes)
    {
        EncodingFileInfo info;

        try
        {
            if (bytes != null)
            {
                info = _GetEncodingFileInfo(bytes);
            }
            else
            {
                info = new EncodingFileInfo();
            }
        }
        catch (Exception)
        {
            info = new EncodingFileInfo();
        }
        return info;
    }

    private static EncodingFileInfo _GetEncodingFileInfo(byte[] bytes)
    {
        var info = new EncodingFileInfo();
        if (bytes == null || bytes.Length == 0) return info;

        // Use MemoryStream with StreamReader to automatically handle BOM
        using (var ms = new MemoryStream(bytes))
        {
            // Key point: The second parameter is set to Encoding.UTF8, and the third parameter true indicates BOM detection
            // For files without BOM, we need a fallback solution
            using (var reader = new StreamReader(ms, Encoding.UTF8, true))
            {
                reader.Peek(); // Trigger detection behavior
                var encoding = reader.CurrentEncoding;

                // If the detection result is UTF8 but it actually contains invalid bytes (might be GBK)
                // Here you can keep your ContainsInvalidUTF8Bytes logic
                if (encoding.CodePage == Encoding.UTF8.CodePage && ContainsInvalidUTF8Bytes(bytes))
                {
                    // Force try GBK (CodePage 936) or system default
                    // Note: .NET Core needs to register CodePagesEncodingProvider first
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    encoding = Encoding.GetEncoding(936);
                }

                info.CodePage = encoding.CodePage;
                ms.Position = 0; // Reset stream position to read again

                // Create a new reader with correct encoding to read content
                using (var finalReader = new StreamReader(ms, encoding))
                {
                    info.Contents = finalReader.ReadToEnd();
                }
            }
        }
        return info;
    }

    private static Boolean ContainsInvalidUTF8Bytes(Byte[] bytes)
    {
        Int32 bits = 0;
        Int32 i = 0, c = 0, b = 0;
        Int32 length = bytes.Length;
        for (i = 0; i < length; i++)
        {
            c = bytes[i];
            if (c > 128)
            {
                if ((c >= 254)) return true;
                else if (c >= 252) bits = 6;
                else if (c >= 248) bits = 5;
                else if (c >= 240) bits = 4;
                else if (c >= 224) bits = 3;
                else if (c >= 192) bits = 2;
                else return true;
                if ((i + bits) > length) return true;
                while (bits > 1)
                {
                    i++;
                    b = bytes[i];
                    if (b < 128 || b > 191) return true;
                    bits--;
                }
            }
        }
        return false;
    }
}

    /// <summary>
    /// A corrected UTF-7 encoding implementation that properly returns the UTF-7 byte order mark.
    /// </summary>
    public class UTF7EncodingFixed : UTF7Encoding
    {
        /// <summary>
        /// Returns the UTF-7 BOM byte sequence (0x2B, 0x2F, 0x76, 0x38, 0x2D).
        /// </summary>
        /// <returns>The UTF-7 BOM as a byte array.</returns>
        public override byte[] GetPreamble()
    {
        return [0x2B, 0x2F, 0x76, 0x38, 0x2D];
    }
}

/// <summary>
/// Holds encoding information and decoded file contents, including BOM detection details.
/// </summary>
public class EncodingFileInfo
{
    /// <summary>
    /// The code page identifier of the detected encoding, or -1 if undetermined.
    /// </summary>
    public Int32 CodePage = -1;

    /// <summary>
    /// The decoded text contents of the file.
    /// </summary>
    public String Contents = String.Empty;

    /// <summary>
    /// Indicates whether the file contains a byte order mark.
    /// </summary>
    public Boolean ContainsBOM = false;

    /// <summary>
    /// The length in bytes of the detected BOM.
    /// </summary>
    public Int32 BomLength = 0;
}