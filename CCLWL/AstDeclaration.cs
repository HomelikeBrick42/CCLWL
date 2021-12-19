namespace CCLWL
{
    public sealed class AstDeclaration : AstStatement
    {
        public AstDeclaration(Token name, AstType type)
        {
            Name = name;
            Type = type;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Declaration;
        public Token Name { get; }
        public AstType Type { get; }
    }
}
