using System;

namespace Suity.Editor.Services;

/// <summary>
/// Defines the available license types for the editor.
/// </summary>
public enum LicenseTypes
{
    /// <summary>
    /// Community license type.
    /// </summary>
    [DisplayText("Community")]
    Community,

    /// <summary>
    /// Starter license type.
    /// </summary>
    [DisplayText("Starter")]
    Starter,

    /// <summary>
    /// Professional license type.
    /// </summary>
    [DisplayText("Professional")]
    Professional,

    /// <summary>
    /// Enterprise license type.
    /// </summary>
    [DisplayText("Enterprise")]
    Enterprise,
}

/// <summary>
/// Defines the available editor features that can be licensed.
/// </summary>
public enum EditorFeatures
{
    [DisplayCategory("Data Modeling")]
    [DisplayText("Data Design")]
    [ToolTipsText("Supports data structure design and editing.")]
    DataDesign,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Data Flow")]
    [ToolTipsText("Supports flowchart-style data editing and usage.")]
    DataFlow,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Value Flow")]
    [ToolTipsText("Supports value formula flowchart editing and usage.")]
    ValueFlow,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Value Condition")]
    [ToolTipsText("Supports generating different data under different configuration conditions.")]
    ValueCondition,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Export")]
    [ToolTipsText("Supports exporting data or files")]
    Export,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Export Library")]
    [ToolTipsText("Supports exporting files via read-only library.")]
    ExportLibrary,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Trigger")]
    [ToolTipsText("Supports trigger document editing and usage.")]
    Trigger,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Reference Analysis")]
    [ToolTipsText("Supports real-time reference analysis and metrics.")]
    ReferenceAnalysis,

    [DisplayCategory("Data Modeling")]
    [DisplayText("Kanban")]
    [ToolTipsText("Supports quick navigation kanban usage.")]
    Kanban,

    [DisplayCategory("Development")]
    [DisplayText("Canvas")]
    [ToolTipsText("Canvas system.")]
    Canvas,

    [DisplayCategory("Code Generation")]
    [DisplayText("Code Generation")]
    [ToolTipsText("Supports generating target code from data structures.")]
    CodeGenerate,

    [DisplayCategory("Code Generation")]
    [DisplayText("Render Flow")]
    [ToolTipsText("Supports code generation flow usage.")]
    RenderFlow,

    [DisplayCategory("Code Generation")]
    [DisplayText("Advanced Render Nodes")]
    [ToolTipsText("Supports using advanced code generation nodes in code generation flow.")]
    AdvancedRenderNodes,

    [DisplayCategory("Development")]
    [DisplayText("Application Development")]
    [ToolTipsText("Supports application development.")]
    GalaxyDevelopment,

    [DisplayCategory("Development")]
    [DisplayText("Publish")]
    [ToolTipsText("Supports application publishing.")]
    Publish,

    [DisplayCategory("Development")]
    [DisplayText("Local Debug")]
    [ToolTipsText("Supports local debugging of applications.")]
    LocalDebug,

    [DisplayCategory("Development")]
    [DisplayText("Remote Deployment")]
    [ToolTipsText("Supports remote deployment of applications.")]
    RemoteDeployment,

    [DisplayCategory("Development")]
    [DisplayText("Remote Conversation")]
    [ToolTipsText("Supports remote interactive dialog usage.")]
    RemoteConversation,

    [DisplayCategory("AIGC")]
    [DisplayText("AIGC Workflow")]
    [ToolTipsText("Supports AIGC workflow usage.")]
    AigcWorkflow,

    [DisplayCategory("AIGC")]
    [DisplayText("Planning")]
    [ToolTipsText("Supports planning documents and planning workflow nodes usage.")]
    AigcPlanning,

    [DisplayCategory("AIGC")]
    [DisplayText("Third-Party AIGC Model")]
    [ToolTipsText("Supports using self-applied third-party AIGC accounts as models.")]
    AigcThirdPartiModel,

    [DisplayCategory("Extensions")]
    [DisplayText("Custom Plugin")]
    [ToolTipsText("Supports loading plugins generated via application development.")]
    CustomPlugin,

    [DisplayCategory("Extensions")]
    [DisplayText("Third-Party Plugin")]
    [ToolTipsText("Supports loading third-party official plugins.")]
    ThirdPartyPlugin,
}

/// <summary>
/// Attribute that marks a type or member with a specific editor feature requirement.
/// </summary>
[AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = true)]
public sealed class EditorFeatureAttribute : Attribute
{
    /// <summary>
    /// Gets the editor feature associated with this attribute.
    /// </summary>
    public EditorFeatures Feature { get; }

    /// <summary>
    /// Initializes a new instance of the EditorFeatureAttribute class.
    /// </summary>
    /// <param name="feature">The editor feature to associate with this attribute.</param>
    public EditorFeatureAttribute(EditorFeatures feature)
    {
        Feature = feature;
    }
}

/// <summary>
/// Exception thrown when an editor feature is not available or licensed.
/// </summary>
[Serializable]
public class EditorFeatureException : Exception
{
    /// <summary>
    /// Gets the editor feature that caused the exception.
    /// </summary>
    public EditorFeatures Feature { get; }

    /// <summary>
    /// Initializes a new instance of the EditorFeatureException class.
    /// </summary>
    /// <param name="feature">The editor feature that caused the exception.</param>
    public EditorFeatureException(EditorFeatures feature)
        => Feature = feature;

