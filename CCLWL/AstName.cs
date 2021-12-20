namespace CCLWL
{
    public sealed class AstName : AstExpression
    {
        public AstName(AstDeclaration declaration, string name)
        {
            Declaration = declaration;
            Name = name;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.Name;
        public override AstType Type => Declaration.Type;
        public AstDeclaration Declaration { get; }
        public string Name { get; }
    }
}