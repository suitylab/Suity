using Suity.Helpers;

namespace Suity.Editor.Flows.Nodes;

#region Increment

/// <summary>
/// A flow node that increments an integer value by one.
/// </summary>
[DisplayText("++")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class Increment : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Increment"/> class.
    /// </summary>
    public Increment()
    {
        _v = AddConnector("V", "int", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "int", FlowDirections.Output, FlowConnectorTypes.Data, null, "++");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<int>(_v);
        compute.SetValue(_out, v + 1);
    }
}

#endregion

#region Decrement

/// <summary>
/// A flow node that decrements an integer value by one.
/// </summary>
[DisplayText("--")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class Decrement : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Decrement"/> class.
    /// </summary>
    public Decrement()
    {
        _v = AddConnector("V", "int", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "int", FlowDirections.Output, FlowConnectorTypes.Data, null, "--");
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<int>(_v);
        compute.SetValue(_out, v - 1);
    }
}

#endregion

#region Addition

/// <summary>
/// A flow node that computes the sum of two floating-point values.
/// </summary>
[DisplayText("A + B")]
public class Addition : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Addition"/> class.
    /// </summary>
    public Addition()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, a + b);
    }
}

#endregion

#region Subtract

/// <summary>
/// A flow node that computes the difference between two floating-point values (A - B).
/// </summary>
[DisplayText("A - B")]
public class Subtract : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Subtract"/> class.
    /// </summary>
    public Subtract()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, a - b);
    }
}

#endregion

#region Multiply

/// <summary>
/// A flow node that computes the product of two floating-point values.
/// </summary>
[DisplayText("A * B")]
public class Multiply : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Multiply"/> class.
    /// </summary>
    public Multiply()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, a * b);
    }
}

#endregion

#region Divide

/// <summary>
/// A flow node that computes the quotient of two floating-point values (A / B).
/// </summary>
[DisplayText("A / B")]
public class Divide : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Divide"/> class.
    /// </summary>
    public Divide()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, a / b);
    }
}

#endregion

#region Pow

/// <summary>
/// A flow node that raises a floating-point value to a specified power.
/// </summary>
[DisplayText("Pow")]
public class Pow : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _p;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Pow"/> class.
    /// </summary>
    public Pow()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _p = AddConnector("Power", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        float p = compute.GetValueConvert<float>(_p);
        compute.SetValue(_out, Mathf.Pow(v, p));
    }
}

#endregion

#region Negative

/// <summary>
/// A flow node that negates a floating-point value.
/// </summary>
[DisplayText("-V")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class Negative : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Negative"/> class.
    /// </summary>
    public Negative()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("-V", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        compute.SetValue(_out, -v);
    }
}

#endregion

#region Not

/// <summary>
/// A flow node that computes the logical negation of a boolean value.
/// </summary>
[DisplayText("Not")]
[SimpleFlowNodeStyle(HasHeader = false, Width = 100, Height = 20)]
public class Not : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Not"/> class.
    /// </summary>
    public Not()
    {
        _v = AddConnector("V", "bool", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Not", "bool", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        bool v = compute.GetValueConvert<bool>(_v);
        compute.SetValue(_out, !v);
    }
}

#endregion

#region Abs

/// <summary>
/// A flow node that computes the absolute value of a floating-point number.
/// </summary>
[DisplayText("Abs")]
public class Abs : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Abs"/> class.
    /// </summary>
    public Abs()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        compute.SetValue(_out, Mathf.Abs(v));
    }
}

#endregion

#region Round

/// <summary>
/// A flow node that rounds a floating-point value to the nearest integer.
/// </summary>
[DisplayText("Round")]
public class Round : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Round"/> class.
    /// </summary>
    public Round()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        compute.SetValue(_out, Mathf.Round(v));
    }
}

#endregion

#region Floor

/// <summary>
/// A flow node that computes the largest integer less than or equal to a floating-point value.
/// </summary>
[DisplayText("Floor")]
public class Floor : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Floor"/> class.
    /// </summary>
    public Floor()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        compute.SetValue(_out, Mathf.Floor(v));
    }
}

#endregion

#region Ceil

/// <summary>
/// A flow node that computes the smallest integer greater than or equal to a floating-point value.
/// </summary>
[DisplayText("Ceil")]
public class Ceil : ValueFlowNode
{
    private readonly FixedNodeConnector _v;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Ceil"/> class.
    /// </summary>
    public Ceil()
    {
        _v = AddConnector("V", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float v = compute.GetValueConvert<float>(_v);
        compute.SetValue(_out, Mathf.Ceil(v));
    }
}

#endregion

#region Max

/// <summary>
/// A flow node that returns the larger of two floating-point values.
/// </summary>
[DisplayText("Max")]
public class Max : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Max"/> class.
    /// </summary>
    public Max()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, Mathf.Max(a, b));
    }
}

#endregion

#region Min

/// <summary>
/// A flow node that returns the smaller of two floating-point values.
/// </summary>
[DisplayText("Min")]
public class Min : ValueFlowNode
{
    private readonly FixedNodeConnector _a;
    private readonly FixedNodeConnector _b;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Min"/> class.
    /// </summary>
    public Min()
    {
        _a = AddConnector("A", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _b = AddConnector("B", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float a = compute.GetValueConvert<float>(_a);
        float b = compute.GetValueConvert<float>(_b);
        compute.SetValue(_out, Mathf.Min(a, b));
    }
}

#endregion

#region PI

/// <summary>
/// A flow node that outputs the mathematical constant PI (π).
/// </summary>
[DisplayText("PI")]
public class PI : ValueFlowNode
{
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="PI"/> class.
    /// </summary>
    public PI()
    {
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        compute.SetValue(_out, Mathf.PI);
    }
}

#endregion

#region Clamp

/// <summary>
/// A flow node that clamps a floating-point value to a specified minimum and maximum range.
/// </summary>
[DisplayText("Clamp")]
public class Clamp : ValueFlowNode
{
    private readonly FixedNodeConnector _in;
    private readonly FixedNodeConnector _min;
    private readonly FixedNodeConnector _max;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Clamp"/> class.
    /// </summary>
    public Clamp()
    {
        _in = AddConnector("In", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _min = AddConnector("Min", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _max = AddConnector("Max", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float @in = compute.GetValueConvert<float>(_in);
        float min = compute.GetValueConvert<float>(_min);
        float max = compute.GetValueConvert<float>(_max);
        compute.SetValue(_out, Mathf.Clamp(@in, min, max));
    }
}

#endregion

#region Lerp

/// <summary>
/// A flow node that performs linear interpolation between two floating-point values.
/// </summary>
[DisplayText("Lerp")]
public class Lerp : ValueFlowNode
{
    private readonly FixedNodeConnector _in;
    private readonly FixedNodeConnector _from;
    private readonly FixedNodeConnector _to;
    private readonly FixedNodeConnector _out;

    /// <summary>
    /// Initializes a new instance of the <see cref="Lerp"/> class.
    /// </summary>
    public Lerp()
    {
        _in = AddConnector("In", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _from = AddConnector("From", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _to = AddConnector("To", "*System|Single", FlowDirections.Input, FlowConnectorTypes.Data);
        _out = AddConnector("Out", "*System|Single", FlowDirections.Output, FlowConnectorTypes.Data);
    }

    /// <inheritdoc/>
    public override void Compute(IFlowComputation compute)
    {
        float @in = compute.GetValueConvert<float>(_in);
        float from = compute.GetValueConvert<float>(_from);
        float to = compute.GetValueConvert<float>(_to);
        compute.SetValue(_out, Mathf.Lerp(from, to, @in));
    }
}

#endregion
