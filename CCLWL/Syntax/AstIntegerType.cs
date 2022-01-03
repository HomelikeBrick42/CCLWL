namespace CCLWL.Syntax
{
    public sealed class AstIntegerType : AstType
    {
        public AstIntegerType(long size, bool signed)
        {
            Size = size;
            Signed = signed;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Integer;

        public override long Size { get; }
        public bool Signed { get; }

        public override AstType Clone()
        {
            return new AstIntegerType(Size, Signed);
        }

        public override bool Matches(AstType other)
        {
            return this == other;
        }
    }
}