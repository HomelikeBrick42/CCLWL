namespace CCLWL
{
    public sealed class AstArrayType : AstType
    {
        public AstArrayType(AstType innerType, long count)
        {
            InnerType = innerType;
            Count = count;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Array;
        public override long Size => Count * InnerType.Size;

        public AstType InnerType { get; }
        public long Count { get; }

        public override AstType Clone()
        {
            return new AstArrayType(InnerType, Count);
        }

        public override bool Matches(AstType other)
        {
            if (other.TypeKind == AstTypeKind.Array)
                return InnerType.Matches(((AstArrayType) other).InnerType);
            return false;
        }
    }
}