namespace CCLWL
{
    public sealed class AstBoolType : AstType
    {
        public override AstTypeKind TypeKind => AstTypeKind.Bool;

        public override AstType Clone()
        {
            return new AstBoolType();
        }
    }
}