using ComputerBeacon.Json;
using Suity.Editor;
using Suity.Editor.Documents;
using Suity.Editor.Services;
using Suity.Editor.Values;
using Suity.Helpers;
using Suity.Networking;
using Suity.NodeQuery;
using Suity.Synchonizing;
using Suity.Synchonizing.Preset;
using Suity.Views;
using Suity.Views.Im;
using Suity.Views.PathTree;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Suity.Editor.View.ViewModel;

#region EntityExInfo

/// <summary>
/// Contains extended network information such as session and channel identifiers.
/// </summary>
public class NetworkExInfo
{
    /// <summary>
    /// Gets or sets the session identifier.
    /// </summary>
    public string SessionId { get; set; }

    /// <summary>
    /// Gets or sets the channel identifier.
    /// </summary>
    public string ChannelId { get; set; }
}

/// <summary>
/// Contains extended entity information such as room, entity identifiers, name, and image.
/// </summary>
public class EntityExInfo
{
    /// <summary>
    /// Gets or sets the room identifier.
    /// </summary>
    public long RoomId { get; set; }

    /// <summary>
    /// Gets or sets the entity identifier.
    /// </summary>
    public long EntityId { get; set; }

    /// <summary>
    /// Gets or sets the entity name.
    /// </summary>
    public string EntityName { get; set; }

    /// <summary>
    /// Gets or sets the entity image.
    /// </summary>
    public Image EntityImageEx { get; set; }
}

#endregion

#region LogNode

/// <summary>
/// Represents a node in the log path tree that displays log messages with various formatting and population capabilities.
/// </summary>
public class LogNode : PopulatePathNode
{
    /// <summary>
    /// The maximum string length before text is trimmed.
    /// </summary>
    public const int MaxStringLength = 300;

    /// <summary>
    /// The default row height for log nodes.
    /// </summary>
    public const int DefaultRowHeight = 25;


    private static readonly char[] _splitChars = ['\n', '\r', '\0'];

    private readonly string _originText;
    private readonly string _trimmedText;

    private readonly string[] _lines;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogNode"/> class with default values.
    /// </summary>
    protected LogNode()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogNode"/> class with the specified text.
    /// </summary>
    /// <param name="text">The log message text.</param>
    protected LogNode(string text)
    {
        _originText = _trimmedText = text ?? string.Empty;

        if (_trimmedText.IndexOfAny(_splitChars) >= 0)
        {
            _lines = text.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries);
            if (_lines.Length == 1)
            {
                _trimmedText = _lines[0];
                _lines = null;
            }
        }

