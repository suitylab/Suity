using Suity.Editor.Documents;
using Suity.Helpers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Suity.Editor.Services;

/// <summary>
/// Service for generating and managing file and folder names, including incremental naming.
/// </summary>
public class FileNameServiceBK : IFileNameService
{
    /// <summary>
    /// Singleton instance of the file name service.
    /// </summary>
    public static readonly FileNameServiceBK Instance = new();


    /// <inheritdoc/>
    public async Task<string> ShowCreateDocumentDialogAsync(string basePath, string defaultName, string ext)
    {
        if (!Directory.Exists(basePath))
        {
            return null;
        }

        int num = 0;
        string name = null;
        string fileName = null;

        while (true)
        {
            num++;
            name = string.Format("{0}{1:00}", defaultName, num);
            fileName = name + "." + ext;
            if (!File.Exists(Path.Combine(basePath, fileName)))
            {
                break;
            }
        }

        name = await DialogUtility.ShowSingleLineTextDialogAsyncL("Create Document", name, test =>
        {
            if (!NamingVerifier.VerifyFileName(test))
            {
                return false;
            }

            if (File.Exists(Path.Combine(basePath, test + "." + ext)))
            {
                return false;
            }
            else
            {
                return true;
            }
        });

        if (!string.IsNullOrEmpty(name))
        {
            return name + "." + ext;
        }
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public string GetIncrementalFileName(string basePath, string defaultName, string ext)
    {
        if (string.IsNullOrWhiteSpace(defaultName))
        {
            throw new ArgumentNullException(nameof(defaultName));
        }

        if (!string.IsNullOrWhiteSpace(basePath) && !Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        // Prefer to use default filename
        string defaultFileName = $"{defaultName}.{ext}";
        string defaultFullName = Path.Combine(basePath, defaultFileName);
        //if (!File.Exists(defaultFullName))
        //{
        //    return defaultFileName;
        //}

        ext = ext.Trim(' ', '.');

        // Generate filename with incrementing numbers
        int num = 0;
        while (true)
        {
            num++;
            string name = string.Format("{0}{1:00}", defaultName, num);
            string fileName = $"{name}.{ext}";
            string fullName = Path.Combine(basePath, fileName);
            if (!File.Exists(fullName))
            {
                return fileName;
            }
        }
    }

    /// <inheritdoc/>
    public string GetIncrementalFolderName(string basePath, string defaultName)
    {
        if (string.IsNullOrWhiteSpace(defaultName))
        {
            throw new ArgumentNullException(nameof(defaultName));
        }

        if (!string.IsNullOrWhiteSpace(basePath) && !Directory.Exists(basePath))
        {
            Directory.CreateDirectory(basePath);
        }

        // Prefer to use default folder name
        string defaultFullName = Path.Combine(basePath, defaultName);
        if (!Directory.Exists(defaultFullName))
        {
            return defaultName;
        }

        // Generate folder name with incrementing numbers
        int num = 0;
        while (true)
        {
            num++;
            string folderName = string.Format("{0}{1:00}", defaultName, num);
            string fullName = Path.Combine(basePath, folderName);
            if (!File.Exists(fullName))
            {
                return folderName;
            }
        }
    }
}
