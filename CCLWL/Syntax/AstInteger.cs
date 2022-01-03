namespace CCLWL.Syntax
{
    public sealed class AstInteger : AstExpression
    {
        public AstInteger(AstIntegerType type, long value)
        {
            Type = type;
            Value = value;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Integer;
        public override AstIntegerType Type { get; }
        public long Value { get; }
    }
}