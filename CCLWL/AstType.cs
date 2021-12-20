namespace CCLWL
{
    public abstract class AstType : AstNode
    {
        public override AstKind Kind => AstKind.Type;
        public abstract AstTypeKind TypeKind { get; }
        public abstract AstType Clone();
    }
}