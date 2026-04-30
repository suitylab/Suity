using Suity.Synchonizing.Preset;
using System;
using System.Collections.Generic;

namespace Suity.Synchonizing.Core;

public delegate bool VisitorPredicate<T>(T obj, PathContext path);

public delegate void VisitorAction<T>(T obj, PathContext path);

public static class Visitor
{
    public static void Visit(object obj, VisitFlag flag = VisitFlag.None)
    {
        if (obj is null)
        {
            return;
        }

        Visit(obj, (o, p) => true, SyncContext.Empty, new PathContext(), flag);
    }

    public static void Visit(object obj, VisitorAction<object> action, VisitFlag flag = VisitFlag.None)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (obj is null)
        {
            return;
        }

        Visit(obj, (o, p) =>
        {
            action(o, p);
            return true;
        }, SyncContext.Empty, new PathContext(), flag );
    }

    public static void Visit(object obj, VisitorPredicate<object> action, VisitFlag flag = VisitFlag.None)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (obj is null)
        {
            return;
        }

        Visit(obj, action, SyncContext.Empty, new PathContext(), flag);
    }

    public static void Visit<T>(object obj, VisitorAction<T> action, VisitFlag flag = VisitFlag.None)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (obj is null)
        {
            return;
        }

        Visit(obj, (o, p) =>
        {
            if (o is T t)
            {
                action(t, p);
            }
            // Indicate to continue deep search
            return true;
        }, SyncContext.Empty, new PathContext(), flag);
    }

    public static void Visit<T>(object obj, VisitorPredicate<T> action, VisitFlag flag = VisitFlag.None)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        if (obj is null)
        {
            return;
        }

        Visit(obj, (o, p) =>
        {
            if (o is T t)
            {
                // Determine whether to continue deep search by function
                return action(t, p);
            }
            else
            {
                // Indicate to continue deep search
                return true;
            }
        }, SyncContext.Empty, new PathContext(), flag);
    }

    public static void Visit<T1, T2>(object obj, VisitorAction<T1> action1, VisitorAction<T2> action2, VisitFlag flag = VisitFlag.None)
    {
        if (action1 is null)
        {
            throw new ArgumentNullException(nameof(action1));
        }

        if (action2 is null)
        {
            throw new ArgumentNullException(nameof(action2));
        }

        if (obj is null)
        {
            return;
        }

        Visit(obj, (o, p) =>
        {
            if (o is T1 t1)
            {
                action1(t1, p);
            }

            if (o is T2 t2)
            {
                action2(t2, p);
            }

            // Indicate to continue deep search
            return true;
        }, SyncContext.Empty, new PathContext(), flag);
    }

    public static List<object> Find(object obj, VisitorPredicate<object> predicate, VisitFlag flag = VisitFlag.None)
    {
        List<object> list = [];
        Visit(obj, (o, p) =>
        {
            if (predicate(o, p))
            {
                list.Add(o);
            }

            return true;
        });

        return list;
    }

    public static bool TryGetValueDeep(object obj, SyncPath path, out object result)
    {
        if (SyncPath.IsNullOrEmpty(path))
        {
            result = obj;

            return true;
        }

        result = obj;
        if (result is null)
        {
            return false;
        }

        for (int i = 0; i < path.Length; i++)
        {
            switch (path[i])
            {
                case string name:
                    {
                        if (result is ISyncObject rObj)
                        {
                            result = rObj.GetProperty(name);
                        }
                        else
                        {
                            result = null;

                            return false;
                        }
                    }
                    break;

                case int index:
                    {
                        if (result is ISyncList rList)
                        {
                            result = rList.GetItem(index);
                        }
                        else if (result is ISyncNode rNode)
                        {
                            result = rNode.GetList().GetItem(index);
                        }
                        else
                        {
                            result = null;

                            return false;
                        }
                    }
                    break;

                case Guid id:
                    {
                        ISyncPathIdObject context = null;

                        if (result is ISyncObject rObj)
                        {
                            VisitSyncObjectOnce(rObj, (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            result = context;
                        }
                        else if (result is ISyncList rList)
                        {
                            VisitSyncListOnce(rList, (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            result = context;
                        }
                        else if (result is ISyncNode rNode)
                        {
                            VisitSyncListOnce(rNode.GetList(), (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            result = context;
                        }
                        else
                        {
                            result = null;

                            return false;
                        }
                    }
                    break;

                default:
                    result = null;

                    return false;
            }
        }

        return true;
    }

    public static bool TryGetValueDeep<T>(object obj, SyncPath path, out T result, out SyncPath rest)
        where T : class
    {
        if (SyncPath.IsNullOrEmpty(path))
        {
            result = obj as T;
            rest = SyncPath.Empty;

            return obj != null;
        }

        if (obj is T tObj)
        {
            result = tObj;
            rest = path;

            return true;
        }

        var v = obj;
        if (v is null)
        {
            result = null;
            rest = path;

            return false;
        }

        for (int i = 0; i < path.Length; i++)
        {
            switch (path[i])
            {
                case string name:
                    {
                        if (v is ISyncObject rObj)
                        {
                            v = rObj.GetProperty(name);
                        }
                        else
                        {
                            v = null;
                            result = null;
                            rest = path.SubPath(i, path.Length - i);

                            return false;
                        }
                    }
                    break;

                case int index:
                    {
                        if (v is ISyncList rList)
                        {
                            v = rList.GetItem(index);
                        }
                        else if (v is ISyncNode rNode)
                        {
                            v = rNode.GetList().GetItem(index);
                        }
                        else
                        {
                            v = null;
                            rest = path.SubPath(i, path.Length - i);
                            result = null;

                            return false;
                        }
                    }
                    break;

                case Guid id:
                    {
                        ISyncPathIdObject context = null;

                        if (v is ISyncObject rObj)
                        {
                            VisitSyncObjectOnce(rObj, (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            v = context;
                        }
                        else if (v is ISyncList rList)
                        {
                            VisitSyncListOnce(rList, (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            v = context;
                        }
                        else if (v is ISyncNode rNode)
                        {
                            VisitSyncListOnce(rNode.GetList(), (o, p) => MatchIdAction(o, id, ref context), SyncContext.Empty, new PathContext(), VisitFlag.None);
                            v = context;
                        }
                        else
                        {
                            v = null;
                            rest = path.SubPath(i, path.Length - i);
                            result = null;

                            return false;
                        }
                    }
                    break;

                default:
                    v = null;
                    rest = path.SubPath(i, path.Length - i);
                    result = null;

                    return false;
            }

            if (v is T tV)
            {
                result = tV;
                rest = path.SubPath(i + 1, path.Length - i - 1);

                return true;
            }
        }

        rest = SyncPath.Empty;
        result = null;

        return true;
    }

    private static bool MatchIdAction(object obj, Guid id, ref ISyncPathIdObject context)
    {
        if (context is null && id != Guid.Empty && obj is ISyncPathIdObject c && c.Id == id)
        {
            context = c;

            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool SetValueDeep(object obj, SyncPath path, object value)
    {
        if (SyncPath.IsNullOrEmpty(path))
        {
            return false;
        }

        SyncPath getterPath = path.RemoveLast();
        if (!TryGetValueDeep(obj, getterPath, out object terminal, out _))
        {
            return false;
        }

        object lastPathItem = path[path.Length - 1];

        if (lastPathItem is string name)
        {
            Member.SetProperty(terminal, name, value);
        }
        else if (lastPathItem is int index)
        {
            Member.SetItem(obj, index, value);
        }

        return true;
    }

    private static bool Visit(object obj, VisitorPredicate<object> action, SyncContext context, PathContext path, VisitFlag flag)
    {
        if (obj is null)
        {
            return true;
        }
        // Ignore value structure
        else if (obj.GetType().IsPrimitive || obj.GetType().IsEnum || obj is string)
        {
            return true;
        }

        if (!action(obj, path))
        {
            return false;
        }

        if (obj is ISyncObject syncObject)
        {
            return VisitSyncObject(syncObject, action, context, path, flag);
        }
        else if (obj is ISyncList syncList)
        {
            return VisitSyncList(syncList, action, context, path, flag);
        }
        else if (SyncTypes.GetObjectProxyType(obj) != null)
        {
            return VisitSyncObject(SyncTypes.CreateObjectProxy(obj), action, context, path, flag);
        }
        else if (SyncTypes.GetListProxyType(obj) != null)
        {
            return VisitSyncList(SyncTypes.CreateListProxy(obj), action, context, path, flag);
        }
        else if (context.Resolver != null)
        {
            object proxy = context.Resolver.CreateProxy(obj);

            if (proxy is ISyncObject syncObject2)
            {
                VisitSyncObject(syncObject2, action, context, path, flag);
            }
            else if (proxy is ISyncList syncList2)
            {
                VisitSyncList(syncList2, action, context, path, flag);
            }
        }

        return true;
    }

    private static bool VisitSyncObject(ISyncObject obj, VisitorPredicate<object> action, SyncContext context, PathContext path, VisitFlag flag)
    {
        var elementContext = context.CreateNew(obj);
        var sync = new VisitPropertySync(path, flag);
        bool next = true;
        sync.Visit = o =>
        {
            if (!Visit(o, action, elementContext, path, flag))
            {
                next = false;

                return false;
            }
            else
            {
                return true;
            }
        };

        obj.Sync(sync, context);

        return next;
    }

    private static bool VisitSyncList(ISyncList list, VisitorPredicate<object> action, SyncContext context, PathContext path, VisitFlag flag)
    {
        var elementContext = context.CreateNew(list);
        var sync = new VisitIndexSync(path, flag);
        bool next = true;
        sync.Visit = o =>
        {
            if (!Visit(o, action, elementContext, path, flag))
            {
                next = false;

                return false;
            }
            else
            {
                return true;
            }
        };

        list.Sync(sync, context);

        return next;
    }

    private static void VisitSyncObjectOnce(ISyncObject obj, VisitorPredicate<object> action, SyncContext context, PathContext path, VisitFlag flag)
    {
        var elementContext = context.CreateNew(obj);
        var sync = new VisitPropertySync(path, flag)
        {
            Visit = o => action(o, path)
        };

        obj.Sync(sync, context);
    }

    private static void VisitSyncListOnce(ISyncList list, VisitorPredicate<object> action, SyncContext context, PathContext path, VisitFlag flag)
    {
        var elementContext = context.CreateNew(list);
        var sync = new VisitIndexSync(path, flag)
        {
            Visit = o => action(o, path)
        };

        list.Sync(sync, context);
    }
}

internal delegate bool VisitorElementVisit(object obj);

public class PathContext
{
    private readonly Stack<object> _stack = new();

    internal void Push(string name)
    {
        _stack.Push(name);
    }

    internal void Push(int index)
    {
        _stack.Push(index);
    }

    internal void Pop()
    {
        _stack.Pop();
    }

    public SyncPath GetPath()
    {
        return new SyncPath(_stack);
    }

    public override string ToString()
    {
        return GetPath().ToString();
    }
}

internal class VisitPropertySync(PathContext path, VisitFlag flag) : MarshalByRefObject, IPropertySync
{
    private readonly PathContext _path = path;
    private readonly VisitFlag _flag = flag;

    public VisitorElementVisit Visit;

    public SyncMode Mode => SyncMode.GetAll;

    public SyncIntent Intent => SyncIntent.Visit;

    public string Name => null;

    public IEnumerable<string> Names
    { get { yield break; } }

    public object Value => null;

    public T Sync<T>(string name, T obj, SyncFlag flag = SyncFlag.None, T defaultValue = default, string description = null)
    {
        if ((flag & SyncFlag.Element) == SyncFlag.Element && (_flag & VisitFlag.IgnoreElement) == VisitFlag.IgnoreElement)
        {
            return obj;
        }

        //TODO *** Can the name string be intelligently recognized as an Id, so that renaming the Id later will not result in reference loss

        if ((flag & SyncFlag.PathHidden) == SyncFlag.PathHidden)
        {
            Visit(obj);
        }
        else
        {
            _path.Push(name);
            Visit(obj);
            _path.Pop();
        }

        return obj;
    }
}

internal class VisitIndexSync(PathContext path, VisitFlag flag) : MarshalByRefObject, IIndexSync
{
    private readonly PathContext _path = path;
    private readonly VisitFlag _flag = flag;

    public VisitorElementVisit Visit;

    public SyncMode Mode => SyncMode.GetAll;

    public SyncIntent Intent => SyncIntent.Visit;

    public int Count => 0;

    public int Index => 0;

    public object Value => null;

    public int SyncCount(int count)
    {
        return 0;
    }

    public T Sync<T>(int index, T obj, SyncFlag flag = SyncFlag.None)
    {
        _path.Push(index);
        Visit(obj);
        _path.Pop();

        return obj;
    }

    public string SyncAttribute(string name, string value)
    {
        return value;
    }
}