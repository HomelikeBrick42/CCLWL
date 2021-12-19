namespace CCLWL
{
    public sealed class AstVoidType : AstType
    {
        public AstVoidType()
        {
        }
        
        public override AstTypeKind TypeKind => AstTypeKind.Void;
    }
}
