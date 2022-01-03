using System.Collections.Generic;
using System.Linq;

namespace CCLWL.Syntax
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
            if (other.TypeKind == AstTypeKind.Function)
            {
                var function = (AstFunctionType) other;
                if (Parameters.Count() != function.Parameters.Count())
                    return false;

                var funcA = Parameters.GetEnumerator();
                var funcB = function.Parameters.GetEnumerator();
                
                while (true)
                {
                    if (!funcA.MoveNext())
                        break;
                    if (!funcB.MoveNext())
                        break;
                    var paramA = funcA.Current;
                    var paramB = funcB.Current;
                    if (!paramA.Type.Matches(paramB.Type))
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
