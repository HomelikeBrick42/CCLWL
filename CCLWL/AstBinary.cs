namespace CCLWL
{
    public sealed class AstBinary: AstExpression
    {
        public AstBinary(AstExpression left, Token @operator, AstExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Binary;
        public AstExpression Left { get; }
        public Token Operator { get; }
        public AstExpression Right { get; }
    }
}
