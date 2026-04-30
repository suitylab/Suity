using Suity.Rex.Operators;
using System;
using System.Collections.Generic;

namespace Suity.Rex.VirtualDom;

/// <summary>
/// Provides extension methods for RexTree operations, including listener creation, data manipulation, and operator chaining.
/// </summary>
public static class RexTreeExtensions
{
    #region Listener

    /// <summary>
    /// Creates a data listener for the specified path.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives data changes at the specified path.</returns>
    public static IRexListener<T> AsRexListener<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeListener<T>(engine, path, tag);
    }

    /// <summary>
    /// Creates a data listener for collections at the specified path.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives collection changes at the specified path.</returns>
    public static IRexListener<IEnumerable<T>> AsRexListeners<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeListener<IEnumerable<T>>(engine, path, tag);
    }

    /// <summary>
    /// Creates a before-listener for the specified path. Before-listeners are invoked before regular listeners and can be cancelled.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives data changes at the specified path.</returns>
    public static IRexListener<T> AsRexBeforeListener<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeBeforeListener<T>(engine, path, tag);
    }

    /// <summary>
    /// Creates a before-listener for collections at the specified path.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives collection changes at the specified path.</returns>
    public static IRexListener<IEnumerable<T>> AsRexBeforeListeners<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeBeforeListener<IEnumerable<T>>(engine, path, tag);
    }

    /// <summary>
    /// Creates an after-listener for the specified path. After-listeners are invoked after regular listeners.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives data changes at the specified path.</returns>
    public static IRexListener<T> AsRexAfterListener<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeAfterListener<T>(engine, path, tag);
    }

    /// <summary>
    /// Creates an after-listener for collections at the specified path.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="engine">The RexTree to create the listener from.</param>
    /// <param name="path">The path to listen on.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives collection changes at the specified path.</returns>
    public static IRexListener<IEnumerable<T>> AsRexAfterListeners<T>(this RexTree engine, RexPath path, string tag = null)
    {
        return new RexTreeAfterListener<IEnumerable<T>>(engine, path, tag);
    }

    #endregion

    #region Define

    /// <summary>
    /// Gets the data value for a property definition.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="model">The RexTree to retrieve the data from.</param>
    /// <param name="property">The property definition.</param>
    /// <returns>The current value of the property.</returns>
    public static T GetData<T>(this RexTree model, RexPropertyDefine<T> property)
    {
        return model.GetData<T>(property.Path);
    }

    /// <summary>
    /// Sets the data value for a property definition.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="property">The property definition.</param>
    /// <param name="value">The value to set.</param>
    public static void SetData<T>(this RexTree model, RexPropertyDefine<T> property, T value)
    {
        model.SetData<T>(property.Path, value);
    }

    /// <summary>
    /// Sets the data value deeply for a property definition, replacing all nested child data.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="property">The property definition.</param>
    /// <param name="value">The value to set deeply.</param>
    public static void SetDataDeep<T>(this RexTree model, RexPropertyDefine<T> property, T value)
    {
        model.SetDataDeep<T>(property.Path, value);
    }

    /// <summary>
    /// Sets the default data value for a property definition if no value exists.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="property">The property definition.</param>
    /// <param name="value">The default value to set.</param>
    public static void SetDefaultData<T>(this RexTree model, RexPropertyDefine<T> property, T value)
    {
        model.SetDefaultData<T>(property.Path, value);
    }

    /// <summary>
    /// Triggers an update notification for a property definition.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="model">The RexTree to update.</param>
    /// <param name="property">The property definition.</param>
    public static void UpdateData<T>(this RexTree model, RexPropertyDefine<T> property)
    {
        model.UpdateData(property.Path);
    }

    /// <summary>
    /// Invokes an action without arguments.
    /// </summary>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="action">The action definition.</param>
    public static void DoAction(this RexTree model, RexActionDefine action)
    {
        model.DoAction(action.Path);
    }

    /// <summary>
    /// Invokes an action with a single argument.
    /// </summary>
    /// <typeparam name="T">The type of the action argument.</typeparam>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="action">The action definition.</param>
    /// <param name="arg">The argument to pass.</param>
    public static void DoAction<T>(this RexTree model, RexActionDefine<T> action, T arg)
    {
        model.DoAction<T>(action.Path, arg);
    }

    /// <summary>
    /// Invokes an action with two arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="action">The action definition.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    public static void DoAction<T1, T2>(this RexTree model, RexActionDefine<T1, T2> action, T1 arg1, T2 arg2)
    {
        model.DoAction<T1, T2>(action.Path, arg1, arg2);
    }

    /// <summary>
    /// Invokes an action with three arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="action">The action definition.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    public static void DoAction<T1, T2, T3>(this RexTree model, RexActionDefine<T1, T2, T3> action, T1 arg1, T2 arg2, T3 arg3)
    {
        model.DoAction<T1, T2, T3>(action.Path, arg1, arg2, arg3);
    }

    /// <summary>
    /// Invokes an action with four arguments.
    /// </summary>
    /// <typeparam name="T1">The type of the first argument.</typeparam>
    /// <typeparam name="T2">The type of the second argument.</typeparam>
    /// <typeparam name="T3">The type of the third argument.</typeparam>
    /// <typeparam name="T4">The type of the fourth argument.</typeparam>
    /// <param name="model">The RexTree to invoke the action on.</param>
    /// <param name="action">The action definition.</param>
    /// <param name="arg1">The first argument.</param>
    /// <param name="arg2">The second argument.</param>
    /// <param name="arg3">The third argument.</param>
    /// <param name="arg4">The fourth argument.</param>
    public static void DoAction<T1, T2, T3, T4>(this RexTree model, RexActionDefine<T1, T2, T3, T4> action, T1 arg1, T2 arg2, T3 arg3, T4 arg4)
    {
        model.DoAction<T1, T2, T3, T4>(action.Path, arg1, arg2, arg3, arg4);
    }

    #endregion

    #region IRexInstance

    /// <summary>
    /// Creates a data listener from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives data changes.</returns>
    public static IRexListener<T> AsRexListener<T>(this IRexTreeInstance<T> property, string tag = null)
    {
        return new RexTreeListener<T>(property.Tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a collection data listener from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexListeners<T>(this IRexTreeInstance<IEnumerable<T>> property, string tag = null)
    {
        return new RexTreeListener<IEnumerable<T>>(property.Tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a before-listener from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives data changes.</returns>
    public static IRexListener<T> AsRexBeforeListener<T>(this IRexTreeInstance<object> property, string tag = null)
    {
        return new RexTreeBeforeListener<T>(property.Tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a before-listener for collections from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexBeforeListeners<T>(this IRexTreeInstance<object> property, string tag = null)
    {
        return new RexTreeBeforeListener<IEnumerable<T>>(property.Tree, property.Path, tag);
    }

    /// <summary>
    /// Creates an after-listener from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives data changes.</returns>
    public static IRexListener<T> AsRexAfterListener<T>(this IRexTreeInstance<object> property, string tag = null)
    {
        return new RexTreeAfterListener<T>(property.Tree, property.Path, tag);
    }

    /// <summary>
    /// Creates an after-listener for collections from a RexTree instance.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree instance.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexAfterListeners<T>(this IRexTreeInstance<object> property, string tag = null)
    {
        return new RexTreeAfterListener<IEnumerable<T>>(property.Tree, property.Path, tag);
    }

    #endregion

    #region IRexDefine

    /// <summary>
    /// Creates a data listener from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives data changes.</returns>
    public static IRexListener<T> AsRexListener<T>(this IRexTreeDefine<T> property, RexTree tree, string tag = null)
    {
        return new RexTreeListener<T>(tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a collection data listener from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexListeners<T>(this IRexTreeDefine<IEnumerable<T>> property, RexTree tree, string tag = null)
    {
        return new RexTreeListener<IEnumerable<T>>(tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a before-listener from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives data changes.</returns>
    public static IRexListener<T> AsRexBeforeListener<T>(this IRexTreeDefine<object> property, RexTree tree, string tag = null)
    {
        return new RexTreeBeforeListener<T>(tree, property.Path, tag);
    }

    /// <summary>
    /// Creates a before-listener for collections from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>A before-listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexBeforeListeners<T>(this IRexTreeDefine<object> property, RexTree tree, string tag = null)
    {
        return new RexTreeBeforeListener<IEnumerable<T>>(tree, property.Path, tag);
    }

    /// <summary>
    /// Creates an after-listener from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The type of data to listen for.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives data changes.</returns>
    public static IRexListener<T> AsRexAfterListener<T>(this IRexTreeDefine<object> property, RexTree tree, string tag = null)
    {
        return new RexTreeAfterListener<T>(tree, property.Path, tag);
    }

    /// <summary>
    /// Creates an after-listener for collections from a RexTree definition.
    /// </summary>
    /// <typeparam name="T">The element type of the collection.</typeparam>
    /// <param name="property">The RexTree definition.</param>
    /// <param name="tree">The RexTree to create the listener from.</param>
    /// <param name="tag">An optional tag for identifying the listener.</param>
    /// <returns>An after-listener that receives collection changes.</returns>
    public static IRexListener<IEnumerable<T>> AsRexAfterListeners<T>(this IRexTreeDefine<object> property, RexTree tree, string tag = null)
    {
        return new RexTreeAfterListener<IEnumerable<T>>(tree, property.Path, tag);
    }

    #endregion

    #region Operation

    /// <summary>
    /// Creates a conditional listener that only propagates when the specified data predicate is satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the source listener data.</typeparam>
    /// <typeparam name="TData">The type of the condition data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to monitor for the condition.</param>
    /// <param name="path">The path to check the condition on.</param>
    /// <param name="predicate">The predicate to evaluate.</param>
    /// <returns>A conditional listener.</returns>
    public static IRexListener<T> WhenData<T, TData>(this IRexListener<T> source, RexTree model, RexPath path, Predicate<TData> predicate)
    {
        return new WhenData<T, TData>(source, model, path, predicate);
    }

    /// <summary>
    /// Creates a conditional listener that only propagates when the specified property predicate is satisfied.
    /// </summary>
    /// <typeparam name="T">The type of the source listener data.</typeparam>
    /// <typeparam name="TData">The type of the property value.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="property">The property to check the condition on.</param>
    /// <param name="predicate">The predicate to evaluate.</param>
    /// <returns>A conditional listener.</returns>
    public static IRexListener<T> WhenProperty<T, TData>(this IRexListener<T> source, IRexProperty<TData> property, Predicate<TData> predicate)
    {
        return new WhenProperty<T, TData>(source, property, predicate);
    }

    /// <summary>
    /// Creates a listener that sets data to a target path when the source listener is triggered.
    /// </summary>
    /// <typeparam name="TSource">The type of the source listener data.</typeparam>
    /// <typeparam name="TResult">The type of the target data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="pathFunc">A function that determines the target path from the source data.</param>
    /// <param name="dataFunc">A function that transforms the source data to the target value.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<TResult> SetDataTo<TSource, TResult>(this IRexListener<TSource> source, RexTree model, Func<TSource, RexPath> pathFunc, Func<TSource, TResult> dataFunc)
    {
        return new SetDataTo<TSource, TResult>(source, model, pathFunc, dataFunc);
    }

    /// <summary>
    /// Creates a listener that sets data to a fixed target path when the source listener is triggered.
    /// </summary>
    /// <typeparam name="TSource">The type of the source listener data.</typeparam>
    /// <typeparam name="TResult">The type of the target data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="path">The fixed target path.</param>
    /// <param name="dataFunc">A function that transforms the source data to the target value.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<TResult> SetDataTo<TSource, TResult>(this IRexListener<TSource> source, RexTree model, RexPath path, Func<TSource, TResult> dataFunc)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }
        return new SetDataTo<TSource, TResult>(source, model, o => path, dataFunc);
    }

    /// <summary>
    /// Creates a listener that sets data to a target instance when the source listener is triggered.
    /// </summary>
    /// <typeparam name="TSource">The type of the source listener data.</typeparam>
    /// <typeparam name="TResult">The type of the target data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="target">The target RexTree instance.</param>
    /// <param name="dataFunc">A function that transforms the source data to the target value.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<TResult> SetDataTo<TSource, TResult>(this IRexListener<TSource> source, IRexTreeInstance<TResult> target, Func<TSource, TResult> dataFunc)
    {
        return new SetDataTo<TSource, TResult>(source, target.Tree, o => target.Path, dataFunc);
    }

    /// <summary>
    /// Creates a listener that sets the source data to a target path determined by a function.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="pathFunc">A function that determines the target path from the source data.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<T> SetDataTo<T>(this IRexListener<T> source, RexTree model, Func<T, RexPath> pathFunc)
    {
        return new SetDataTo<T, T>(source, model, pathFunc, o => o);
    }

    /// <summary>
    /// Creates a listener that sets the source data to a fixed target path.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to set the data in.</param>
    /// <param name="path">The fixed target path.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<T> SetDataTo<T>(this IRexListener<T> source, RexTree model, RexPath path)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }
        return new SetDataTo<T, T>(source, model, o => path, o => o);
    }

    /// <summary>
    /// Creates a listener that sets the source data to a target instance.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="target">The target RexTree instance.</param>
    /// <returns>A listener that performs the data set operation.</returns>
    public static IRexListener<T> SetDataTo<T>(this IRexListener<T> source, IRexTreeInstance<T> target)
    {
        return new SetDataTo<T, T>(source, target.Tree, o => target.Path, o => o);
    }

    /// <summary>
    /// Creates a listener that maps update notifications to a target path determined by a function.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to send updates to.</param>
    /// <param name="pathFunc">A function that determines the target path from the source data.</param>
    /// <returns>A listener that performs the update mapping.</returns>
    public static IRexListener<T> MapUpdateTo<T>(this IRexListener<T> source, RexTree model, Func<T, RexPath> pathFunc)
    {
        return new MapUpdateTo<T>(source, model, pathFunc);
    }

    /// <summary>
    /// Creates a listener that maps update notifications to a fixed target path.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to send updates to.</param>
    /// <param name="path">The fixed target path.</param>
    /// <returns>A listener that performs the update mapping.</returns>
    public static IRexListener<T> MapUpdateTo<T>(this IRexListener<T> source, RexTree model, RexPath path)
    {
        if (path == null)
        {
            throw new ArgumentNullException();
        }
        return new MapUpdateTo<T>(source, model, o => path);
    }

    /// <summary>
    /// Creates a listener that maps update notifications to a target instance.
    /// </summary>
    /// <typeparam name="T">The type of the data.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="target">The target RexTree instance.</param>
    /// <returns>A listener that performs the update mapping.</returns>
    public static IRexListener<T> MapUpdateTo<T>(this IRexListener<T> source, IRexTreeInstance<T> target)
    {
        return new MapUpdateTo<T>(source, target.Tree, o => target.Path);
    }

    /// <summary>
    /// Creates a listener that maps action notifications to a target path.
    /// </summary>
    /// <typeparam name="T">The type of the action arguments.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="model">The RexTree to send actions to.</param>
    /// <param name="path">The target path.</param>
    /// <returns>A listener that performs the action mapping.</returns>
    public static IRexListener<T> MapActionTo<T>(this IRexListener<T> source, RexTree model, RexPath path) where T : ActionArguments
    {
        return new MapActionTo<T>(source, model, path);
    }

    /// <summary>
    /// Creates a listener that maps action notifications to a target action instance.
    /// </summary>
    /// <typeparam name="T">The type of the action arguments.</typeparam>
    /// <param name="source">The source listener.</param>
    /// <param name="action">The target action instance.</param>
    /// <returns>A listener that performs the action mapping.</returns>
    public static IRexListener<T> MapActionTo<T>(this IRexListener<T> source, IRexTreeInstance<T> action) where T : ActionArguments
    {
        return new MapActionTo<T>(source, action.Tree, action.Path);
    }

    #endregion
}
