using Suity.Collections;
using Suity.Helpers;
using Suity.Reflecting;
using System;
using System.Collections.Generic;

namespace Suity.Editor.Transferring;

/// <summary>
/// Defines the pipeline stages for content transfer operations.
/// </summary>
public enum ContentTransferPipelines
{
    /// <summary>Pre-input flow - identifies objects that will be affected by incoming data.</summary>
    Preinput,
    /// <summary>Input property flow - transfers individual property values.</summary>
    InputProperty,
    /// <summary>Input collection flow - transfers collection items.</summary>
    InputCollection,

    /// <summary>Delete flow - removes objects from the target.</summary>
    Delete,

    /// <summary>Output property flow - writes individual property values.</summary>
    OutputProperty,
    /// <summary>Output collection flow - writes collection items.</summary>
    OutputCollection,
}

/// <summary>
/// Base class for content transfer operations that handle reading/writing data between different formats.
/// </summary>
public abstract class ContentTransfer
{
    /// <summary>
    /// Gets the content transfer for the specified type.
    /// </summary>
    /// <typeparam name="T">The target type.</typeparam>
    /// <param name="type">The source type to get transfer for.</param>
    /// <returns>The content transfer instance.</returns>
    public static ContentTransfer<T> GetTransfer<T>(Type type)
    {
        return ContentTransfer<T>.GetTransfer(type);
    }

}

#region ContentTransfer<TTransfer>

/// <summary>
/// Generic base class for content transfers with a specific target type.
/// </summary>
/// <typeparam name="TTarget">The target type for the transfer.</typeparam>
public abstract class ContentTransfer<TTarget> : ContentTransfer
{
    /// <summary>
    /// Transfers data between source and target using the specified pipeline.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="pipeline">The transfer pipeline stage.</param>
    /// <param name="selection">Collection to store affected objects.</param>
    public abstract void Transfer(object source, TTarget target, ContentTransferPipelines pipeline, ICollection<object> selection = null);

    private static readonly Dictionary<Type, ContentTransfer<TTarget>> _dataReadWrites = [];

    /// <summary>
    /// Pre-input flow, objects that may be affected should be saved to the selection collection to let the caller know which objects in the incoming data will be affected.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="selection">Collection to save affected objects.</param>
    public void PreInput(object source, TTarget target, ICollection<object> selection = null)
    {
        Transfer(source, target, ContentTransferPipelines.Preinput, selection);
    }

    /// <summary>
    /// Performs input transfer for properties and collections.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="preinput">Whether to run pre-input first.</param>
    public void Input(object source, TTarget target, bool preinput = false)
    {
        if (preinput)
        {
            Transfer(source, target, ContentTransferPipelines.Preinput);
        }

        Transfer(source, target, ContentTransferPipelines.InputProperty);
        Transfer(source, target, ContentTransferPipelines.InputCollection);
    }

    /// <summary>
    /// Performs delete transfer to remove objects.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="selection">Objects to be deleted.</param>
    public void Delete(object source, TTarget target, ICollection<object> selection)
    {
        Transfer(source, target, ContentTransferPipelines.Delete, selection);
    }

    /// <summary>
    /// Performs output transfer for properties and collections.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="selection">Objects to output.</param>
    public void Output(object source, TTarget target, ICollection<object> selection = null)
    {
        Transfer(source, target, ContentTransferPipelines.OutputProperty, selection);
        Transfer(source, target, ContentTransferPipelines.OutputCollection, selection);
    }



    /// <summary>
    /// Gets the content transfer for the specified type.
    /// </summary>
    /// <param name="type">The source type.</param>
    /// <returns>The content transfer instance, or null if not found.</returns>
    public static ContentTransfer<TTarget> GetTransfer(Type type)
    {
        if (type is null)
        {
            return null;
        }

        var readWrite = _dataReadWrites.GetOrAdd(type, t =>
        {
            var implTypes = typeof(ContentTransfer<,>).GetGenericDerivedType(t, typeof(TTarget));
            foreach (var implType in implTypes)
            {
                try
                {
                    if (implType.CreateInstanceOf() is ContentTransfer<TTarget> target)
                    {
                        return target;
                    }
                }
                catch (Exception)
                {
                }
            }

            return null;
        });

        return readWrite;
    }

    /// <summary>
    /// Gets the transfer for the source type and performs pre-input.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    public static void GetAndPreInput(object source, TTarget target)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType());
        if (transfer is null)
        {
            throw new NullReferenceException($"{nameof(ContentTransfer)} not found for type : {source.GetType().Name}");
        }