        if (_lines != null)
        {
            Height = 16 * _lines.Length + 6;
        }
        else
        {
            if (_trimmedText.Length > MaxStringLength)
            {
                _trimmedText = _trimmedText.Substring(0, MaxStringLength) + "...";
            }
        }
    }

    /// <summary>
    /// Gets the original, unmodified text of the log message.
    /// </summary>
    public string OriginText => _originText;

    /// <summary>
    /// Gets or sets the log message level (debug, info, warning, error).
    /// </summary>
    public LogMessageType LogLevel { get; set; }

    /// <summary>
    /// Gets or sets the extended network information associated with this log entry.
    /// </summary>
    public NetworkExInfo NetworkInfo { get; set; }

    /// <summary>
    /// Gets or sets the extended entity information associated with this log entry.
    /// </summary>
    public EntityExInfo EntityInfo { get; set; }

    /// <summary>
    /// Gets or sets the property name associated with this log entry.
    /// </summary>
    public string PropertyName { get; set; }

    /// <summary>
    /// Gets or sets a custom tag object associated with this log entry.
    /// </summary>
    public object Tag { get; set; }

    /// <summary>
    /// Gets or sets the height of this log node in the tree view.
    /// </summary>
    public int Height { get; set; } = DefaultRowHeight;

    /// <summary>
    /// Gets the array of text lines if the log message contains multiple lines.
    /// </summary>
    public string[] Lines => _lines;

    /// <summary>
    /// Gets or sets the indentation level for this log node.
    /// </summary>
    public int Indent { get; set; }

    /// <summary>
    /// Gets the object that can be inspected when this node is selected.
    /// </summary>
    /// <returns>The inspectable object, or null if none is available.</returns>
    public virtual object GetInspectableObject() => null;

    /// <summary>
    /// Gets the display text for this log node.
    /// </summary>
    protected override string OnGetText()
    {
        return _trimmedText ?? string.Empty;
    }

    /// <summary>
    /// Returns a string representation of this log node.
    /// </summary>
    public override string ToString()
    {
        return OnGetText();
    }

    #region Static

    /// <summary>
    /// Creates a new <see cref="LogNode"/> based on the type of the message object.
    /// </summary>
    /// <param name="logLevel">The log message level.</param>
    /// <param name="message">The message object to create a node from.</param>
    /// <param name="indent">The indentation level for the node.</param>
    /// <returns>A new <see cref="LogNode"/> instance appropriate for the message type.</returns>
    public static LogNode Create(LogMessageType logLevel, object message, int indent = 0)
    {
        switch (message)
        {
            case string str:
                return new LogNodeTextNode(str ?? string.Empty, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Tag = message,
                    Indent = indent,
                };

            case Exception err:
                return new LogExceptionNode(err, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Tag = err,
                    Indent = indent,
                };

            case ExceptionLogItem errLogItem:
                try
                {
                    return new LogExceptionNode(errLogItem.Exception, errLogItem.Message, status: GetLogTextStatus(logLevel))
                    {
                        LogLevel = logLevel,
                        Tag = errLogItem.Exception,
                        Indent = indent,
                    };
                }
                catch (Exception)
                {
                    // The passed Exception does not support Serializable
                    return new LogNodeTextNode(errLogItem.Message, status: TextStatus.Error)
                    {
                        LogLevel = logLevel,
                        Tag = null,
                        Indent = indent,
                    };
                }

            case ActionLogItem actionLogItem:
                return new LogNodeTextNode(actionLogItem.Message, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Tag = actionLogItem.Action,
                    Indent = indent,
                };

            case ObjectLogCoreItem objLogItem:
                return new LogNodeTextNode(objLogItem.Message, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Tag = objLogItem.Target,
                    Indent = indent,
                };

            case INodeReader reader:
                return new ReaderInfoPathNode(reader, GetLogIcon(logLevel), status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Indent = indent,
                };

            case ISyncObject syncObj:
                return new LogSyncObjectNode(syncObj, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Indent = indent,
                };

            case ISyncList syncList:
                return new LogSyncListNode(syncList, status: GetLogTextStatus(logLevel))
                {
                    LogLevel = logLevel,
                    Indent = indent,
                };

            case JsonObject jObj:
                return new LogJsonNode(null, jObj, GetLogIcon(logLevel))
                {
                    LogLevel = logLevel,
                    Indent = indent,
                };

            case JsonArray jAry:
                return new LogJsonNode(null, jAry, GetLogIcon(logLevel))
                {
                    LogLevel = logLevel,
                    Indent = indent,
                };

            default:
                if (message != null)
                {
                    return new LogObjectNode(message, GetLogTextStatus(logLevel))
                    {
                        LogLevel = logLevel,
                        Indent = indent,
                    };
                }
                else
                {
                    return new LogNodeTextNode(string.Empty, status: GetLogTextStatus(logLevel))
                    {
                        LogLevel = logLevel,
                        Indent = indent,
                    };
                }
        }
    }

    /// <summary>
    /// Gets the icon image for the specified network direction.
    /// </summary>
    /// <param name="dir">The network direction.</param>
    /// <returns>The icon image for the direction, or null if none.</returns>
    public static Image GetLogIcon(NetworkDirection dir) => dir switch
    {
        NetworkDirection.None => null,
        NetworkDirection.Upload => CoreIconCache.Upload,
        NetworkDirection.Download => CoreIconCache.Download,
        _ => null,
    };

    /// <summary>
    /// Gets the icon image for the specified log message type.
    /// </summary>
    /// <param name="type">The log message type.</param>
    /// <returns>The icon image for the log level.</returns>
    public static Image GetLogIcon(LogMessageType type) => type switch
    {
        LogMessageType.Debug => CoreIconCache.LogDebug,
        LogMessageType.Info => CoreIconCache.LogInfo,
        LogMessageType.Warning => CoreIconCache.LogWarning,
        LogMessageType.Error => CoreIconCache.LogError,
        _ => CoreIconCache.LogInfo,
    };

    /// <summary>
    /// Gets the icon image for the specified log message type and network direction.
    /// </summary>
    /// <param name="type">The log message type.</param>
    /// <param name="direction">The network direction.</param>
    /// <returns>The icon image for the log level and direction.</returns>
    public static Image GetLogIcon(LogMessageType type, NetworkDirection direction) => type switch
    {
        LogMessageType.Debug => GetLogIcon(direction) ?? CoreIconCache.LogDebug,
        LogMessageType.Info => GetLogIcon(direction) ?? CoreIconCache.LogInfo,
        LogMessageType.Warning => CoreIconCache.LogWarning,
        LogMessageType.Error => CoreIconCache.LogError,
        _ => CoreIconCache.LogInfo,
    };

    /// <summary>
    /// Gets the text color status for the specified log message type.
    /// </summary>
    /// <param name="type">The log message type.</param>
    /// <returns>The text status corresponding to the log level.</returns>
    public static TextStatus GetLogTextStatus(LogMessageType type) => type switch
    {
        LogMessageType.Debug => TextStatus.Normal,
        LogMessageType.Info => TextStatus.Info,
        LogMessageType.Warning => TextStatus.Warning,
        LogMessageType.Error => TextStatus.Error,
        _ => TextStatus.Normal,
    };

    /// <summary>
    /// Gets the icon image for the specified entity action type.
    /// </summary>
    /// <param name="actionType">The entity action type.</param>
    /// <returns>The icon image for the action type, or null if none.</returns>
    public static Image GetLogIcon(EntityActionTypes actionType) => actionType switch
    {
        EntityActionTypes.CreateEntity or EntityActionTypes.DestroyEntity => CoreIconCache.Entity,
        EntityActionTypes.AddOrReplaceValue or EntityActionTypes.RemoveValue => CoreIconCache.Component,
        EntityActionTypes.AddedToLogic or EntityActionTypes.RemovedFromLogic => CoreIconCache.LogicModule,
        _ => null,
    };

    /// <summary>
    /// Gets the extended icon image for the specified entity action type (add or disable indicator).
    /// </summary>
    /// <param name="actionType">The entity action type.</param>
    /// <returns>The icon image for the action type, or null if none.</returns>
    public static Image GetLogIconEx(EntityActionTypes actionType) => actionType switch
    {
        EntityActionTypes.CreateEntity or EntityActionTypes.AddOrReplaceValue or EntityActionTypes.AddedToLogic => CoreIconCache.Add,
        EntityActionTypes.DestroyEntity or EntityActionTypes.RemoveValue or EntityActionTypes.RemovedFromLogic => CoreIconCache.Disable,
        _ => null,
    };

    #endregion
}

