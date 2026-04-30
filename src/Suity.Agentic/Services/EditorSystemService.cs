using Suity.Editor.CodeRender;
using Suity.Helpers;
using Suity.Synchonizing.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Suity.Editor.Services;

public class EditorSystemService : IEditorSystemService
{
    public static readonly EditorSystemService Instance = new();


    public EditorSystemService()
    {
    }

    public IDataInputList CreateDataInputList(ISyncPathObject parent, string propertyName)
    {
        return new DataInputList(parent, propertyName);
    }

    public IDataInputItem CreateDataInputItem(IDataInput dataInput)
    {
        return new DataInput(dataInput);
    }


    public IEditorFileSystemWatcher CreateFileSystemWatcher(string path, object owner = null, bool enableUnwatch = true)
    {
        return new EditorFileSystemWatcher(path, owner, enableUnwatch);
    }

    public IInitialize[] ActivateIInitialize()
    {
        // Exclude editor Assembly
        var myAsm = this.GetType().Assembly;

        var types = typeof(IInitialize).GetDerivedTypes()
            .Where(o => o.Assembly != myAsm)
            .ToArray();

        List<IInitialize> list = [];

        foreach (Type initType in types)
        {
            try
            {
                var init = Activator.CreateInstance(initType) as IInitialize;
                if (init != null)
                {
                    list.Add(init);
                }
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return list.ToArray();
    }


    public Type ResolveType(string typeString, MethodInfo declaringMethod = null)
    {
        return InternalTypeResolve.ResolveType(typeString, declaringMethod);
    }


    public string GenerateRandomId(int length) => IdGenerator.GenerateId(length);


    public string RsaEncrypt(string pubKey, string plainText) => RsaEncryptionHelper.Encrypt(pubKey, plainText);

    public string RsaDecrypt(string privKey, string encryptedText) => RsaEncryptionHelper.Decrypt(privKey, encryptedText);

    public string RsaSign(string privKey, string text) => RsaEncryptionHelper.Sign(privKey, text);

    public bool RsaVerify(string pubKey, string text, string signature) => RsaEncryptionHelper.Verify(pubKey, text, signature);


    public uint ComputeCrc32(byte[] input) => Crc32Algorithm.Compute(input);
}
