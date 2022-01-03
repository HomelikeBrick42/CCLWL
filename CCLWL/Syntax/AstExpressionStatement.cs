namespace CCLWL.Syntax
{
    public sealed class AstExpressionStatement : AstStatement
    {
        public AstExpressionStatement(AstExpression expression)
        {
            Expression = expression;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Expression;
        public AstExpression Expression { get; }
    }
}