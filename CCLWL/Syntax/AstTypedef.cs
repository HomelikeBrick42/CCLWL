namespace CCLWL.Syntax
{
    public sealed class AstTypedef : AstStatement
    {
        public AstTypedef(string name, AstType type)
        {
            Name = name;
            Type = type;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Typedef;
        public string Name { get; }
        public AstType Type { get; }
    }
}