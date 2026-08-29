using Suity.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Suity.Editor;

public static class CliMagicLine
{
    public const string MagicPrefix = "[SUITY_CMD]->";
    public static string GetMagicLine(object obj, string sid = null)
    {
        var writer = new JsonDataWriter();

        if (obj is string str)
        {
            writer.Node("@type").WriteString("String");
            writer.Node("value").WriteString(str);
        }
        else if (obj is string[] strs)
        {
            writer.Node("@type").WriteString("StringArray");
            var aryWriter = writer.Nodes("values", strs.Length);
            foreach (var str2 in strs)
            {
                var strWriter = aryWriter.Item();
                strWriter.WriteString(str2);
            }
        }
        else if (obj is IDataWritable writable)
        {
            // writer.Node("@type").WriteString(writable.GetType().FullName);
            writable.WriteData(writer);
        }
        else if (obj is Exception ex)
        {
            writer.Node("@type").WriteString("Exception");
            writer.Node("exception").WriteString(ex.GetType().FullName);
            writer.Node("message").WriteString(ex.Message);
            writer.Node("stackTrace").WriteString(ex.StackTrace);
        }
        else
        {
            writer.Node("@type").WriteString("String");
            writer.Node("value").WriteString(obj?.ToString() ?? string.Empty);
        }

        if (!string.IsNullOrWhiteSpace(sid))
        {
            writer.Node("sid").WriteString(sid);
        }
        
        string json = writer.ToString(false);
        byte[] b = Encoding.UTF8.GetBytes(json);
        string base64 = Convert.ToBase64String(b);

        return MagicPrefix + base64;
    }

    public static JsonDataReader ParseMagicLine(string line)
    {
        string base64 = line[MagicPrefix.Length..];
        byte[] bytes = Convert.FromBase64String(base64);
        string json = System.Text.Encoding.UTF8.GetString(bytes);

        var reader = new JsonDataReader(json);

        return reader;
    }

    public static object ParseReader(JsonDataReader reader)
    {
        if (reader is null)
        {
            throw new InvalidOperationException("Failed to receive response from the CLI process. (reader is null)");
        }

        string type = reader.Node("@type").ReadString();
        if (string.IsNullOrWhiteSpace(type))
        {
            throw new InvalidOperationException("Failed to receive response from the CLI process. (no type)");
        }

        if (type == "Exception")
        {
            string typeName = reader.Node("exception").ReadString();
            string message = reader.Node("message").ReadString();
            string stackTrace = reader.Node("stackTrace").ReadString();

            var remoteException = new RemoteCliException(message)
            {
                RemoteTypeName = typeName,
                RemoteStackTrace = stackTrace,
            };

            throw remoteException;
        }

        if (type == "String")
        {
            return reader.Node("value").ReadString();
        }
        else if (type == "StringArray")
        {
            var valuesNode = reader.Nodes("values");
            List<string> values = new List<string>();
            foreach (var valueNode in valuesNode)
            {
                values.Add(valueNode.ReadString());
            }
            return values.ToArray();
        }
        else
        {
            return reader;
        }
    }

    public static void OutputMagicLine(object obj, string sid = null)
    {
        string line = GetMagicLine(obj, sid);
        Console.WriteLine(line);
    }
}