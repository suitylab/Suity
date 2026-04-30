using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Design;
using Suity.Editor.Types;
using Suity.Synchonizing;
using Suity.Views;
using System;

namespace Suity.Editor.AIGC.Assistants;

#region System Attributes

/// <summary>
/// Specifies which object type an AI assistant can be used for.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = true)]
public sealed class AIAssistantUsageAttribute : Attribute
{
    /// <summary>
    /// Gets the object type this assistant is applicable to.
    /// </summary>
    public Type ObjectType { get; }

    /// <summary>
    /// Initializes a new instance with the specified object type.
    /// </summary>
    /// <param name="objectType">The type of object this assistant handles.</param>
    public AIAssistantUsageAttribute(Type objectType)
    {
        ObjectType = objectType;
    }
}

/// <summary>
/// Marks an AI assistant as the default assistant for its target object type.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public sealed class DefaultAIAssistantUsageAttribute : Attribute
{
}

#endregion

#region Design Attributes

/// <summary>
/// Design attribute that associates an AI assistant with a design element.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "Assistant", Description = "Assistant", Icon = "*CoreIcon|Assistant")]
public class AssistantAttribute : DesignAttribute
{
    /// <summary>
    /// Gets or sets the name of the associated AI assistant.
    /// </summary>
    public string Assistant { get; set; }

    /// <summary>
    /// Synchronizes the assistant property with the given sync context.
    /// </summary>
    /// <param name="sync">The property synchronizer.</param>
    /// <param name="context">The synchronization context.</param>
    protected override void OnSync(IPropertySync sync, ISyncContext context)
    {
        base.OnSync(sync, context);

        Assistant = sync.Sync(nameof(Assistant), Assistant);
    }

    /// <summary>
    /// Sets up the view for the assistant property in the inspector.
    /// </summary>
    /// <param name="setup">The view object setup interface.</param>
    protected override void OnSetupView(IViewObjectSetup setup)
    {
        setup.InspectorField(Assistant, new ViewProperty(nameof(Assistant), "Assistant"));
    }

    /// <summary>
    /// Returns a string representation of this attribute.
    /// </summary>
    /// <returns>A string containing the assistant name.</returns>
    public override string ToString() => L("Assistant") + ": " + Assistant;
}

/// <summary>
/// Attribute that marks a field or type to be excluded from AI generation.
/// </summary>
[NativeType(CodeBase = "*AIGC", Name = "SkipAIGeneration", Description = "Skip AI Generation", Icon = "*CoreIcon|Prompt")]
public class SkipAIGenerationAttribute : DesignAttribute
{
    /// <summary>
    /// Returns a string representation of this attribute.
    /// </summary>
    /// <returns>A string indicating "Skip AI Generation".</returns>
    public override string ToString() => L("Skip AI Generation");
}

#endregion