#endregion

#region LogNodeTextNode

/// <summary>
/// Represents a log node that displays plain text with an optional image and text color status.
/// </summary>
public class LogNodeTextNode(string text, Image image = null, TextStatus status = TextStatus.Normal) : LogNode(text)
{
    private readonly Image _image = image;
    private readonly TextStatus _textStatus = status;

    /// <summary>
    /// Gets a value indicating whether the user can drag this node.
    /// </summary>
    public override bool CanUserDrag => false;

    /// <summary>
    /// Gets the image displayed alongside the text.
    /// </summary>
    public override Image Image => _image?.ToIconSmall();

    /// <summary>
    /// Gets the text color status used for rendering the text.
    /// </summary>
    public override TextStatus TextColorStatus => _textStatus;

    /// <summary>
    /// Returns a string representation of this text node.
    /// </summary>
    public override string ToString()
    {
        return OnGetText() ?? string.Empty;
    }
}

#endregion

#region LogObjectNode

/// <summary>
/// Represents a log node that displays an arbitrary object, resolving it to a displayable form such as a document, asset, or text display.
/// </summary>
public class LogObjectNode : LogNode
{
    private readonly object _obj;
    private readonly string _text;
    private readonly Image _image;
    private readonly TextStatus _textStatus = TextStatus.Normal;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogObjectNode"/> class for the specified object.
    /// </summary>
    /// <param name="obj">The object to display.</param>
    /// <param name="textStatus">The text color status for rendering.</param>
    public LogObjectNode(object obj, TextStatus textStatus)
        : base()
    {
        _textStatus = textStatus;

        _obj = obj;

        object nav = obj;

        if (obj is INavigable n)
        {
            nav = n.GetNavigationTarget() ?? obj;
        }

        if (nav is StorageLocation loc)
        {
            do
            {
                var docAsset = FileAssetManager.Current.GetAsset(loc.FullPath);
                if (docAsset != null)
                {
                    nav = docAsset;
                    break;
                }

                var docTest = DocumentManager.Instance.GetDocument(loc);
                if (docTest != null)
                {
                    nav = docTest.Content;
                    break;
                }
            } while (false);
        }

        switch (nav)
        {
            case Document doc:
                _text = doc.ToString();
                _image = doc.Icon;
                _textStatus = TextStatus.Normal;
                break;

            case Asset asset:
                _text = asset.DisplayText ?? asset.Name;
                _image = asset.Icon;
                _textStatus = TextStatus.Normal;
                break;

            case ITextDisplay d:
                _textStatus = d.DisplayStatus;
                _text = d.DisplayText;
                break;

            default:
                _text = nav?.ToString();
                _textStatus = TextStatus.Normal;
                break;
        }

        this.Tag = nav;
    }

