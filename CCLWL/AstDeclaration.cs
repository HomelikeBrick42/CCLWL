namespace CCLWL
{
    public sealed class AstDeclaration : AstStatement
    {
        public AstDeclaration(Token name, AstType type, AstExpression value)
        {
            Name = name;
            Type = type;
            Value = value;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Declaration;
        public Token Name { get; }
        public AstType Type { get; }
        public AstExpression Value { get; }
    }
}
