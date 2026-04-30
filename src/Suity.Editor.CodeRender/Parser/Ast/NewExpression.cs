using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class NewExpression : Expression
{
    public Expression Callee;
    public IEnumerable<Expression> Arguments;
}