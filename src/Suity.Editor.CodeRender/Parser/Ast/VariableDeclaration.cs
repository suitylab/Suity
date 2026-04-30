using System.Collections.Generic;

namespace Suity.Parser.Ast;

public class VariableDeclaration : StatementNode
{
    public IEnumerable<VariableDeclarator> Declarations;
    public string Kind;
}