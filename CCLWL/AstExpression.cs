namespace CCLWL
{
    public abstract class AstExpression : AstNode
    {
        public override AstKind Kind => AstKind.Expression;
        public abstract AstExpressionKind ExpressionKind { get; }
    }
}