    /// <summary>
    /// Gets a value indicating whether the user can drag this node.
    /// </summary>
    public override bool CanUserDrag => false;

    /// <summary>
    /// Gets the display text for this object node.
    /// </summary>
    protected override string OnGetText() => _text;

    /// <summary>
    /// Gets the image displayed alongside the text.
    /// </summary>
    public override Image Image => _image?.ToIconSmall();

    /// <summary>
    /// Gets the text color status used for rendering the text.
    /// </summary>
    public override TextStatus TextColorStatus => _textStatus;

    /// <summary>
    /// Returns a string representation of this object node.
    /// </summary>
    public override string ToString()
    {
        return OnGetText() ?? string.Empty;
    }
}

#endregion

#region DoubleClickStringNode

/// <summary>
/// Represents a text log node that shows a detail dialog when double-clicked.
/// </summary>
public class DoubleClickTextNode : LogNodeTextNode, IViewDoubleClickAction
{
    private readonly string _detail;

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleClickTextNode"/> class.
    /// </summary>
    /// <param name="text">The display text.</param>
    /// <param name="detail">The detailed text shown on double-click.</param>
    /// <param name="image">The optional image to display.</param>
    /// <param name="status">The text color status.</param>
    public DoubleClickTextNode(string text, string detail, Image image = null, TextStatus status = TextStatus.Normal)
        : base(text, image, status)
    {
        _detail = detail;
    }

    /// <summary>
    /// Handles the double-click action by showing a dialog with the detail text.
    /// </summary>
    public void DoubleClick()
    {
        DialogUtility.ShowTextBlockDialogAsync(Text, _detail, null);
    }
}

#endregion

#region LogJsonNode

/// <summary>
/// Represents a log node that displays JSON objects or arrays with expandable child nodes.
/// </summary>
public class LogJsonNode : LogNode
{
    private readonly string _name;
    private readonly object _obj;
    private readonly Image _image;
    private readonly string _typeName;

    private SItem _sitem;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogJsonNode"/> class.
    /// </summary>
    /// <param name="name">The name of the JSON property or null for root.</param>
    /// <param name="obj">The JSON object or array to display.</param>
    /// <param name="image">The icon image to display.</param>
    public LogJsonNode(string name, object obj, Image image)
    {
        _name = name;
        _obj = obj;
        _image = image;

        if (obj is JsonObject jsonObj)
        {
            _typeName = jsonObj["@type"] as string;
        }
    }

    /// <summary>
    /// Gets the underlying JSON value (object, array, or primitive).
    /// </summary>
    public object Value => _obj;

