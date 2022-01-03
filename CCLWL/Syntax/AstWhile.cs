namespace CCLWL.Syntax
{
    public sealed class AstWhile : AstStatement
    {
        public AstWhile(AstExpression condition, AstStatement body)
        {
            Condition = condition;
            Body = body;
        }

        public override AstStatementKind StatementKind => AstStatementKind.While;
        public AstExpression Condition { get; }
        public AstStatement Body { get; }
    }
}