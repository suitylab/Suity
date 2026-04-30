using Suity.Editor.AIGC.Tools;
using System.Threading.Tasks;

namespace Suity.Editor.AIGC.Assistants
{
    /// <summary>
    /// Assistant responsible for resuming a previously paused or interrupted AI execution.
    /// </summary>
    [DisplayText("Resume Execution Assistant")]
    [ToolTipsText("Assistant to resume the current operation.")]
    public class ResumeAssistant : ToolingAssistant, IRootUpdaterAssistant
    {
        /// <inheritdoc/>
        public Task<AICallResult> HandleRootUpdate(AIRequest request)
        {
            var canvas = this.Context;

            return AIAssistantServiceBK.Instance.HandleResume(request, canvas);
        }
    }
}