    /// <summary>
    /// Gets the type name extracted from the JSON object, if available.
    /// </summary>
    public override string TypeName => _typeName;

    /// <summary>
    /// Determines whether this node can be populated with child nodes.
    /// </summary>
    protected override bool CanPopulate()
    {
        if (_obj is JsonArray)
        {
            return true;
        }
        else if (_obj is JsonObject jsonObj)
        {
            return jsonObj.Any(o => !o.Key.StartsWith("@"));
        }

        return false;
    }

    /// <summary>
    /// Populates the child nodes for this JSON node.
    /// </summary>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        if (_obj is JsonObject obj)
        {
            foreach (var p in obj)
            {
                if (!p.Key.StartsWith("@"))
                {
                    yield return CreateJsonNode(p.Key, p.Value, null);
                }
            }
        }
        else if (_obj is JsonArray ary)
        {
            int num = 0;
            foreach (var p in ary)
            {
                yield return CreateJsonNode($"[{num}]", p, null);
                num++;
            }
        }
    }

    /// <summary>
    /// Gets the display text for this JSON node.
    /// </summary>
    protected override string OnGetText()
    {
        if (_obj is JsonObject obj)
        {
            if (obj["@text"] is string text)
            {
                return text;
            }

            string typeName = obj["@type"] as string;
            if (typeName != null)
            {
                var content = AssetManager.Instance.GetAsset(typeName, AssetFilters.Default);
                if (content != null)
                {
                    typeName = content.DisplayText;
                }
            }

            if (!string.IsNullOrEmpty(typeName))
            {
                Guid id = GlobalIdResolver.Resolve(typeName);
                if (id != Guid.Empty)
                {
                    typeName = EditorObjectManager.Instance.GetObject(id)?.Name ?? typeName;
                }
            }
            else
            {
                typeName = "...";
            }

            if (!string.IsNullOrEmpty(_name))
            {
                return $"\"{_name}\" : {{{typeName}}}";
            }
            else
            {
                return $"{{{typeName}}}";
            }
        }
        else if (_obj is JsonArray)
        {
            if (!string.IsNullOrEmpty(_name))
            {
                return $"\"{_name}\" : [{(_obj as JsonArray).Count} items]";
            }
            else
            {
                return $"({(_obj as JsonArray).Count} items)";
            }
        }
        else
        {
            string str;
            if (_obj is null)
            {
                str = "null";
            }
            else if (_obj is string)
            {
                str = $"\"{_obj}\"";
            }
            else
            {
                str = _obj.ToString();
            }

            if (!string.IsNullOrEmpty(_name))
            {
                return $"\"{_name}\" : {str}";
            }
            else
            {
                return str;
            }
        }
    }

    /// <summary>
    /// Gets the text color status based on the "@status" field in the JSON object, if present.
    /// </summary>
    public override TextStatus TextColorStatus
    {
        get
        {
            if (_obj is JsonObject obj)
            {
                if (obj["@status"] is string status)
                {
                    return status.ToLower() switch
                    {
                        "info" => TextStatus.Info,
                        "warning" => TextStatus.Warning,
                        "error" => TextStatus.Error,
                        "comment" => TextStatus.Comment,
                        "anonymous" => TextStatus.Anonymous,
                        "disabled" => TextStatus.Disabled,
                        _ => TextStatus.Normal,
                    };
                }
            }

            return TextStatus.Normal;
        }
    }

    /// <summary>
    /// Gets the icon image for this JSON node based on its value type.
    /// </summary>
    public override Image Image
    {
        get
        {
            if (_image != null)
            {
                return _image.ToIconSmall();
            }

            if (_name?.StartsWith("@") == true)
            {
                return null;
            }

            if (_obj is JsonObject)
            {
                return CoreIconCache.Object.ToIconSmall();
            }
            else if (_obj is JsonArray)
            {
                return CoreIconCache.Array.ToIconSmall();
            }
            else
            {
                return CoreIconCache.Value.ToIconSmall();
            }
        }
    }

    /// <summary>
    /// Gets an inspectable object representation of the JSON value.
    /// </summary>
    public override object GetInspectableObject()
    {
        _sitem ??= EditorServices.JsonResource.FromJson(_obj);

        return _sitem;
    }

    /// <summary>
    /// Creates a child JSON node for the specified name and value.
    /// </summary>
    /// <param name="name">The property name or array index.</param>
    /// <param name="value">The JSON value.</param>
    /// <param name="image">The optional icon image.</param>
    /// <returns>A new <see cref="LogJsonNode"/> or <see cref="DoubleClickJsonNode"/> for the value.</returns>
    public LogJsonNode CreateJsonNode(string name, object value, Image image)
    {
        if (value is JsonObject obj)
        {
            if (obj["@content"] is string content)
            {
                return new DoubleClickJsonNode(name, value, image, content)
                {
                    Height = 16
                }
                .WithNodePath(this, name);
            }
        }

        return new LogJsonNode(name, value, image)
        {
            Height = 16
        }
        .WithNodePath(this, name);
    }
}

