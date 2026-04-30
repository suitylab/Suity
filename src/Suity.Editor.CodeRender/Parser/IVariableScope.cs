using Suity.Parser.Ast;
using System.Collections.Generic;

namespace Suity.Parser;

/// <summary>
/// Used to safe references to all variable delcarations in a specific scope.
/// Hoisting.
/// </summary>
public interface IVariableScope
{
    IList<VariableDeclaration> VariableDeclarations { get; set; }
}

public class VariableScope : IVariableScope
{
    public VariableScope()
    {
        VariableDeclarations = [];
    }

    public IList<VariableDeclaration> VariableDeclarations { get; set; }
}