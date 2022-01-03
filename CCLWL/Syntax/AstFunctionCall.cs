using System.Collections.Generic;

namespace CCLWL.Syntax
{
    public sealed class AstFunctionCall : AstExpression
    {
        public AstFunctionCall(AstFunctionType functionType, IEnumerable<AstExpression> arguments)
        {
            FunctionType = functionType;
            Arguments = arguments;
        }

        public override AstExpressionKind ExpressionKind => AstExpressionKind.FunctionCall;
        public override AstType Type => FunctionType.ReturnType;
        public AstFunctionType FunctionType { get; }
        public IEnumerable<AstExpression> Arguments { get; }
    }
}