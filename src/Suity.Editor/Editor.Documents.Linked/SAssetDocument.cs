using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Core;
using Suity.Views;
using System;

namespace Suity.Editor.Documents.Linked;

/// <summary>
/// Represents an asset document that supports synchronization and view setup operations within the asset management
/// system.
/// </summary>
public class SAssetDocument : AssetDocument,
    ISyncObject, IViewObject
{
    bool _resaveRequired;

    public SAssetDocument()
    {
    }

    public SAssetDocument(AssetBuilder builder)
        : base(builder)
    {
    }

    #region ISyncObject IViewObject

    void ISyncObject.Sync(IPropertySync sync, ISyncContext context)
    {
        if (sync.Intent == SyncIntent.Serialize && sync.IsNameOf("AssetId"))
        {
            if (sync.IsGetter())
            {
                Guid id = this.Id;
                if (id != Guid.Empty)
                {
                    sync.Sync("AssetId", this.Id);
                }
            }
            else if (sync.IsSetter() && this.AssetBuilder is { } builder)
            {
                Guid id = sync.Sync("AssetId", this.Id);
                builder.SetRecordedId(id);
            }
        }


        OnSyncInternal(sync, context);
    }

    /// <summary>
    /// Internal method for synchronization.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    /// <param name="context">The sync context.</param>
    internal virtual void OnSyncInternal(IPropertySync sync, ISyncContext context)
    {
        OnSync(sync, context);
    }

    /// <summary>
    /// Called during synchronization.
    /// </summary>
    /// <param name="sync">The property sync.</param>
    /// <param name="context">The sync context.</param>
    protected virtual void OnSync(IPropertySync sync, ISyncContext context)
    { }

    void IViewObject.SetupView(IViewObjectSetup setup)
    {
        OnSetupViewInternal(setup);
    }

    /// <summary>
    /// Internal method for view setup.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    internal virtual void OnSetupViewInternal(IViewObjectSetup setup)
    {
        OnSetupView(setup);
    }

    /// <summary>
    /// Called to set up the view.
    /// </summary>
    /// <param name="setup">The view object setup.</param>
    protected virtual void OnSetupView(IViewObjectSetup setup)
    {
    }

    #endregion

    #region Virtual

    /// <inheritdoc/>
    protected internal override bool LoadDocument(IStorageItem op, object loaderObject, DocumentLoadingIntent intent, bool cloneMode)
    {
        if (loaderObject is not INodeReader reader || !reader.Exist)
        {
            reader = XmlNodeReader.FromStream(op.GetInputStream(), false);
        }

        if (!reader.Exist)
        {
            return false;
        }

        var syncIntent = cloneMode ? SyncIntent.Clone : SyncIntent.Serialize;
        Serializer.Deserialize(this, reader, SyncTypeResolver, this, syncIntent);

        return true;
    }

    /// <inheritdoc/>
    protected internal override bool SaveDocument(IStorageItem op)
    {
        var writer = new XmlNodeWriter("SuityAsset");
        writer.SetAttribute("version", "1.0");
        writer.SetAttribute("format", Format.FormatName);

        Serializer.Serialize(this, writer, SyncTypeResolver, this);
        writer.SaveToStream(op.GetOutputStream());
        return true;
    }

    /// <inheritdoc/>
    protected internal override bool ExportDocument(IStorageItem op, bool cloneMode = false)
    {
        var writer = new XmlNodeWriter("SuityAsset");
        writer.SetAttribute("version", "1.0");
        writer.SetAttribute("format", Format.FormatName);

        var intent = cloneMode ? SyncIntent.Clone : SyncIntent.DataExport;
        Serializer.Serialize(this, writer, SyncTypeResolver, this, intent);
        writer.SaveToStream(op.GetOutputStream());
        return true;
    }

    /// <inheritdoc/>
    protected internal override void OnLoaded(DocumentLoadingIntent intent)
    {
        base.OnLoaded(intent);

        var builder = this.AssetBuilder;
        // If Id was not loaded from document, it proves old storage method, need to force save once, otherwise Id will be lost.
        if (builder != null && builder.RecordedId == Guid.Empty) 
        {
            _resaveRequired = true;
        }

        // Will cause importer failure.
        //if (_resaveRequired)
        //{
        //    this.Entry.MarkDirty(this);
        //    this.Entry.SaveDelayed();
        //}
    }

    #endregion
}

/// <summary>
/// Generic SAssetDocument with a specific asset builder type.
/// </summary>
public abstract class SAssetDocument<TAssetBuilder> : SAssetDocument
where TAssetBuilder : AssetBuilder, new()
{
    public SAssetDocument()
        : base(new TAssetBuilder())
    {
    }

    protected internal new TAssetBuilder AssetBuilder => (TAssetBuilder)base.AssetBuilder;
}