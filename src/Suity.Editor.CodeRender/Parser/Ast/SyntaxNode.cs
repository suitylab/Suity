using System.Diagnostics;

namespace Suity.Parser.Ast;

public class SyntaxNode
{
    public SyntaxNodes Type;
    public int[] Range;
    public Location Location;

    [DebuggerStepThrough]
    public T As<T>() where T : SyntaxNode
    {
        return (T)this;
    }
}
