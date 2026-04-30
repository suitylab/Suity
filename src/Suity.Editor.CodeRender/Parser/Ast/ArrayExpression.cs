using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class ArrayExpression : Expression
{
    public IEnumerable<Expression> Elements;
}