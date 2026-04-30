using Suity.Editor.Analyzing;
using Suity.Editor.Design;
using Suity.Editor.Selecting;
using Suity.Editor.Services;
using System;

namespace Suity.Editor.Analysis;

/// <summary>
/// Collects analysis problems for <see cref="EditorObjectSelection{TObject}"/> instances,
/// validating that the selection has a valid target reference and key.
/// </summary>
/// <typeparam name="TObject">The type of object being selected. Must be a reference type.</typeparam>
[InternalPriority]
public class EditorObjectSelectionCollector<TObject> : ProblemCollector<EditorObjectSelection<TObject>>
    where TObject : class
{
    /// <inheritdoc/>
    public override void CollectProblem(EditorObjectSelection<TObject> target, AnalysisProblem problems, AnalysisIntents intent)
    {
        if (intent != AnalysisIntents.Normal)
        {
            return;
        }

        do
        {
            if (target._ref.Id == Guid.Empty && string.IsNullOrEmpty(target._key))
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Target not selected"));
                break;
            }

            if (target._ref.Target == null)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Target is missing"));
                break;
            }

            if (string.IsNullOrEmpty(target.ResolveKey(target._ref.Id)))
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Target is not a selectable member"));
                break;
            }

            if (!target.IsValid)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Invalid target: {target._ref}"));
            }
        } while (false);
    }
}

/// <summary>
/// Collects analysis problems for <see cref="MemberSelection{TContainer, TMember}"/> instances,
/// validating the container getter and member ownership.
/// </summary>
/// <typeparam name="TContainer">The type of the member container. Must implement <see cref="IMemberContainer"/>.</typeparam>
/// <typeparam name="TMember">The type of the member. Must implement <see cref="IMember"/>.</typeparam>
public class MemberSelectionCollector<TContainer, TMember> : ProblemCollector<MemberSelection<TContainer, TMember>>
    where TContainer : class, IMemberContainer
    where TMember : class, IMember
{
    /// <inheritdoc/>
    public override void CollectProblem(MemberSelection<TContainer, TMember> target, AnalysisProblem problems, AnalysisIntents intent)
    {
        Base.CollectProblem(target, problems, intent);

        if (!target.ContainerGetterDefined)
        {
            problems.Add(new AnalysisProblem(TextStatus.Warning, "Container getter not set"));
        }

        TContainer container = target.GetContainer();
        TMember member = target.Target;
        if (container != null && member != null)
        {
            if (container.GetMember(member.Name) != member)
            {
                problems.Add(new AnalysisProblem(TextStatus.Error, $"Target is missing"));
            }
        }
    }
}
