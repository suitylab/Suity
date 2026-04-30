using Suity.Editor.Expressions;
using System;
using System.Collections.Generic;

namespace Suity.Editor.CodeRender;

/// <summary>
/// Base class for render targets that produce code output.
/// </summary>
public abstract class RenderTarget
{
    protected RenderTarget(Guid id, RenderFileName fileName)
    {
        OwnerId = id;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

        Language = string.Empty;
        Location = string.Empty;
    }

    protected RenderTarget(Guid id, RenderFileName fileName, string language)
    {
        OwnerId = id;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));

        Language = language ?? throw new ArgumentNullException(nameof(language));
        Location = string.Empty;
    }

    protected RenderTarget(Guid id, RenderFileName fileName, DateTime updateTime)
    {
        OwnerId = id;
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        LastUpdateTime = updateTime;

        Language = string.Empty;
        Location = string.Empty;
    }

    protected RenderTarget(RenderItem item, IMaterial material, RenderFileName fileName)
        : this(item, material, fileName, null, null)
    {
    }

    protected RenderTarget(RenderItem item, IMaterial material, RenderFileName fileName, string language, string location)
    {
        Item = item ?? throw new ArgumentNullException(nameof(item));
        Material = material ?? throw new ArgumentNullException(nameof(material));
        FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        OwnerId = item?.Renderable?.Id ?? Guid.Empty;

        Language = language ?? string.Empty;
        Location = location ?? string.Empty;

        LastUpdateTime = material.LastUpdateTime > item.LastUpdateTime ? material.LastUpdateTime : item.LastUpdateTime;
    }

    protected RenderTarget(RenderFileName fileName)
    {
        FileName = fileName;
    }

    protected RenderTarget(RenderTarget copyFrom, RenderItem item = null, IMaterial material = null, RenderFileName fileName = null, Guid? ownerId = null, string language = null, string location = null)
    {
        if (copyFrom is null)
        {
            throw new ArgumentNullException(nameof(copyFrom));
        }

        Item = item ?? copyFrom.Item;
        Material = material ?? copyFrom.Material;
        FileName = fileName ?? copyFrom.FileName;
        OwnerId = ownerId ?? copyFrom.OwnerId;

        Language = language ?? copyFrom.Language;
        Location = location ?? copyFrom.Location;

        LastUpdateTime = copyFrom.LastUpdateTime;

        Suspended = copyFrom.Suspended;
        Tag = copyFrom.Tag;
        RestoreCodeLibrary = copyFrom.RestoreCodeLibrary;
        UserCodeCount = copyFrom.UserCodeCount;
    }

    #region Read-only Properties

    /// <summary>
    /// Owner id.
    /// </summary>
    public Guid OwnerId { get; }

    /// <summary>
    /// Render item id.
    /// </summary>
    public Guid RenderItemId => Item?.Id ?? OwnerId;

    /// <summary>
    /// Render object.
    /// </summary>
    public RenderItem Item { get; }

    /// <summary>
    /// Material.
    /// </summary>
    public IMaterial Material { get; }

    /// <summary>
    /// Render target file name.
    /// </summary>
    public RenderFileName FileName { get; }

    /// <summary>
    /// Render target language. Can correspond to ILanguage and get UserTagConfig.
    /// </summary>
    public string Language { get; }

    /// <summary>
    /// Specific render location for material.
    /// </summary>
    public string Location { get; }

    /// <summary>
    /// Local non-UTC time.
    /// </summary>
    public DateTime LastUpdateTime { get; private set; }

    #endregion

    #region Settable Properties

    /// <summary>
    /// Whether to suspend, render will not execute when suspended.
    /// </summary>
    public bool Suspended { get; set; }

    /// <summary>
    /// Tag object.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Code library for restore.
    /// </summary>
    public ICodeLibrary RestoreCodeLibrary { get; set; }

    /// <summary>
    /// Whether user code is enabled.
    /// </summary>
    public bool UserCodeEnabled { get; set; } = true;

    /// <summary>
    /// User code count.
    /// </summary>
    public int UserCodeCount { get; set; }

    /// <summary>
    /// Render result.
    /// </summary>
    public RenderResult Result { get; set; }

    #endregion

    #region Abstract & Virtual

    /// <summary>
    /// Executes render.
    /// </summary>
    /// <param name="option">Render parameter option.</param>
    /// <returns>Returns render result.</returns>
    public abstract RenderResult Render(object option);

    /// <summary>
    /// Executes replacement.
    /// </summary>
    /// <param name="segments">Segment collection.</param>
    /// <param name="option">Option.</param>
    public virtual void Inject(IRenderSegmentCollection segments, object option)
    { }

    /// <summary>
    /// Affected ids.
    /// </summary>
    public virtual IEnumerable<Guid> AffectedIds => (Item?.Object as IIdCluster)?.Ids ?? [];

    /// <summary>
    /// File bunch.
    /// </summary>
    public virtual IFileBunch FileBunch => null;

    #endregion

    /// <summary>
    /// Updates the last update time.
    /// </summary>
    /// <param name="time">New time.</param>
    public void UpdateTime(DateTime time)
    {
        if (time > LastUpdateTime)
        {
            LastUpdateTime = time;
        }
    }

    /// <inheritdoc/>
    public override string ToString() => $"[{FileName}:{Item}]";
}

/// <summary>
/// Render target that uses a template to produce code output.
/// </summary>
public class TemplateRenderTarget : RenderTarget
{
    private readonly ICodeTemplate _template;
    private readonly IRenderLanguage _language;

    /// <summary>
    /// Creates a template render target.
    /// </summary>
    /// <param name="renderItem">Render item.</param>
    /// <param name="fileName">File name.</param>
    /// <param name="template">Code template.</param>
    /// <param name="language">Render language.</param>
    /// <param name="material">Material.</param>
    /// <param name="location">Render location.</param>
    public TemplateRenderTarget(RenderItem renderItem, RenderFileName fileName, ICodeTemplate template, IRenderLanguage language, IMaterial material, string location)
        : base(renderItem, material, fileName, language.LanguageName, location)
    {
        _template = template;
        _language = language;
    }

    /// <inheritdoc/>
    public override IEnumerable<Guid> AffectedIds
    {
        get
        {
            foreach (var id in base.AffectedIds)
            {
                yield return id;
            }

            if (!(_template?.Id == Guid.Empty))
            {
                yield return _template.Id;
            }
            if (_language != null)
            {
                yield return _language.Id;
            }
        }
    }

    /// <inheritdoc/>
    public override RenderResult Render(object option)
    {
        if (option is not ExpressionContext context)
        {
            return RenderResult.Empty;
        }

        if (_template != null)
        {
            return _template.RenderText(context, this);
        }
        else
        {
            Logs.LogError($"Template not found : {this.FileName}");
            return RenderHelper.CreateTextRenderResult(RenderStatus.ErrorContinue, string.Empty);
        }
    }
}