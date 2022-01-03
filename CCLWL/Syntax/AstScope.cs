using System.Collections.Generic;

namespace CCLWL.Syntax
{
    public sealed class AstScope : AstStatement
    {
        public AstScope(IEnumerable<AstStatement> statements)
        {
            Statements = statements;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Scope;
        public IEnumerable<AstStatement> Statements { get; }
    }
}