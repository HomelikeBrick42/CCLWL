namespace CCLWL
{
    public sealed class AstIntegerType : AstType
    {
        public AstIntegerType(int size, bool signed)
        {
            Size = size;
            Signed = signed;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Integer;

        public int Size { get; }
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