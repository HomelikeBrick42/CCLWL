namespace CCLWL
{
    public sealed class AstVoidType : AstType
    {
        public override AstTypeKind TypeKind => AstTypeKind.Void;

        public override AstType Clone()
        {
            return new AstVoidType();
        }
    }
}