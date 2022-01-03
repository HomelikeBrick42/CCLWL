namespace CCLWL.Syntax
{
    public sealed class AstIf : AstStatement
    {
        public AstIf(AstExpression condition, AstStatement thenStatement, AstStatement elseStatement)
        {
            Condition = condition;
            ThenStatement = thenStatement;
            ElseStatement = elseStatement;
        }

        public override AstStatementKind StatementKind => AstStatementKind.If;
        public AstExpression Condition { get; }
        public AstStatement ThenStatement { get; }
        public AstStatement ElseStatement { get; }
    }
}