#endregion

#region DoubleClickJsonNode

/// <summary>
/// Represents a JSON log node that shows a content detail dialog when double-clicked.
/// </summary>
public class DoubleClickJsonNode : LogJsonNode, IViewDoubleClickAction
{
    private readonly string _content;

    /// <summary>
    /// Initializes a new instance of the <see cref="DoubleClickJsonNode"/> class.
    /// </summary>
    /// <param name="name">The property name.</param>
    /// <param name="value">The JSON value.</param>
    /// <param name="image">The icon image.</param>
    /// <param name="content">The detailed content shown on double-click.</param>
    public DoubleClickJsonNode(string name, object value, Image image, string content)
        : base(name, value, image)
    {
        _content = content ?? string.Empty;
    }

    /// <summary>
    /// Handles the double-click action by showing a dialog with the content text.
    /// </summary>
    public void DoubleClick()
    {
        DialogUtility.ShowTextBlockDialogAsync(Text, _content, null);
    }
}

#endregion

#region LogExceptionNode

/// <summary>
/// Represents a log node that displays exception information with expandable details including type, message, stack trace, and inner exceptions.
/// </summary>
public class LogExceptionNode : LogNode
{
    private readonly Exception _exception;
    private readonly TextStatus _status;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionNode"/> class using the exception's message as display text.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    /// <param name="status">The text color status. Defaults to Error.</param>
    public LogExceptionNode(Exception exception, TextStatus status = TextStatus.Error)
        : this(exception, exception.Message, status)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogExceptionNode"/> class with a custom message.
    /// </summary>
    /// <param name="exception">The exception to display.</param>
    /// <param name="message">The custom display message.</param>
    /// <param name="status">The text color status. Defaults to Error.</param>
    public LogExceptionNode(Exception exception, string message, TextStatus status = TextStatus.Error)
        : base(message)
    {
        _exception = exception ?? throw new ArgumentNullException(nameof(exception));
        _status = status;
    }

    /// <summary>
    /// Gets the text color status for this exception node.
    /// </summary>
    public override TextStatus TextColorStatus => _status;

    /// <summary>
    /// Gets the error icon image for this exception node.
    /// </summary>
    public override Image Image => CoreIconCache.LogError.ToIconSmall();

    /// <summary>
    /// Gets a value indicating whether this node can be populated with child details.
    /// </summary>
    protected override bool CanPopulate() => true;

    /// <summary>
    /// Populates the child nodes with exception details including type, message, stack trace, and inner exception.
    /// </summary>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        yield return new TextNode("Type : " + _exception.GetType().FullName, null, TextStatus.Error)
            .WithNodePath(this, "Type");
        yield return new TextNode("Message : " + _exception.Message, null, TextStatus.Error)
            .WithNodePath(this, "Message");
        yield return new DoubleClickTextNode("StackTrace [...]", _exception.StackTrace, CoreIconCache.Stack, TextStatus.Error)
            .WithNodePath(this, "StackTrace");

        if (_exception.InnerException != null)
        {
            yield return new LogExceptionNode(_exception.InnerException)
                .WithNodePath(this, "Inner");
        }
    }
}

#endregion

#region LogResourceNode

