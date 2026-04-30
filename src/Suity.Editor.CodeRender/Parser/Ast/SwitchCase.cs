using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class SwitchCase : SyntaxNode
{
    public Expression Test;
    public IEnumerable<StatementNode> Consequent;
}