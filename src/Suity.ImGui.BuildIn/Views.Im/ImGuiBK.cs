using Suity;
using Suity.Collections;
using Suity.Helpers;
using Suity.Views.Graphics;
using Suity.Views.Im.PropertyEditing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Suity.Views.Im;

/// <summary>
/// Core ImGui implementation that manages the node tree, input processing, layout, and rendering.
/// </summary>
internal class ImGuiBK : ImGui, IFloatTime
{
    /// <summary>
    /// Whether to log ignored value types in debug mode.
    /// </summary>
    public static bool LogIgnoreValueType = false;

    /// <summary>
    /// Whether to globally enable debug drawing of dirty rectangles.
    /// </summary>
    public static bool GlobalDebugDraw = false;

    private static readonly HashSet<Type> IgnoredValues =
    [
        typeof(GuiColorStyle),
        typeof(GuiSizeStyle),
        typeof(GuiPaddingStyle),
        typeof(GuiFitOrientationStyle),
        typeof(GuiAlignmentStyle),
        typeof(GuiChildSpacingStyle),
        typeof(GuiHeaderStyle),
    ];

    private static readonly Pen DebugDirtyPen = new(Color.Cyan, 4);
    private static readonly Pen DebugDirtyClearPen = new(Color.Black, 4);

    internal static ConcurrentPool<StringBuilder> StringBuilderPool { get; } = new(() => new());

    private readonly Random _rnd = new();

    private readonly ImGuiConfig _config;
    private readonly ImGuiTheme _defaultTheme;
    private ImGuiTheme _theme;

    private readonly Stack<ImGuiNodeBK> _nodeStack = new();
    private readonly ImGuiNodeBK _rootNode;
    private IGraphicContext _context = EmptyGraphicContext.Empty;
    private GuiInputState _inputState = GuiInputState.FullSync;
    private GuiInputState _queuedInputState = GuiInputState.FullSync;
    private bool _debugDrawNow;

    private bool _isInputProcessing;
    private bool _isOutputProcessing;
    private object _processLock = new();

    private readonly Queue<Action> _queuedActions = new();
    private bool _queuedRefresh;
    //TODO: Consider converting to a HashSet collection
    private ImGuiNode? _refreshingNode;
    private RefreshCallerInfo _refreshCallerInfo;
    private readonly List<Action> _tempActions = [];

    private readonly HashSet<ImGuiNodeBK> _timerNodes = [];
    private readonly HashSet<ImGuiNodeBK> _timerRemovingNodes = [];
    private readonly HashSet<ImGuiNodeBK> _mouseInNodes = [];

    private ImGuiNodeBK? _lastNode;
    private ImGuiNodeBK? _hoverNode;
    private ImGuiNodeBK? _focusNode;
    private ImGuiNodeBK? _controllingNode;

    private readonly HashSet<RectangleF> _dirtyRects = [];

    private long _inputVersion;
     
    private readonly DateTime _startTime;
    private float _time;
    private float _deltaTime;
    private float _toolTipDuration = 0.5f;
    private float? _toolTipTime;

    private GuiMouseButtons _lastMouseButton;
    private Point _lastMouseDownLocation;
    private float _lastMouseClickTime;
    private Point _lastMouseClickLocation;

    private bool _isClick;
    private bool _isDoubleClick;
    internal ImGuiStatistic _statistic = new();

    private readonly ValueCollection _values = new();

    private IImGuiGuiBackupState? _restoreState;
    private bool _stateRestored = false;

    private IGraphicInput? _tempInput;

    private bool _debugDraw;

    private readonly Dictionary<ImGuiPath, ImGuiNode> _nodeCache = [];


    static ImGuiBK()
    {
        ImGuiServices.Initialize();
    }

