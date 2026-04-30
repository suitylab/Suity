using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class SequenceExpression : Expression
{
    public IList<Expression> Expressions;
}