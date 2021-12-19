namespace CCLWL
{
    public abstract class AstStatement: AstNode
    {
        public override AstKind Kind => AstKind.Statement;
        public abstract AstStatementKind StatementKind { get; }
    }
}
