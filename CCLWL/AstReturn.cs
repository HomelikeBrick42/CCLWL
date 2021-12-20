namespace CCLWL
{
    public sealed class AstReturn : AstStatement
    {
        public AstReturn(AstExpression returnExpression)
        {
            ReturnExpression = returnExpression;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Return;
        public AstExpression ReturnExpression { get; }
    }
}