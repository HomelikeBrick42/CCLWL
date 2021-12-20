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

        public AstType InnerType { get; }
        public long Count { get; }

        public override AstType Clone()
        {
            return new AstArrayType(InnerType, Count);
        }
    }
}