namespace CCLWL
{
    public abstract class AstExpression : AstNode
    {
        public override AstKind Kind => AstKind.Expression;
        public abstract AstExpressionKind ExpressionKind { get; }
        public abstract AstType Type { get; }
    }

    public sealed class AstCast : AstExpression
    {
        public AstCast(AstType type, AstExpression expression)
        {
            Type = type;
            Expression = expression;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Cast;
        public override AstType Type { get; }
        public AstExpression Expression { get; }
    }
}