using static Suity.Helpers.GlobalLocalizer;
using Suity.Editor.Analyzing;
using Suity.Views.Im;
using System;
using System.Drawing;
using Suity.Editor.Services;
using Suity.Drawing;

namespace Suity.Editor.VirtualTree;

/// <inheritdoc/>
public partial class VirtualNode : IDrawEditorImGui, IDrawContext
{
    /// <inheritdoc/>
    public bool OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        switch (pipeline)
        {
            case EditorImGuiPipeline.Preview when DisplayedValue is ISupportAnalysis analysis && analysis.Analysis is AnalysisResult result:
                OnDrawAnalysisResult(gui, result);

                return true;
        }

        if (DisplayedValue is IDrawEditorImGui draw)
        {
            try
            {
                return draw.OnEditorGui(gui, pipeline, this);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        return false;
    }

    /// <summary>
    /// Draws the analysis result in the ImGui interface. Override to customize rendering.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    /// <param name="analysis">The analysis result to display.</param>
    protected virtual void OnDrawAnalysisResult(ImGui gui, AnalysisResult analysis)
    {
        DrawAnalysisNumberBox(gui, analysis);
        DrawAnalysisPreview(gui, analysis);
    }

    /// <summary>
    /// Draws number boxes showing various analysis metrics.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    /// <param name="analysis">The analysis result containing metrics.</param>
    /// <param name="common">Whether to draw common metrics like reference count.</param>
    /// <param name="error">Whether to draw error metrics like conflicts and problems.</param>
    protected void DrawAnalysisNumberBox(ImGui gui, AnalysisResult analysis, bool common = true, bool error = true)
    {
        var c = EditorServices.ColorConfig;

        if (common)
        {
            if (analysis.ReferenceCount > 0)
            {
                gui.NumberBox("ref", analysis.ReferenceCount.ToString(), c.GetStatusColor(TextStatus.Reference), CoreIconCache.Reference, iconDark: true, tooltips: L("Reference Count"));
            }

            if (analysis.UserCodeCount > 0)
            {
                gui.NumberBox("userCode", analysis.UserCodeCount.ToString(), c.GetStatusColor(TextStatus.UserCode), CoreIconCache.UserCode, tooltips: L("User Code Count"));
            }

            //if (analysis.MemberCount > 0)
            //{
            //    NumberBox(gui, "member", analysis.MemberCount.ToString(), c.GetStatusColor(TextStatus.Disabled), CoreIconCache.List, tooltips: L("Member Count"));
            //}

            if (analysis.ResourceUseCount > 0)
            {
                gui.NumberBox("resUse", analysis.ResourceUseCount.ToString(), c.GetStatusColor(TextStatus.ResourceUse), CoreIconCache.Goal, tooltips: L("Asset Use Count"));
            }

            if (analysis.MultipleFullTypeNameCount > 1)
            {
                gui.NumberBox("nameShare", analysis.MultipleFullTypeNameCount.ToString(), c.GetStatusColor(TextStatus.Info), CoreIconCache.Share, iconDark: true, tooltips: L("Same Name Count"));
            }

            //if (analysis.ExternalDependenices.Count > 0)
            //{
            //    NumberBox(gui, "extDep", analysis.ExternalDependenices.Count, c.GetStatusColor(TextStatus.FileReference), CoreIconCache.External, tooltips: L("External Dependencies"));
            //}
        }

        if (error)
        {
            if (analysis.IdConflictCount > 0)
            {
                gui.NumberBox("idConflict", analysis.IdConflictCount.ToString(), c.GetStatusColor(TextStatus.Warning), CoreIconCache.Warning, tooltips: L("Id Conflict"));
            }

            if (analysis.AssetKeyConflictCount > 0)
            {
                gui.NumberBox("assetKeyConflict", analysis.AssetKeyConflictCount.ToString(), c.GetStatusColor(TextStatus.Warning), CoreIconCache.Warning, tooltips: L("AssetKey Conflict"));
            }

            if (analysis.ReferenceMissingCount > 0)
            {
                gui.NumberBox("refMissing", analysis.AssetKeyConflictCount.ToString(), c.GetStatusColor(TextStatus.Warning), CoreIconCache.Warning, tooltips: L("Reference Missing"));
            }

            if (analysis.ReferenceConflictCount > 0)
            {
                gui.NumberBox("refConflict", analysis.ReferenceConflictCount.ToString(), c.GetStatusColor(TextStatus.Warning), CoreIconCache.Warning, tooltips: L("Reference Conflict"));
            }

            if (analysis.Problems.Count > 0)
            {
                gui.NumberBox("problems", analysis.Problems.Count.ToString(), c.GetStatusColor(TextStatus.Warning), CoreIconCache.Warning, tooltips: L("Problems"));
            }
        }
    }

    /// <summary>
    /// Draws the preview section for analysis results.
    /// </summary>
    /// <param name="gui">The ImGui instance to draw with.</param>
    /// <param name="analysis">The analysis result to display preview for.</param>
    protected void DrawAnalysisPreview(ImGui gui, AnalysisResult analysis)
    {
        var c = EditorServices.ColorConfig;

        if (DisplayedValue is IDrawEditorImGui draw)
        {
            try
            {
                draw.OnEditorGui(gui, EditorImGuiPipeline.Preview, this);
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }
        else
        {
            string p = GetPreviewText();
            Color textColor = EditorServices.ColorConfig.GetStatusColor(this.TextStatus);

            if (!string.IsNullOrWhiteSpace(p))
            {
                var node = gui.HorizontalFrame("preview")
                .InitClass("refBox")
                .InitFit()
                .InitColor(Color.Black)
                .OverrideBorder(1f, ViewColor ?? c.GetStatusColor(this.GetPreviewTextStatus()))
                .InitOverridePadding(2, 2, 5, 5)
                .OnContent(() =>
                {
                    if (GetPreviewIcon() is ImageDef icon)
                    {
                        gui.Image("icon", icon).InitClass("icon");
                    }

                    gui.Text("text", p).InitClass("numBoxText").SetFontColor(textColor);
                });
            }
        }
    }

}