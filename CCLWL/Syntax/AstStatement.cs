namespace CCLWL.Syntax
{
    public abstract class AstStatement : AstNode
    {
        public override AstKind Kind => AstKind.Statement;
        public abstract AstStatementKind StatementKind { get; }
    }
}