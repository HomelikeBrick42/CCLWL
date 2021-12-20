namespace CCLWL
{
    public sealed class AstUnary : AstExpression
    {
        public AstUnary(AstType type, Token @operator, AstExpression operand)
        {
            Type = type;
            Operator = @operator;
            Operand = operand;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Unary;
        public override AstType Type { get; }
        public Token Operator { get; }
        public AstExpression Operand { get; }
    }
}