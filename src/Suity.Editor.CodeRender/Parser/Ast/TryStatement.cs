using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class TryStatement : StatementNode
{
    public StatementNode Block;
    public IEnumerable<StatementNode> GuardedHandlers;
    public IEnumerable<CatchClause> Handlers;
    public StatementNode Finalizer;
}