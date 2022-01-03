namespace CCLWL.Syntax
{
    public sealed class AstFunction : AstStatement
    {
        public AstFunction(string name, AstType functionType, AstScope body)
        {
            Name = name;
            FunctionType = functionType;
            Body = body;
        }

        public override AstStatementKind StatementKind => AstStatementKind.Function;
        public string Name { get; }
        public AstType FunctionType { get; }
        public AstScope Body { get; }
    }
}