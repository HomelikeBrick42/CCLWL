namespace CCLWL
{
    public sealed class AstUnary: AstExpression
    {
        public AstUnary(AstExpression operand, Token @operator)
        {
            Operand = operand;
            Operator = @operator;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Unary;
        public AstExpression Operand { get; }
        public Token Operator { get; }
    }
}