    /// <summary>
    /// Initializes a new ImGui instance with the specified configuration.
    /// </summary>
    /// <param name="config">The configuration for this ImGui instance.</param>
    public ImGuiBK(ImGuiConfig config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));
        _config._inputSystem ??= ImGuiInputSystemBK.Instance;
        _config._layoutSystem ??= ImGuiLayoutSystemBK.Instance;
        _config._fitSystem ??= ImGuiFitSystemBK.Instance;
        _config._renderSystem ??= ImGuiRenderSystemBK.Instance;

        _theme = _defaultTheme = _config.Theme ?? new()
        {
            RenderSystem = config.RenderSystem,
            Colors = _config.ColorConfig,
        };

        _theme.BuildTheme();

        _rootNode = new ImGuiNodeBK(this, "_ROOT");
        _rootNode.SetLayoutFunction(ImGuiLayoutSystem.Vertical);
        _rootNode.IsMouseDragOutSideEvent = true;

        _nodeStack.Push(_rootNode);

        _startTime = DateTime.Now;
    }

    /// <summary>
    /// Initializes a new ImGui instance with a graphic context and configuration.
    /// </summary>
    /// <param name="context">The graphic context for rendering and input.</param>
    /// <param name="config">The configuration for this ImGui instance.</param>
    public ImGuiBK(IGraphicContext context, ImGuiConfig config)
        : this(config)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        SetContext(context);
    }

    #region System

    internal ImGuiConfig Config => _config;

    public override string Name => _config.Name ?? string.Empty;

    // ReSharper disable once MemberCanBePrivate.Global
    public override IGraphicContext Context => _context;

    private void SetContext(IGraphicContext context)
    {
        _context = context;

        var rect = _rootNode.Rect;
        rect.Width = _context.Output.Width;
        rect.Height = _context.Output.Height;
        _rootNode.Rect = rect;
    }

    public override IGraphicInput Input => _tempInput ?? _context.Input;
    public override IGraphicOutput Output => _context.Output;

    public override ImGuiTheme Theme
    {
        get => _theme;
        set => _theme = value ?? _defaultTheme;
    }
    public override ImGuiInputSystem InputSystem => _theme?.InputSystem ?? _config.InputSystem;
    public override ImGuiLayoutSystem LayoutSystem => _theme?.LayoutSystem ?? _config.LayoutSystem;
    public override ImGuiFitSystem FitSystem => _theme?.FitSystem ?? _config.FitSystem;
    public override ImGuiRenderSystem RenderSystem => _theme?.RenderSystem ?? _config.RenderSystem;

    public override T? GetSystem<T>() where T : class
    {
        T? system = _config.GetSystem<T>();
        if (system is not null)
        {
            return system;
        }

        if (typeof(T) == typeof(IImGuiPropertyEditorProvider))
        {
            return (T)(object)PropertyEditorProviderBK.Instance;
        }

        return null;
    }

    public override T GetOrAddSystem<T>() => _config.GetOrAddSystem<T>();

    public override long InputVersion => _inputVersion;

    public override GuiInputState LastInputState => _inputState;

    public override float Time => _time;
    public override float DeltaTime => _deltaTime;

    public override float ToolTipDuration
    {
        get => _toolTipDuration;
        set => _toolTipDuration = value;
    }

    public override void SetCursor(GuiCursorTypes cursor)
    {
        if (_controllingNode is { })
        {
            return;
        }

        _context.SetCursor(cursor);
    }

    public override bool DebugDraw
    {
        get => _debugDraw || GlobalDebugDraw;
        set => _debugDraw = value;
    }

    public override bool IsProcessing => _isInputProcessing;
    public override bool IsClick => _isClick;
    public override bool IsDoubleClick => _isDoubleClick;
    public override Point LastMouseDownLocation => _lastMouseDownLocation;
    public override float LastMouseClickTime => _lastMouseClickTime;
    public override Point LastMouseClickLocation => _lastMouseClickLocation;
    public ImGuiStatistic Statistic => _statistic;

    internal void ReleaseNode(ImGuiNodeBK node)
    {
        _nodeCache.Remove(node.FullPath);
    }

    #endregion

    #region Node

    internal ImGuiNodeBK? EndNode(ImGuiNodeBK node, bool layout = true)
    {
        var currentNode = node.CurrentLayoutNode;
        currentNode?.EndSync(layout);
        OnNodeFinishConfig(node);

        return currentNode;
    }

    protected override void OnNodeBeginConfig(ImGuiNode node)
    {
        if (_restoreState?.BeginRestoreNode(node) == true)
        {
            _stateRestored = true;
        }

        base.OnNodeBeginConfig(node);
    }

    protected override void OnNodeFinishConfig(ImGuiNode node)
    {
        _restoreState?.EndRestoreNode(node);
        base.OnNodeFinishConfig(node);
    }

    /// <summary>
    /// The node currently being populated with content
    /// </summary>
    public override ImGuiNode CurrentNode => _nodeStack.Peek();

    internal ImGuiNodeBK InternalCurrentNode => _nodeStack.Peek();

    public override ImGuiNode? HoverNode => _hoverNode;

    internal GuiInputState SetHoverNode(ImGuiNodeBK? value)
    {
        if (_hoverNode == value)
        {
            return GuiInputState.None;
        }

        GuiInputState state = GuiInputState.None;

        if (_hoverNode is { Parent: { } })
        {
            var s = _hoverNode.InputFunction?.Invoke(GuiPipeline.Main, _hoverNode, CommonGraphicInput.HoverOut, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        _hoverNode = value;

        if (_hoverNode is { Parent: { } })
        {
            var s = _hoverNode.InputFunction?.Invoke(GuiPipeline.Main, _hoverNode, CommonGraphicInput.HoverIn, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        return state;
    }

    public override ImGuiNode? RefreshingNode => _refreshingNode;

    /// <summary>
    /// The control that has focus
    /// </summary>
    public override ImGuiNode? FocusNode => _focusNode;

    internal GuiInputState SetFocusNode(ImGuiNodeBK? value)
    {
        if (_focusNode == value)
        {
            return GuiInputState.None;
        }

        GuiInputState state = GuiInputState.None;

        if (_focusNode is { Parent: { } })
        {
            var s = _focusNode.InputFunction?.Invoke(GuiPipeline.Main, _focusNode, CommonGraphicInput.FocusOut, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        _focusNode = value;

        if (_focusNode is { Parent: { } })
        {
            var s = _focusNode.InputFunction?.Invoke(GuiPipeline.Main, _focusNode, CommonGraphicInput.FocusIn, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        return state;
    }

    public override ImGuiNode? ControllingNode => _controllingNode;

    internal GuiInputState SetControllingNode(ImGuiNodeBK? value)
    {
        if (ReferenceEquals(_controllingNode, value))
        {
            return GuiInputState.None;
        }

        GuiInputState state = GuiInputState.None;

        if (_controllingNode is { Parent: { } })
        {
            var s = _controllingNode.InputFunction?.Invoke(GuiPipeline.Main, _controllingNode, CommonGraphicInput.ControllingIn, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        _controllingNode = value;

        if (_controllingNode is { Parent: { } })
        {
            var s = _controllingNode.InputFunction?.Invoke(GuiPipeline.Main, _controllingNode, CommonGraphicInput.ControllingIn, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref state, s);
        }

        return state;
    }

    public override bool GetIsMouseIn(ImGuiNode node) => _mouseInNodes.Contains(node);

    public override IEnumerable<ImGuiNode> MouseInNodes => _mouseInNodes.Pass();

    public override bool AddTimerNode(ImGuiNode node)
    {
        if (node is not ImGuiNodeBK bNode || bNode.Parent is null)
        {
            return false;
        }

        return _timerNodes.Add(bNode);
    }

    public override bool RemoveTimerNode(ImGuiNode node)
    {
        if (node is not ImGuiNodeBK bNode)
        {
            return false;
        }

        return _timerRemovingNodes.Add(bNode);
    }

    public override ImGuiNode? FindNode(ImGuiPath? path)
    {
        if (path is null)
        {
            return null;
        }

        ImGuiNode? current = _rootNode;

        if (ImGuiPath.IsNullOrEmpty(path))
        {
            return current;
        }

        if (_nodeCache.TryGetValue(path, out var cachedNode))
        {
            if (cachedNode.Parent != null)
            {
                return cachedNode;
            }
            else
            {
                _nodeCache.Remove(path);
            }
        }

        int index = 0;

        while (current != null && index < path.Length)
        {
            current = current.GetChildNode(path.GetStringAt(index)!);
            if (current is null)
            {
                return null;
            }

            index++;
        }

        _nodeCache[path] = current!;

        return current;
    }

    #endregion

    #region Input & Output

    public override void HandleGraphicInput(IGraphicInput input, Action<ImGui> onGui)
    {
        if (_isInputProcessing)
        {
            //throw new InvalidOperationException("ImGui is running.");
            //Debug.WriteLine("Skip event:" + input.EventType);
            return;
        }

        HandleQueuedAction();

        IGraphicInput? current = _tempInput;

        try
        {
            _isInputProcessing = true;
            _tempInput = input;

            HandleGuiInput(input, onGui);

            _statistic.InputFrame++;
        }
        finally
        {
            _tempInput = current;
            _isInputProcessing = false;

            if (_queuedRefresh)
            {
                _queuedRefresh = false;
                _context.RequestRefreshInput(false);
            }
            else if (input.EventType == GuiEventTypes.Refresh)
            {
                // If there are no subsequent queued input requests, clear the refreshing node
                if (_refreshingNode != null)
                {
                    _refreshingNode = null;
                }

                _refreshCallerInfo = default;
            }
        }
    }

    public override void HandleGraphicOutput(IGraphicOutput output)
    {
        if (_isOutputProcessing)
        {
            //throw new InvalidOperationException("ImGui is running.");
            return;
        }

        try
        {
            _isOutputProcessing = true;

            if (_context.RepaintAll)
            {
                RenderFull(output);
            }
            else
            {
                RenderPartial(output);
            }

            _dirtyRects.Clear();
            _theme.ClearDirty();
            _statistic.OutputFrame++;
        }
        finally
        {
            _isOutputProcessing = false;
        }
    }

    public override void QueueAction(Action action)
    {
        if (action is null)
        {
            return;
        }

        _queuedActions.Enqueue(action);
    }

    public override void QueueRefresh([CallerLineNumber] int line = 0, [CallerMemberName] string? member = null, [CallerFilePath] string? path = null)
    {
        if (_nodeStack.Count > 0)
        {
            QueueRefresh(_nodeStack.Peek(), line, member, path);
        }
    }

    internal void QueueRefresh(ImGuiNode refreshingNode, [CallerLineNumber] int line = 0, [CallerMemberName] string? member = null, [CallerFilePath] string? path = null)
    {
        //refreshingNode.MarkRenderDirty();

        _refreshingNode = refreshingNode;
        _queuedRefresh = true;
        _refreshCallerInfo = new RefreshCallerInfo
        {
            Line = line,
            Member = member,
            Path = path,
        };
    }

    public override void QueueInputState(GuiInputState state)
    {
        if (_queuedInputState < state)
        {
            _queuedInputState = state;
        }
    }

    public override void RequestOutput()
    {
        _dirtyRects.AddRange(CollectDirtyRects());
        if (_dirtyRects.Count > 0)
        {
            _context.RequestOutput(_dirtyRects);
        }
    }

    public override void RequestFullOutput()
    {
        _context.RequestOutput();
    }


    private void HandleQueuedAction()
    {
        if (_queuedActions.Count == 0)
        {
            return;
        }

        _tempActions.Clear();
        _tempActions.AddRange(_queuedActions);
        _queuedActions.Clear();

        // ToArray is needed here to prevent collection modification during iteration
        foreach (var action in _tempActions.ToArray())
        {
            try
            {
                action.Invoke();
            }
            catch (Exception err)
            {
                err.LogError();
            }
        }

        _tempActions.Clear();
    }

    #endregion

    #region Gui

    private GuiInputState HandleTimerInput(IGraphicInput input)
    {
        var inputState = GuiInputState.None;

        //TODO: Change to Stopwatch
        float time = (float)(DateTime.Now - _startTime).TotalSeconds;
        _deltaTime = time - _time;
        _time = time;
        if (_toolTipTime.HasValue)
        {
            _toolTipTime = _toolTipTime.Value + _deltaTime;
            if (_toolTipTime >= _toolTipDuration)
            {
                _toolTipTime = null;

                //_rootNode.HandleInput(CommonGraphicInput.ToolTip, out _);
                //if (_hoverNode is { })
                //{
                //    _hoverNode.HandleInput(CommonGraphicInput.ToolTip, out _);
                //}

                CommonGraphicInput.ToolTip.Handled = false;
                foreach (var node in _mouseInNodes)
                {
                    node.InputFunction?.Invoke(GuiPipeline.Main, node, CommonGraphicInput.ToolTip, (_, _) => GuiInputState.None);
                    if (CommonGraphicInput.ToolTip.Handled)
                    {
                        break;
                    }
                }

                //Debug.WriteLine($"ToolTip hover:{_hoverNode?.Id}");
            }
        }

        foreach (var timerNode in _timerNodes)
        {
            if (timerNode.Parent is { })
            {
                var state = timerNode.HandleTimerUpdate(input);
                if (inputState < state)
                {
                    inputState = state;
                }
            }
            else
            {
                _timerRemovingNodes.Add(timerNode);
            }
        }

        if (DebugDraw && _debugDrawFadeOut > 0)
        {
            _debugDrawNow = true;
        }

        return inputState;
    }

    private GuiInputState CheckMouseOut(IGraphicInput input)
    {
        var inputState = GuiInputState.None;

        List<ImGuiNodeBK>? removes = null;

        bool forceMouseOut = input.EventType == GuiEventTypes.MouseOut;

        foreach (var node in _mouseInNodes)
        {
            if (node is { Parent: null })
            {
                (removes ??= []).Add(node);
            }
            else if (forceMouseOut /*Ensure order*/ || node is { IsMouseInClickRect: false })
            {
                (removes ??= []).Add(node);

                var state = node.InputFunction?.Invoke(
                    GuiPipeline.Main, node, CommonGraphicInput.MouseOut, (_, _) => GuiInputState.None) ?? GuiInputState.None;
                MergeState(ref inputState, state);
            }
        }

        if (removes is { })
        {
            foreach (var remove in removes)
            {
                // Make sure mouse not in rect
                remove.UpdateInputVersion(input);
                _mouseInNodes.Remove(remove);
            }
        }

        return inputState;
    }

    private GuiInputState HandleNormalInput(IGraphicInput input)
    {
        var inputState = GuiInputState.None;

        if (_controllingNode is { Parent: { } })
        {
            // Directly executing the input function prevents it from being captured by parent nodes
            var state = _controllingNode.InputFunction?.Invoke(GuiPipeline.Main, _controllingNode, input, (_, _) => GuiInputState.None) ?? GuiInputState.None;
            MergeState(ref inputState, state);
        }

        var rootState = _rootNode.HandleInput(input, out var hoverNode);
        MergeState(ref inputState, rootState);
        if (_rootNode.NeedRender)
        {
            MergeState(ref inputState, GuiInputState.Render);
        }

        if (hoverNode is ImGuiNodeBK bHoverNode && input.EventType != GuiEventTypes.MouseOut)
        {
            var hoverState = SetHoverNode(bHoverNode);
            MergeState(ref inputState, hoverState);

            var current = bHoverNode;

            while (current is { Parent: { } })
            {
                if (current.IsMouseInClickRect && _mouseInNodes.Add(current))
                {
                    var state = current.InputFunction?.Invoke(
                        GuiPipeline.Main, current, CommonGraphicInput.MouseIn, (_, _) => GuiInputState.None) ?? GuiInputState.None;
                    MergeState(ref inputState, state);

                    //Debug.WriteLine($"add mouse-in : {hoverNode.FullName}");
                }

                current = current.InternalParent;
            }
        }
        else
        {
            var hoverState = SetHoverNode(null);
            MergeState(ref inputState, hoverState);
        }

        return inputState;
    }

    private void HandleGuiInput(IGraphicInput input, Action<ImGui> onGui)
    {
        //switch (input.EventType)
        //{
        //    case GuiEventTypes.MouseDown:
        //        return;
        //    case GuiEventTypes.MouseUp:
        //        break;
        //    case GuiEventTypes.MouseClick:
        //        return;
        //    case GuiEventTypes.Refresh:
        //        return;
        //}

        _inputState = GuiInputState.None;
        _debugDrawNow = false;

        _inputVersion++;

        var moveState = CheckMouseOut(input);
        MergeState(ref _inputState, moveState);

        PreInput(input);

        if (_queuedRefresh)
        {
            var tempInput = _tempInput;

            try
            {
                _tempInput = CommonGraphicInput.Refresh;
                PreInput(_tempInput);
            }
            catch (Exception err)
            {
                err.LogError();
            }
            finally
            {
                _tempInput = tempInput;
            }

            // Ignore refresh attempts during GUI process
            _queuedRefresh = false;
        }

        PostInput(onGui);

        if (_timerRemovingNodes.Count > 0)
        {
            foreach (var node in _timerRemovingNodes)
            {
                _timerNodes.Remove(node);
            }

            _timerRemovingNodes.Clear();
        }
    }

    private void PreInput(IGraphicInput input)
    {
        switch (input.EventType)
        {
            case GuiEventTypes.Resize:
                MergeState(ref _inputState, GuiInputState.Layout);
                break;

            case GuiEventTypes.Timer:
                {
                    var state = HandleTimerInput(input);
                    MergeState(ref _inputState, state);
                    _statistic.TimerEventCall++;
                }
                break;

            case GuiEventTypes.MouseDown:
                {
                    if (input.MouseLocation is { } pos)
                    {
                        _lastMouseButton = input.MouseButton;
                        _lastMouseDownLocation = pos;
                        var state = HandleNormalInput(input);
                        MergeState(ref _inputState, state);
                    }
                }
                break;

            case GuiEventTypes.MouseUp:
                {
                    if (input.MouseLocation is { } pos)
                    {
                        if (_lastMouseButton == input.MouseButton)
                        {
                            int offset = Math.Abs(pos.X - LastMouseDownLocation.X) +
                                Math.Abs(pos.Y - LastMouseDownLocation.Y);

                            _isClick = offset < 10;
                        }
                        else
                        {
                            _isClick = false;
                        }

                        if (input.MouseButton == GuiMouseButtons.Left)
                        {
                            _isDoubleClick = IsClick && input.MouseLocation == LastMouseClickLocation && _time - LastMouseClickTime <= DoubleClickDuration;

                            _lastMouseClickTime = _time;
                            _lastMouseClickLocation = pos;
                        }

                        var state = HandleNormalInput(input);
                        MergeState(ref _inputState, state);
                    }
                }
                break;

            case GuiEventTypes.MouseMove:
                {
                    _toolTipTime = 0;
                    var state = HandleNormalInput(input);
                    MergeState(ref _inputState, state);
                }
                break;

            case GuiEventTypes.Refresh:
                //TODO: Not ideal for targeted refresh, unable to handle specific node refreshes
                MergeState(ref _inputState, GuiInputState.FullSync);
                _statistic.RefreshEventCall++;
                break;

            default:
                {
                    var state = HandleNormalInput(input);
                    MergeState(ref _inputState, state);
                }
                break;
        }
    }

    private void PostInput(Action<ImGui> onGui)
    {
        if (_queuedInputState != GuiInputState.None)
        {
            MergeState(ref _inputState, _queuedInputState);
            _queuedInputState = GuiInputState.None;
        }

        switch (_inputState)
        {
            case GuiInputState.FullSync:
            case GuiInputState.PartialSync:
                {
                    var rect = new Rectangle(0, 0, _context.Output.Width, _context.Output.Height);
                    HandleGuiSync(rect, onGui, false);
                    RequestOutput();

                    break;
                }
            case GuiInputState.Layout:
                {
                    var rect = new Rectangle(0, 0, _context.Output.Width, _context.Output.Height);
                    //TODO: Implement pure Layout without sync
                    //HandleLayoutOnly(rect);
                    HandleGuiSync(rect, onGui, true);
                    RequestOutput();

                    break;
                }
            case GuiInputState.Render:
                RequestOutput();

                break;

            default:
                if (_debugDrawNow && _lastDirtyRects?.Length > 0)
                {
                    _dirtyRects.Clear();
                    _context.RequestOutput(_lastDirtyRects);
                }
                break;
        }
    }

    private bool _firstGuiSync = true;

    private void HandleGuiSync(Rectangle rect, Action<ImGui> onGui, bool layoutOnly)
    {
        if (_firstGuiSync)
        {
            _firstGuiSync = false;
            layoutOnly = false;
        }

        _rootNode.Rect = rect;

        while (_nodeStack.Count > 1)
        {
            var node = _nodeStack.Pop();
            EndNode(node);
            node.InternalEndContent();
        }

        _nodeStack.Peek().InternalBeginContent();

        try
        {
            if (layoutOnly)
            {
                _nodeStack.Peek().LayoutContentsDeep();
            }
            else
            {
                onGui(this);
            }
        }
        finally
        {
            while (_nodeStack.Count > 1)
            {
                var node = _nodeStack.Pop();
                EndNode(node);
                node.InternalEndContent();
            }

            EndNode(_nodeStack.Peek());
            _nodeStack.Peek().InternalEndContent();

            if (_restoreState != null && _stateRestored)
            {
                _restoreState = null;
            }
        }

        _lastNode = null;
    }

    private void HandleLayoutOnly(Rectangle rect)
    {
        _rootNode.Rect = rect;

        while (_nodeStack.Count > 1)
        {
            var node = _nodeStack.Pop();
            EndNode(node);
            node.InternalEndContent();
        }

        _nodeStack.Peek().InternalBeginContent();

        _nodeStack.Peek().LayoutContentsDeep();

        while (_nodeStack.Count > 1)
        {
            var node = _nodeStack.Pop();
            EndNode(node);
            node.InternalEndContent();
        }

        EndNode(_nodeStack.Peek());
        _nodeStack.Peek().InternalEndContent();

        _lastNode = null;
    }

    private void RenderFull(IGraphicOutput output)
    {
        if (BackgroundColor is { } color)
        {
            output.Clear(color);
        }
        
        _rootNode.HandleRender(GuiPipeline.Main, output, _rootNode.Rect, false, true);
    }

    private void RenderPartial(IGraphicOutput output)
    {
        if (_dirtyRects.Count == 0)
        {
            if (DebugDraw)
            {
                DoDebugDraw(output);
            }
            return;
        }

        //if (BackgroundColor is { } color)
        //{
        //    output.Clear(color);
        //}

        _rootNode.HandleRender(GuiPipeline.Main, output, _rootNode.Rect, true, true);

        if (DebugDraw)
        {
            ClearDebugDraw(output);

            _lastDirtyRects = [.. _dirtyRects];
            _debugDrawFadeOut = 1;
            DoDebugDraw(output);
        }
    }

    private readonly HashSet<ImGuiNode> _tempNodes = [];

    private RectangleF[] CollectDirtyRects()
    {
        bool useDirtyRect = _inputState == GuiInputState.FullSync;

        _tempNodes.Clear();

        // Collect all dirty nodes
        foreach (var node in _rootNode.InternalChildNodes)
        {
            node.CollectRenderDirtyNodes(_tempNodes);
        }

        //foreach (var node in _tempNodes)
        //{
        //    var r = node.DirtyRect.HasValue ? (object)node.DirtyRect.Value : null;
        //    Debug.WriteLine($"dirty:{node.FullName} {r}");
        //}

        RectangleF[] rects;

        rects = _tempNodes.Where(o => o.Parent is { })
            .Select(o => o.GlobalDirtyRect)
            .Where(o => o.HasValue)
            .Select(o => o!.Value)
            .ToArray();

        _tempNodes.Clear();

        return rects;
    }

    private void ClearDebugDraw(IGraphicOutput output)
    {
        if (_lastDirtyRects?.Length > 0)
        {
            foreach (var rect in _lastDirtyRects)
            {
                output.DrawRectangle(DebugDirtyClearPen, rect);
            }
        }
    }

    private void DoDebugDraw(IGraphicOutput output)
    {
        if (_debugDrawFadeOut > 0)
        {
            _debugDrawFadeOut -= 0.05f;
            if (_debugDrawFadeOut < 0)
            {
                _debugDrawFadeOut = 0;
            }
        }

        if (_debugDrawFadeOut > 0 && _lastDirtyRects?.Length > 0)
        {
            //var color = ColorHelper.Multiply(DebugDirtyPen.Color, _debugDrawFadeOut);
            var color = Color.FromArgb(_rnd.Range(0, 255), _rnd.Range(0, 255), _rnd.Range(0, 255));

            var pen = new Pen(color, DebugDirtyPen.Width);

            foreach (var rect in _lastDirtyRects)
            {
                output.DrawRectangle(pen, rect);
            }
        }
    }

    private RectangleF[]? _lastDirtyRects;
    private float _debugDrawFadeOut;

    #endregion

    #region Sync & Content

    public override ImGuiNode BeginCurrentNode(string id)
    {
        var node = _nodeStack.Peek().BeginNode(id);
        OnNodeBeginConfig(node);

        return node;
    }

    public override ImGuiNode? PassCurrentNode(string? id)
    {
        if (id is null)
        {
            return null;
        }

        var currentNode = _nodeStack.Peek();
        return currentNode.PassNode(id);
    }

    public override ImGuiNode? EndCurrentNode()
    {
        if (_nodeStack.Peek() is { } node)
        {
            return EndNode(node);
        }
        else
        {
            return null;
        }
    }

    public override void PassCurrentContents()
    {
        var node = _nodeStack.Peek();
        node.PassContents();
    }

    public override void LayoutCurrentContents(bool fit, bool align)
    {
        var node = _nodeStack.Peek();
        node.Layout();
        node.LayoutContentsDeep(fit, align);
    }

    public override ImGuiNode? OnContent(Action contentAction, bool layout)
    {
        if (_lastNode is { })
        {
            var node = _lastNode;

            BeginContent(node);

            try
            {
                contentAction();
            }
            catch (Exception err)
            {
                Logs.LogError(err);
                throw;
            }
            finally
            {
                EndContent(node);
            }

            if (layout)
            {
                node.Layout();
            }

            return node;
        }
        else
        {
            return null;
        }
    }

    public override void OnContent(ImGuiNode node, Action contentAction, bool layout)
    {
        BeginContent(node);

#if DEBUG
        contentAction();
        EndContent(node);
#else
        try
        {
            contentAction();
        }
        catch (Exception err)
        {
            Logs.LogError(err);
            throw;
        }
        finally
        {
            EndContent(node);
        }
#endif

        if (layout)
        {
            node.Layout();
        }
    }

    public override void OnContent(ImGuiNode node, Action<ImGuiNode> contentAction, bool layout)
    {
        BeginContent(node);

#if DEBUG
        contentAction(node);
        EndContent(node);
#else
        try
        {
            contentAction(node);
        }
        catch (Exception err)
        {
            Logs.LogError(err);
            throw;
        }
        finally
        {
            EndContent(node);
        }
#endif

        if (layout)
        {
            node.Layout();
        }
    }

    public override ImGuiNode? BeginContent()
    {
        if (_lastNode is { })
        {
            var node = _lastNode;

            BeginContent(node);

            return node;
        }
        else
        {
            return null;
        }
    }

    public override void BeginContent(ImGuiNode node)
    {
        if (node is ImGuiNodeBK bNode)
        {
            bNode.InternalBeginContent();

            if (_nodeStack.Peek() != node)
            {
                _nodeStack.Push(bNode);
            }
        }
    }

    public override ImGuiNode? EndContent()
    {
        if (_nodeStack.Count > 1)
        {
            EndNode(_nodeStack.Peek());
            var node = _nodeStack.Pop();
            node.InternalEndContent();

            return node;
        }
        else
        {
            return null;
        }
    }

    private void EndContent(ImGuiNode node)
    {
        if (node is ImGuiNodeBK bNode && _nodeStack.Count > 1 && _nodeStack.Peek() == bNode)
        {
            EndNode(_nodeStack.Peek());
            _nodeStack.Pop();
            bNode.InternalEndContent();
        }
    }

    #endregion

    #region Layout Node

    internal ImGuiNodeBK? LastNode
    {
        get => _lastNode;
        set => _lastNode = value;
    }

    public override void LayoutNodeContent(ImGuiNode node)
    {
        if (node is ImGuiNodeBK vNode && vNode.Gui == this)
        {
            try
            {
                _nodeStack.Push(vNode);
            }
            finally
            {
                vNode.LayoutContentsDeep();
                _nodeStack.Pop();
            }
        }
    }

    #endregion

    #region Style

    public override void SetCurrentTheme(ImGuiTheme theme)
    {
        InternalCurrentNode.Theme = theme;
    }

    public override void SetCurrentStyle<T>(string name, T style) where T : class
    {
        InternalCurrentNode.I_EnsureMyStyle().SetStyle(name, style);
    }

    public override void SetCurrentStyle<T>(string name, string pseudo, T style) where T : class
    {
        InternalCurrentNode.I_EnsureMyStyle().SetPseudo(name, pseudo, style);
    }

    public override void SetCurrentTransition(string name, string? state, string? targetState, ITransitionFactory transition)
    {
        InternalCurrentNode.I_EnsureMyStyle().SetTransition(name, state, targetState, transition);
    }

    #endregion

    #region Value

    public override T? GetValue<T>() where T : class => _values.GetValue<T>();

    public override object? GetValue(Type type) => _values.GetValue(type);

    public override T GetOrCreateValue<T>() where T : class
    {
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue<T>(out bool created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(Func<T> creation) where T : class
    {
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue(creation, out bool created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(out bool created) where T : class
    {
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue<T>(out created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override T GetOrCreateValue<T>(Func<T> creation, out bool created) where T : class
    {
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        T value = _values.GetOrCreateValue(creation, out created);
        if (created)
        {
            MarkRenderDirty();
        }

        return value;
    }

    public override bool SetValue<T>(T value) where T : class
    {
#if DEBUG
        if (LogIgnoreValueType && IgnoredValues.Contains(typeof(T)))
        {
            Debug.WriteLine($"Value type is ignored : {typeof(T).Name}");
        }
#endif

        _values.SetValue(value, out bool valueSet);
        if (valueSet)
        {
            MarkRenderDirty();
            return true;
        }

        return false;
    }

    public override bool RemoveValue<T>() where T : class
    {
        bool removed = _values.RemoveValue<T>();
        if (removed)
        {
            MarkRenderDirty();
            return true;
        }

        return false;
    }

    private void MarkRenderDirty()
    {
        // Do nothing.
    }

    #endregion

    #region State

    public override void BackupState(IImGuiGuiBackupState state)
    {
        VisitNodeDeep(_rootNode, state.BackupNode);
    }

    public override void RestoreState(IImGuiGuiBackupState state)
    {
        _restoreState = state;
    }

    public override void RestoreState(ImGuiNode node)
    {
        if (_restoreState?.BeginRestoreNode(node) == true)
        {
            _stateRestored = true;
        }
    }

    private void VisitNodeDeep(ImGuiNode node, Action<ImGuiNode> action)
    {
        foreach (var childNode in node.ChildNodes)
        {
            action(childNode);
            VisitNodeDeep(childNode, action);
        }
    }

    #endregion
}