using System.Collections;

namespace Suity.Collections;

/// <summary>
/// An empty enumerator that always returns false for MoveNext.
/// </summary>
public class EmptyEnumerator : IEnumerator
{
    public static EmptyEnumerator Emtpy { get; } = new();

    private EmptyEnumerator()
    {
    }

    #region IEnumerator

    public object Current => null;

    public bool MoveNext() => false;

    public void Reset()
    { }

    #endregion
}