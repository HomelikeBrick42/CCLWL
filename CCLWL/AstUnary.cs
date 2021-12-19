namespace CCLWL
{
    public sealed class AstUnary: AstExpression
    {
        public AstUnary(AstType type, AstExpression operand, Token @operator)
        {
            Type = type;
            Operand = operand;
            Operator = @operator;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Unary;
        public override AstType Type { get; }
        public AstExpression Operand { get; }
        public Token Operator { get; }
    }
}
