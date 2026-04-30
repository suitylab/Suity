using Suity.NodeQuery;

namespace Suity.Synchonizing.Core;

public static class XmlSerializer
{
    public static void SerializeToFile(object obj, string fileName, SyncIntent intent = SyncIntent.Serialize)
    {
        var writer = new XmlNodeWriter("Item");
        Serializer.Serialize(obj, writer, intent);
        writer.SaveToFile(fileName);
    }

    public static void DeserializeFromFile(object obj, string fileName)
    {
        var reader = XmlNodeReader.FromFile(fileName, false);
        Serializer.Deserialize(obj, reader);
    }
}