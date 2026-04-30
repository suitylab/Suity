using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;

namespace Suity.Editor;

/// <summary>
/// Meta-data information
/// </summary>
public sealed class MetaDataInfo : IViewObject
{
    internal ISyncObject _metadata;

    /// <summary>
    /// Gets or sets the full name of the package.
    /// </summary>
    public string PackageFullName { get; internal set; } = string.Empty;

    /// <summary>
    /// Gets or sets the metadata sync object.
    /// </summary>
    public ISyncObject MetaData
    {
        get => _metadata;
        internal set => _metadata = value;
    }

    /// <summary>
    /// Synchronizes the object's properties with the given sync context.
    /// </summary>
    /// <param name="sync">The property sync object.</param>
    /// <param name="context">The sync context.</param>
    public void Sync(IPropertySync sync, ISyncContext context)
    {
        PackageFullName = sync.Sync("PackageFullName", PackageFullName, SyncFlag.NotNull, string.Empty);
        _metadata = sync.Sync("MetaData", _metadata);
    }

    /// <summary>
    /// Sets up the view for this object.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    public void SetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(PackageFullName, new ViewProperty("PackageFullName").WithReadOnly());
    }

    /// <summary>
    /// Loads metadata information from a file.
    /// </summary>
    /// <param name="fileName">The file path to load from.</param>
    /// <returns>A new MetaDataInfo instance.</returns>
    public static MetaDataInfo Load(string fileName)
    {
        INodeReader reader = XmlNodeReader.FromFile(fileName, false);

        if (reader.NodeName != "SuityMeta")
        {
            throw new FormatException();
        }

        MetaDataInfo info = new MetaDataInfo();
        Serializer.Deserialize(info, reader, null, null);

        return info;
    }

    /// <summary>
    /// Saves metadata information to a file.
    /// </summary>
    /// <param name="info">The metadata info to save.</param>
    /// <param name="fileName">The file path to save to.</param>
    public static void Save(MetaDataInfo info, string fileName)
    {
        XmlNodeWriter writer = new XmlNodeWriter("SuityMeta");
        writer.SetAttribute("version", "1.0");

        Serializer.Serialize(info, writer, null, null);
        writer.SaveToFile(fileName);
    }

    /// <summary>
    /// Exports metadata information to a file with data export intent.
    /// </summary>
    /// <param name="info">The metadata info to export.</param>
    /// <param name="fileName">The file path to export to.</param>
    public static void Export(MetaDataInfo info, string fileName)
    {
        XmlNodeWriter writer = new XmlNodeWriter("SuityMeta");
        writer.SetAttribute("version", "1.0");

        Serializer.Serialize(info, writer, null, null, SyncIntent.DataExport);
        writer.SaveToFile(fileName);
    }
}