        transfer.PreInput(source, target);
    }

    /// <summary>
    /// Tries to get the transfer for the source type and perform pre-input.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryGetAndPreInput(object source, TTarget target)
    {
        var transfer = GetTransfer(source?.GetType());
        if (transfer is null)
        {
            return false;
        }

        try
        {
            transfer.PreInput(source, target);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the transfer for the source type and performs input.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="preinput">Whether to run pre-input first.</param>
    public static void GetAndInput(object source, TTarget target, bool preinput = false)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType());
        if (transfer is null)
        {
            throw new NullReferenceException($"{nameof(ContentTransfer)} not found for type : {source.GetType().Name}");
        }

        transfer.Input(source, target, preinput);
    }

    /// <summary>
    /// Tries to get the transfer for the source type and perform input.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="preinput">Whether to run pre-input first.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryGetAndInput(object source, TTarget target, bool preinput = false)
    {
        var transfer = GetTransfer(source?.GetType());
        if (transfer is null)
        {
            return false;
        }

        try
        {
            transfer.Input(source, target, preinput);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets the transfer for the source type and performs output.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="selection">Objects to output.</param>
    public static void GetAndOutput(object source, TTarget target, ICollection<object> selection = null)
    {
        if (source is null)
        {
            throw new ArgumentNullException(nameof(source));
        }

        var transfer = GetTransfer(source.GetType());
        if (transfer is null)
        {
            throw new NullReferenceException($"{nameof(ContentTransfer)} not found for type : {source.GetType().Name}");
        }

        transfer.Output(source, target, selection);
    }

    /// <summary>
    /// Tries to get the transfer for the source type and perform output.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="selection">Objects to output.</param>
    /// <returns>True if successful, false otherwise.</returns>
    public static bool TryGetAndOutput(object source, TTarget target, ICollection<object> selection = null)
    {
        var transfer = GetTransfer(source?.GetType());
        if (transfer is null)
        {
            return false;
        }

        try
        {
            transfer.Output(source, target, selection);
        }
        catch (Exception)
        {
            return false;
        }

        return true;
    }
}

#endregion

#region EmptyContentTransfer<TTransfer>

/// <summary>
/// A content transfer that performs no operations.
/// </summary>
/// <typeparam name="TTarget">The target type.</typeparam>
public sealed class EmptyContentTransfer<TTarget> : ContentTransfer<TTarget>
{
    /// <summary>
    /// Gets a singleton empty content transfer instance.
    /// </summary>
    public static EmptyContentTransfer<TTarget> Empty { get; } = new();

    private EmptyContentTransfer()
    {
    }

    /// <summary>
    /// Performs no operation.
    /// </summary>
    public override void Transfer(object source, TTarget target, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    { }
}

#endregion

#region ContentTransfer<TSource, TTransfer>

/// <summary>
/// Content transfer that supports source type inheritance hierarchy.
/// </summary>
/// <typeparam name="TSource">The source type.</typeparam>
/// <typeparam name="TTarget">The target type.</typeparam>
public abstract class ContentTransfer<TSource, TTarget> : ContentTransfer<TTarget>
{
    private bool _baseObtained;
    private ContentTransfer<TTarget> _baseTransfer;

    /// <summary>
    /// Gets the base transfer from the inheritance hierarchy.
    /// </summary>
    public ContentTransfer<TTarget> BaseTransfer
    {
        get
        {
            if (_baseObtained)
            {
                return _baseTransfer;
            }

            _baseObtained = true;

            var type = typeof(TSource).BaseType;
            while (type != null)
            {
                _baseTransfer = GetTransfer(type);
                if (_baseTransfer != null)
                {
                    return _baseTransfer;
                }

                type = type.BaseType;
            }

            _baseTransfer = null;

            return _baseTransfer;
        }
    }

    /// <summary>
    /// Transfers data between source and target, delegating to base transfer first.
    /// </summary>
    public override sealed void Transfer(object source, TTarget target, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
        BaseTransfer?.Transfer(source, target, pipeline, selection);

        Transfer((TSource)source, target, pipeline, selection);
    }

    /// <summary>
    /// Transfers data between typed source and target.
    /// </summary>
    /// <param name="source">The source object.</param>
    /// <param name="target">The target object.</param>
    /// <param name="pipeline">The transfer pipeline stage.</param>
    /// <param name="selection">Collection to store affected objects.</param>
    public virtual void Transfer(TSource source, TTarget target, ContentTransferPipelines pipeline, ICollection<object> selection = null)
    {
    }
}

#endregion