/// <summary>
/// Represents a log node that displays a resource by key or ID, with navigation to its definition on double-click.
/// </summary>
public class LogResourceNode : LogNode, IViewDoubleClickAction
{
    /// <summary>
    /// Gets the resource key.
    /// </summary>
    public string Key { get; }

    /// <summary>
    /// Gets the resolved resource ID.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets or sets the count associated with this resource.
    /// </summary>
    public int Count { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="LogResourceNode"/> class for the specified resource key.
    /// </summary>
    /// <param name="key">The resource key to resolve.</param>
    public LogResourceNode(string key)
    {
        Key = key;

        if (GlobalIdResolver.TryResolve(key, out Guid id))
        {
            Id = id;
        }
    }

    /// <summary>
    /// Gets the display text, resolved from the editor object or falling back to the key or ID.
    /// </summary>
    protected override string OnGetText()
    {
        EditorObject obj = EditorObjectManager.Instance.GetObject(Id);
        return obj?.FullName ?? Key ?? Id.ToString();
    }

    /// <summary>
    /// Gets the icon image from the resolved asset, if available.
    /// </summary>
    public override Image Image
    {
        get
        {
            Asset asset = AssetManager.Instance.GetAsset(Id);
            return asset?.Icon;
        }
    }

    /// <summary>
    /// Gets the text color status based on whether the resource was resolved.
    /// </summary>
    public override TextStatus TextColorStatus
    {
        get
        {
            EditorObject obj = EditorObjectManager.Instance.GetObject(Id);
            return obj != null ? TextStatus.Normal : TextStatus.Error;
        }
    }

    /// <summary>
    /// Handles the double-click action by navigating to the resource definition.
    /// </summary>
    public void DoubleClick()
    {
        EditorUtility.GotoDefinition(Id);
    }
}

#endregion

#region LogSyncObjectNode

/// <summary>
/// Represents a log node that displays a synchronizable object with expandable property children.
/// </summary>
public class LogSyncObjectNode : LogObjectNode
{
    private readonly ISyncObject _obj;
    private readonly GetAllPropertySync _props;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSyncObjectNode"/> class.
    /// </summary>
    /// <param name="obj">The synchronizable object to display.</param>
    /// <param name="status">The text color status. Defaults to Normal.</param>
    public LogSyncObjectNode(ISyncObject obj, TextStatus status = TextStatus.Normal)
        : base(obj, status)
    {
        _obj = obj;
        _props = new GetAllPropertySync(SyncIntent.Serialize, false);
        _obj.Sync(_props, EmptySyncContext.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether this node can be populated with property children.
    /// </summary>
    protected override bool CanPopulate()
    {
        return _props.Values.Count > 0;
    }

    /// <summary>
    /// Populates the child nodes with the synchronized object's properties.
    /// </summary>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        foreach (var pair in _props.Values)
        {
            var node = Create(this.LogLevel, pair.Value.Value, this.Indent).WithNodePath(this, pair.Key);
            node.PropertyName = pair.Key;
            yield return node;
        }
    }
}

#endregion

#region LogSyncListNode

/// <summary>
/// Represents a log node that displays a synchronizable list with expandable indexed children and ImGui rendering support.
/// </summary>
public class LogSyncListNode : LogObjectNode, IDrawEditorImGui
{
    private readonly ISyncList _list;
    private readonly GetAllIndexSync _props;

    /// <summary>
    /// Initializes a new instance of the <see cref="LogSyncListNode"/> class.
    /// </summary>
    /// <param name="list">The synchronizable list to display.</param>
    /// <param name="status">The text color status. Defaults to Normal.</param>
    public LogSyncListNode(ISyncList list, TextStatus status = TextStatus.Normal)
        : base(list, status)
    {
        _list = list ?? throw new ArgumentNullException(nameof(list));
        _props = new GetAllIndexSync(SyncIntent.Serialize);
        _list.Sync(_props, EmptySyncContext.Empty);
    }

    /// <summary>
    /// Gets a value indicating whether this node can be populated with indexed children.
    /// </summary>
    protected override bool CanPopulate()
    {
        return _props.Values.Count > 0;
    }