    /// <summary>
    /// Initializes a new instance of the EditorFeatureException class with a message.
    /// </summary>
    /// <param name="feature">The editor feature that caused the exception.</param>
    /// <param name="message">The error message.</param>
    public EditorFeatureException(EditorFeatures feature, string message)
        : base(message)
        => Feature = feature;

    /// <summary>
    /// Initializes a new instance of the EditorFeatureException class with a message and inner exception.
    /// </summary>
    /// <param name="feature">The editor feature that caused the exception.</param>
    /// <param name="message">The error message.</param>
    /// <param name="inner">The inner exception.</param>
    public EditorFeatureException(EditorFeatures feature, string message, Exception inner)
        : base(message, inner)
        => Feature = feature;

    /// <summary>
    /// Initializes a new instance of the EditorFeatureException class with serialization info.
    /// </summary>
    /// <param name="capability">The editor feature that caused the exception.</param>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected EditorFeatureException(
        EditorFeatures capability,
      System.Runtime.Serialization.SerializationInfo info,
      System.Runtime.Serialization.StreamingContext context)
        : base(info, context)
        => Feature = capability;
}

/// <summary>
/// License service
/// </summary>
public abstract class LicenseService
{
    private static LicenseService _current;

    /// <summary>
    /// Gets the current license service instance.
    /// </summary>
    public static LicenseService Current
    {
        get
        {
            if (_current != null)
            {
                return _current;
            }

            _current = Device.Current.GetService<LicenseService>();
            return _current ?? EmptyLicenseService.Empty;
        }
        internal set
        {
            _current = value;
        }
    }

    /// <summary>
    /// Gets the user ID associated with the license.
    /// </summary>
    public abstract string UserId { get; }

    /// <summary>
    /// Gets the product name.
    /// </summary>
    public abstract string ProductName { get; }

    /// <summary>
    /// Gets the product version.
    /// </summary>
    public abstract string ProductVersion { get; }

    /// <summary>
    /// Gets the type of license.
    /// </summary>
    public abstract LicenseTypes LicenseType { get; }

    /// <summary>
    /// Checks if the user is logged in.
    /// </summary>
    /// <returns>True if logged in; otherwise, false.</returns>
    public abstract bool CheckLogin();

    /// <summary>
    /// Checks if a specific editor feature is available.
    /// </summary>
    /// <param name="feature">The feature to check.</param>
    /// <returns>True if the feature is available; otherwise, false.</returns>
    public abstract bool GetFeature(EditorFeatures feature);

    /// <summary>
    /// Checks if a specific editor feature is available by name.
    /// </summary>
    /// <param name="name">The feature name to check.</param>
    /// <returns>True if the feature is available; otherwise, false.</returns>
    public abstract bool GetFeatureEx(string name);

    /// <summary>
    /// Checks if the maximum usage limit has been reached.
    /// </summary>
    /// <returns>True if the maximum usage has been reached; otherwise, false.</returns>
    public abstract bool GetMaxUsageReach();

    /// <summary>
    /// Gets the failure message for a specific feature.
    /// </summary>
    /// <param name="capability">The feature to get the message for.</param>
    /// <returns>The failure message.</returns>
    public abstract string GetFailedMessage(EditorFeatures capability);

    /// <summary>
    /// Gets the usage failure message.
    /// </summary>
    /// <returns>The failure message.</returns>
    public abstract string GetUsageFailedMessage();

    /// <summary>
    /// Gets the limited entry count.
    /// </summary>
    public abstract int LimitedEntryCount { get; }

    /// <summary>
    /// Gets the maximum diagram count.
    /// </summary>
    public abstract int MaxDiagramCount { get; }

    /// <summary>
    /// Gets the maximum node count.
    /// </summary>
    public abstract int MaxNodeCount { get; }

    /// <summary>
    /// Gets the editor point count.
    /// </summary>
    public abstract int EditorPoint { get; }

    /// <summary>
    /// Gets the AIGC point count.
    /// </summary>
    public abstract int AigcPoint { get; }
}

/// <summary>
/// Empty implementation of the license service that provides unrestricted access.
/// </summary>
public sealed class EmptyLicenseService : LicenseService
{
    /// <summary>
    /// Gets the singleton instance of the EmptyLicenseService.
    /// </summary>
    public static readonly EmptyLicenseService Empty = new();

    private EmptyLicenseService()
    { }

    /// <inheritdoc/>
    public override string ProductName => string.Empty;

    /// <inheritdoc/>
    public override string ProductVersion => string.Empty;

    /// <inheritdoc/>
    public override string UserId => string.Empty;

    /// <inheritdoc/>
    public override LicenseTypes LicenseType => LicenseTypes.Community;

    /// <inheritdoc/>
    public override bool CheckLogin() => true;

    /// <inheritdoc/>
    public override bool GetFeature(EditorFeatures capability) => true;

    /// <inheritdoc/>
    public override bool GetFeatureEx(string name) => true;

    /// <inheritdoc/>
    public override bool GetMaxUsageReach() => true;

    /// <inheritdoc/>
    public override string GetFailedMessage(EditorFeatures capability) => string.Empty;

    /// <inheritdoc/>
    public override string GetUsageFailedMessage() => string.Empty;

    /// <inheritdoc/>
    public override int LimitedEntryCount => int.MaxValue;

    /// <inheritdoc/>
    public override int MaxDiagramCount => int.MaxValue;

    /// <inheritdoc/>
    public override int MaxNodeCount => int.MaxValue;

    /// <inheritdoc/>
    public override int EditorPoint => int.MaxValue;

    /// <inheritdoc/>
    public override int AigcPoint => int.MaxValue;
}