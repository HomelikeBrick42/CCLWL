using System;
using System.Collections.Generic;

namespace CCLWL
{
    public sealed class Parser
    {
        public Parser(string filepath)
        {
            _lexer = new Lexer(filepath);
            _current = _lexer.NextToken();
        }
        
        private Lexer _lexer;
        private Token _current;

        private Token NextToken()
        {
            var current = _current;
            _current = _lexer.NextToken();
            return current;
        }

        private Token ExpectToken(TokenKind kind)
        {
            var current = NextToken();
            if (current.Kind != kind)
                throw new CompileError($"Expected '{kind}', got '{current.Kind}'", current.Position);
            return current;
        }

        public AstFile Parse()
        {
            var statements = new List<AstStatement>();
            while (_current.Kind != TokenKind.EndOfFile)
                statements.Add(ParseStatement());
            return new AstFile(statements);
        }

        private AstStatement ParseStatement()
        {
            throw new NotImplementedException();
        }

        private AstExpression ParseExpression()
        {
            throw new NotImplementedException();
        }
    }
}
