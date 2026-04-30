using Suity.Parser.Ast;
using System.Collections.Generic;

namespace Suity.Parser;

public interface IFunctionDeclaration : IFunctionScope
{
    Identifier Id { get; }
    IEnumerable<Identifier> Parameters { get; }
    StatementNode Body { get; }
    bool Strict { get; }
}