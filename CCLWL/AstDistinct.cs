namespace CCLWL
{
    public sealed class AstDistinct : AstStatement
    {
        public AstDistinct(string name, AstType type)
        {
            Name = name;
            Type = type;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Distinct;
        public string Name { get; }
        public AstType Type { get; }
    }
}