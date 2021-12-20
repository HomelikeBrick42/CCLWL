namespace CCLWL
{
    public sealed class AstPointerType : AstType
    {
        public AstPointerType(AstType pointedTo)
        {
            PointedTo = pointedTo;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Pointer;
        public override long Size => 64;

        public AstType PointedTo { get; }

        public override AstType Clone()
        {
            return new AstPointerType(PointedTo);
        }

        public override bool Matches(AstType other)
        {
            if (other.TypeKind == AstTypeKind.Pointer)
                return PointedTo.Matches(((AstPointerType) other).PointedTo);
            return false;
        }
    }
}