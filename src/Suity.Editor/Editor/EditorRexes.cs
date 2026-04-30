using Suity.Editor.Documents;
using Suity.Editor.WorkSpaces;
using Suity.Helpers;
using Suity.Rex.Mapping;
using Suity.Rex.VirtualDom;
using Suity.Synchonizing.Core;
using System;

namespace Suity.Editor;

/// <summary>
/// Provides static Rex properties and actions for editor functionality.
/// </summary>
public static class EditorRexes
{
    public static readonly RexTree Tree = new();
    public static readonly RexMapper Mapper = new(false);

    static EditorRexes()
    {
    }

    #region Environment

    public static readonly RexAction EditorBeforeAwake = new(Tree, nameof(EditorBeforeAwake));
    public static readonly RexAction EditorAwake = new(Tree, nameof(EditorAwake));
    public static readonly RexAction EditorStart = new(Tree, nameof(EditorStart));
    public static readonly RexAction EditorPause = new(Tree, nameof(EditorPause));
    public static readonly RexAction EditorResume = new(Tree, nameof(EditorResume));

    public static readonly RexProperty<bool> IsAppActive = new(Tree, nameof(IsAppActive));
    public static readonly RexAction Restart = new(Tree, nameof(Restart));

    #endregion

    #region Project

    public static readonly RexAction<Project> ProjectOpened = new(Tree, nameof(ProjectOpened));
    public static readonly RexAction<Project> ProjectClosing = new(Tree, nameof(ProjectClosing));

    public static readonly RexPropertyCached<bool> ProjectDirty = new(Tree, nameof(ProjectDirty));

    public static readonly RexAction RefreshProjectView = new(Tree, nameof(RefreshProjectView));

    #endregion

    #region Core UI

    public static readonly RexProperty<bool> FormReady = new(Tree, nameof(FormReady));
    public static readonly RexProperty<bool> DocumentAnalyzeEnabled = new(Tree, nameof(DocumentAnalyzeEnabled));

    public static readonly RexAction UIStarted = new(Tree, nameof(UIStarted));
    public static readonly RexAction<CancellableEvent> UIStopping = new(Tree, nameof(UIStopping));
    public static readonly RexAction UIStopped = new(Tree, nameof(UIStopped));

    public static readonly RexAction RefreshUI = new(Tree, nameof(RefreshUI));
    public static readonly RexAction StopUI = new(Tree, nameof(StopUI));

    #endregion

    #region System

    public static readonly RexAction EnsureInMainThread = new(Tree, nameof(EnsureInMainThread));
    public static readonly RexAction PushQueuedActions = new(Tree, nameof(PushQueuedActions));
    public static readonly RexAction<string, Exception> ShowError = new(Tree, nameof(ShowError));
    public static readonly RexAction<string> SendToRecycleBin = new(Tree, nameof(SendToRecycleBin));
    public static readonly RexAction<object> GotoDefinition = new(Tree, nameof(GotoDefinition));
    public static readonly RexAction<object> FindReference = new(Tree, nameof(FindReference));
    public static readonly RexAction<object> FindImplement = new(Tree, nameof(FindImplement));
    public static readonly RexAction<string, SearchOption> GlobalSearch = new(Tree, nameof(GlobalSearch));
    public static readonly RexAction DebugPrintReferencer = new(Tree, nameof(DebugPrintReferencer));

    public static readonly RexAction HeartBeat = new(Tree, nameof(HeartBeat));

    #endregion

    #region Document

    public static readonly RexAction CloseAllDocuments = new(Tree, nameof(CloseAllDocuments));
    public static readonly RexAction CloseOtherDocuments = new(Tree, nameof(CloseOtherDocuments));
    public static readonly RexProperty<DocumentEntry> ActiveDocument = new(Tree, nameof(ActiveDocument));

    #endregion

    #region Reference

    public static readonly RexProperty<bool> ReferenceManagerDisabled = new(Tree, nameof(ReferenceManagerDisabled));
    public static readonly RexAction RaiseUpdateReference = new(Tree, nameof(RaiseUpdateReference));

    #endregion

    #region Rendering

    public static readonly RexAction<WorkSpaceManager> WriteSolution = new(Tree, nameof(WriteSolution));
    public static readonly RexAction OnRenderStart = new(Tree, nameof(OnRenderStart));

    #endregion

    #region UI

    public static readonly RexAction<string, bool> ShowToolWindow = new(Tree, nameof(ShowToolWindow));

    public static readonly RexAction<string> CloseToolWindow = new(Tree, nameof(CloseToolWindow));

    public static readonly RexAction<string, string> ShowText = new(Tree, nameof(ShowText));

    public static readonly RexAction<string> ShowStatusText = new(Tree, nameof(ShowStatusText));

    public static readonly RexAction ShowProjectSetting = new(Tree, nameof(ShowProjectSetting));

    #endregion

    #region UserInfo

    public static RexProperty<string> UserId = new(Tree, nameof(UserId));
    public static RexProperty<string> UserToken = new(Tree, nameof(UserToken));
    public static RexProperty<string> UserHWID = new(Tree, nameof(UserHWID));

    public static RexAction LicenseTypeChanged = new(Tree, nameof(LicenseTypeChanged));

    #endregion

    #region Package

    public static RexAction<string, string> ImportPackage = new(Tree, nameof(ImportPackage));

    #endregion

    #region Localization

    public static RexProperty<string> Language = new(Tree, nameof(Language));

    #endregion

    #region Logging

    public static readonly RexAction<LogEntry> LogEntryAdded = new(Tree, nameof(LogEntryAdded));

    #endregion
}

/// <summary>
/// Represents a log entry with message and level information.
/// </summary>
public class LogEntry
{
    public LogMessageType LogLevel { get; set; }
    public object Message { get; set; }
    public int Indent { get; set; }
}