    /// <summary>
    /// Populates the child nodes with the synchronized list's indexed items.
    /// </summary>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        for (int i = 0; i < _props.Values.Count; i++)
        {
            var info = _props.Values[i];
            string name = $"[{i}]";
            var node = Create(this.LogLevel, info.Value, this.Indent).WithNodePath(this, name);
            //node.PropertyName = name;

            yield return node;
        }
    }

    /// <summary>
    /// Handles ImGui rendering for the list node, displaying text description and item count.
    /// </summary>
    bool IDrawEditorImGui.OnEditorGui(ImGui gui, EditorImGuiPipeline pipeline, IDrawContext context)
    {
        switch (pipeline)
        {
            case EditorImGuiPipeline.Prefix:
                break;

            case EditorImGuiPipeline.Name:
                break;

            case EditorImGuiPipeline.Description:
                gui.Text("##title_text", Text)
                .SetFontColor(Color)
                .InitFullHeight()
                .InitVerticalAlignment(GuiAlignment.Center);
                return true;

            case EditorImGuiPipeline.Preview:
                gui.Frame("frame_version")
                .InitClass("numBox").OnContent(() =>
                {
                    gui.Text("##count", _list.Count.ToString())
                    .InitClass("numBoxText");
                });

                return true;

            default:
                break;
        }

        return false;
    }
}

#endregion

#region ReaderInfoPathNode

/// <summary>
/// Represents a log node that displays information from an <see cref="INodeReader"/> with expandable child nodes.
/// </summary>
public class ReaderInfoPathNode : LogNode
{

    private readonly INodeReader _reader;
    private readonly string _text;
    private readonly string _preview;
    private readonly string _info;
    private readonly TextStatus _textStatus;
    private readonly Image _image;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReaderInfoPathNode"/> class.
    /// </summary>
    /// <param name="reader">The node reader to display.</param>
    /// <param name="image">The optional icon image.</param>
    /// <param name="status">The optional text color status.</param>
    public ReaderInfoPathNode(INodeReader reader, Image image = null, TextStatus? status = null)
    {
        _reader = reader ?? throw new ArgumentNullException(nameof(reader));

        _text = reader.GetAttribute("text");
        _preview = reader.GetAttribute("preview");
        _info = reader.GetAttribute("info");

        var maxLen = MaxStringLength;

        if (_text != null && _text.Length > maxLen)
        {
            _text = _text[..maxLen] + "...";
        }

        if (_preview != null && _preview.Length > maxLen)
        {
            _preview = _preview[..maxLen] + "...";
        }

        if (_info != null && _info.Length > maxLen)
        {
            _info = _text[..maxLen] + "...";
        }

        if (image != null)
        {
            _image = image;
        }
        else
        {
            string iconKey = reader.GetAttribute("icon");
            _image = EditorUtility.GetIcon(iconKey);
        }

        if (status.HasValue)
        {
            _textStatus = status.Value;
        }
        else
        {
            string statusKey = reader.GetAttribute("status");
            if (!string.IsNullOrEmpty(statusKey))
            {
                Enum.TryParse<TextStatus>(statusKey, out _textStatus);
            }
        }
    }

    /// <summary>
    /// Gets the display text for this reader node.
    /// </summary>
    protected override string OnGetText() => _text;

    /// <summary>
    /// Gets the text color status for this reader node.
    /// </summary>
    public override TextStatus TextColorStatus => _textStatus;

    /// <summary>
    /// Gets the preview text from the reader's attributes.
    /// </summary>
    public string Preview => _preview;

    /// <summary>
    /// Gets the info text from the reader's attributes.
    /// </summary>
    public string Info => _info;

    /// <summary>
    /// Gets the icon image for this reader node.
    /// </summary>
    public override Image Image => _image?.ToIconSmall();

    /// <summary>
    /// Gets a value indicating whether this node can be populated with child readers.
    /// </summary>
    protected override bool CanPopulate()
    {
        return _reader.ChildCount > 0;
    }

    /// <summary>
    /// Populates the child nodes from the reader's child nodes.
    /// </summary>
    protected override IEnumerable<PathNode> OnPopulate()
    {
        foreach (var childReader in _reader.Nodes())
        {
            yield return new ReaderInfoPathNode(childReader);
        }
    }
}

#endregion