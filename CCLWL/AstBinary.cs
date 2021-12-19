namespace CCLWL
{
    public sealed class AstBinary: AstExpression
    {
        public AstBinary(AstType type, AstExpression left, Token @operator, AstExpression right)
        {
            Type = type;
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Binary;
        public override AstType Type { get; }
        public AstExpression Left { get; }
        public Token Operator { get; }
        public AstExpression Right { get; }
    }
}
