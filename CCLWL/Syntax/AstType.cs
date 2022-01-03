namespace CCLWL.Syntax
{
    public abstract class AstType : AstNode
    {
        public override AstKind Kind => AstKind.Type;
        public abstract AstTypeKind TypeKind { get; }
        public abstract long Size { get; }
        public abstract AstType Clone();
        public abstract bool Matches(AstType other);
    }
}