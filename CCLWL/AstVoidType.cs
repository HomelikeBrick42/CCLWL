namespace CCLWL
{
    public sealed class AstVoidType : AstType
    {
        public override AstTypeKind TypeKind => AstTypeKind.Void;
        public override long Size => 0;

        public override AstType Clone()
        {
            return new AstVoidType();
        }

        public override bool Matches(AstType other)
        {
            return this == other;
        }
    }
}