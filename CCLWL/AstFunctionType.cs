using System.Collections.Generic;

namespace CCLWL
{
    public sealed class AstFunctionType : AstType
    {
        public AstFunctionType(AstType returnType, IEnumerable<AstDeclaration> parameters)
        {
            ReturnType = returnType;
            Parameters = parameters;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Function;
        public override long Size => 64;

        public AstType ReturnType { get; }
        public IEnumerable<AstDeclaration> Parameters { get; }

        public override AstType Clone()
        {
            return new AstFunctionType(ReturnType, Parameters);
        }

        public override bool Matches(AstType other)
        {
            return this == other;
        }
    }
}