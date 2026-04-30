using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class BlockStatement : StatementNode
{
    public IEnumerable<StatementNode> Body;
}