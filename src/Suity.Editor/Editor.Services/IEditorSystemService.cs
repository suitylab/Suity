using Suity.Editor.CodeRender;
using Suity.Synchonizing.Core;
using System;
using System.Reflection;

namespace Suity.Editor.Services;

/// <summary>
/// Service interface for core editor system operations.
/// </summary>
public interface IEditorSystemService
{
    /// <summary>
    /// Creates a data input list.
    /// </summary>
    /// <param name="parent">The parent sync path object.</param>
    /// <param name="propertyName">The property name.</param>
    /// <returns>A new data input list.</returns>
    IDataInputList CreateDataInputList(ISyncPathObject parent, string propertyName);

    /// <summary>
    /// Creates a data input item.
    /// </summary>
    /// <param name="dataInput">The data input.</param>
    /// <returns>A new data input item.</returns>
    IDataInputItem CreateDataInputItem(IDataInput dataInput);


    /// <summary>
    /// Creates a file system watcher.
    /// </summary>
    /// <param name="path">The path to watch.</param>
    /// <param name="owner">Optional owner object.</param>
    /// <param name="enableUnwatch">Whether to enable unwatching.</param>
    /// <returns>A new file system watcher.</returns>
    IEditorFileSystemWatcher CreateFileSystemWatcher(string path, object owner = null, bool enableUnwatch = true);

    /// <summary>
    /// Activates all IInitialize instances.
    /// </summary>
    /// <returns>An array of initialized objects.</returns>
    IInitialize[] ActivateIInitialize();



    /// <summary>
    /// Resolves a type from a type string.
    /// </summary>
    /// <param name="typeString">The type string.</param>
    /// <param name="declaringMethod">Optional declaring method.</param>
    /// <returns>The resolved type, or null if not found.</returns>
    Type ResolveType(string typeString, MethodInfo declaringMethod = null);


    /// <summary>
    /// Generates a random ID.
    /// </summary>
    /// <param name="length">The length of the ID.</param>
    /// <returns>A random ID string.</returns>
    string GenerateRandomId(int length);


    /// <summary>
    /// Encrypts text using RSA.
    /// </summary>
    /// <param name="pubKey">The public key.</param>
    /// <param name="plainText">The plain text.</param>
    /// <returns>The encrypted text.</returns>
    string RsaEncrypt(string pubKey, string plainText);

    /// <summary>
    /// Decrypts text using RSA.
    /// </summary>
    /// <param name="privKey">The private key.</param>
    /// <param name="encryptedText">The encrypted text.</param>
    /// <returns>The decrypted text.</returns>
    string RsaDecrypt(string privKey, string encryptedText);

    /// <summary>
    /// Signs text using RSA.
    /// </summary>
    /// <param name="privKey">The private key.</param>
    /// <param name="text">The text to sign.</param>
    /// <returns>The signature.</returns>
    string RsaSign(string privKey, string text);

    /// <summary>
    /// Verifies an RSA signature.
    /// </summary>
    /// <param name="pubKey">The public key.</param>
    /// <param name="text">The original text.</param>
    /// <param name="signature">The signature to verify.</param>
    /// <returns>True if valid.</returns>
    bool RsaVerify(string pubKey, string text, string signature);


    /// <summary>
    /// Computes CRC32 checksum.
    /// </summary>
    /// <param name="input">The input data.</param>
    /// <returns>The CRC32 checksum.</returns>
    uint ComputeCrc32(byte[] input);
}
