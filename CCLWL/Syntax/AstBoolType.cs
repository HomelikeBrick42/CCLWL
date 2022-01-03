namespace CCLWL.Syntax
{
    public sealed class AstBoolType : AstType
    {
        public override AstTypeKind TypeKind => AstTypeKind.Bool;
        public override long Size => 8;

        public override AstType Clone()
        {
            return new AstBoolType();
        }

        public override bool Matches(AstType other)
        {
            return this == other;
        }
    }
}