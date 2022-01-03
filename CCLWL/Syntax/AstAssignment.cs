namespace CCLWL.Syntax
{
    public sealed class AstAssignment : AstStatement
    {
        public AstAssignment(AstExpression operand, Token @operator, AstExpression value)
        {
            Operand = operand;
            Operator = @operator;
            Value = value;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Assignment;
        public AstExpression Operand { get; }
        public Token Operator { get; }
        public AstExpression Value { get; }
    }
}