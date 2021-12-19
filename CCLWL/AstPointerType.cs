namespace CCLWL
{
    public sealed class AstPointerType : AstType
    {
        public AstPointerType(AstType pointedTo)
        {
            PointedTo = pointedTo;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Pointer;
        public AstType PointedTo { get; }
    }
}
