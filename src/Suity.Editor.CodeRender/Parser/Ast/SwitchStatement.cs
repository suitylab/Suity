using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class SwitchStatement : StatementNode
{
    public Expression Discriminant;
    public IEnumerable<SwitchCase> Cases;
}