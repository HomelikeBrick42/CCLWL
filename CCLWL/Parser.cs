using System;
using System.Collections.Generic;
using System.Linq;

namespace CCLWL
{
    public sealed class Parser
    {
        public Parser(string filepath)
        {
            var lexer = new Lexer(filepath);
            var tokens = new List<Token>();

            while (true)
            {
                var token = lexer.NextToken();
                tokens.Add(token);
                if (token.Kind == TokenKind.EndOfFile)
                    break;
            }

            Tokens = tokens;

            _position = 0;

            _types = new Dictionary<string, AstType>();
            _types.Add("int", new AstIntegerType(4));
            _types.Add("char", new AstIntegerType(1));
        }

        private List<Token> Tokens { get; }
        private int _position;

        private Dictionary<string, AstType> _types;

        private Token Current => _position < Tokens.Count ? Tokens[_position] : Tokens[^1];

        private Token NextToken()
        {
            var current = Current;
            _position++;
            return current;
        }

        private Token ExpectToken(TokenKind kind)
        {
            var current = NextToken();
            if (current.Kind != kind)
                throw new CompileError($"Expected '{kind}', got '{current.Kind}'", current.Position);
            return current;
        }

        private bool NameIsType(Token token)
        {
            if (token.Kind != TokenKind.Name)
                return false;
            var name = (string) token.Value;
            return _types.ContainsKey(name);
        }

        public AstFile Parse()
        {
            var statements = new List<AstStatement>();
            while (Current.Kind != TokenKind.EndOfFile)
                statements.Add(ParseStatement());
            return new AstFile(statements);
        }

        private AstStatement ParseStatement()
        {
            if (NameIsType(Current))
            {
                var firstTokenForErrorMessage = Current;
                Token name = null;
                var type = ParseDecl(null, ref name);
                if (name == null)
                    throw new CompileError("Expected name in declaration", firstTokenForErrorMessage.Position);
                ExpectToken(TokenKind.Semicolon);
                return new AstDeclaration(name, type);
            }
            else
            {
                var expression = ParseExpression();
                ExpectToken(TokenKind.Semicolon);
                return new AstExpressionStatement(expression);
            }
        }

        private AstType ParseDecl(AstType baseType, ref Token name)
        {
            if (name == null && NameIsType(Current))
                baseType = _types[(string) ExpectToken(TokenKind.Name).Value];

            while (Current.Kind == TokenKind.Asterisk)
            {
                ExpectToken(TokenKind.Asterisk);
                baseType = new AstPointerType(baseType);
            }

            var depth = 0;
            var todoPositions = new List<int>();
            while (Current.Kind != TokenKind.Semicolon && Current.Kind != TokenKind.Equals &&
                   Current.Kind != TokenKind.EndOfFile)
            {
                if (depth == 0 && Current.Kind is TokenKind.Name or TokenKind.OpenBracket or TokenKind.OpenParenthesis)
                    todoPositions.Add(_position);
                if (Current.Kind == TokenKind.OpenParenthesis)
                    depth++;
                if (Current.Kind == TokenKind.CloseParenthesis)
                    depth--;
                if (depth < 0)
                    break;
                if (depth == 0 && Current.Kind == TokenKind.Comma)
                    break;
                NextToken();
            }

            var endPos = _position;

            while (todoPositions.Count > 0)
            {
                _position = todoPositions[^1];
                todoPositions.RemoveAt(todoPositions.Count - 1);
                switch (Current.Kind)
                {
                    case TokenKind.OpenBracket:
                        ExpectToken(TokenKind.OpenBracket);
                        var count = (long) ExpectToken(TokenKind.Integer).Value;
                        ExpectToken(TokenKind.CloseBracket);
                        baseType = new AstArrayType(baseType, count);
                        break;

                    case TokenKind.OpenParenthesis:
                        var errorToken = ExpectToken(TokenKind.OpenParenthesis);
                        if (NameIsType(Current))
                        {
                            var parameters = new List<AstDeclaration>();
                            var expectComma = false;
                            while (Current.Kind != TokenKind.CloseParenthesis && Current.Kind != TokenKind.EndOfFile)
                            {
                                if (expectComma)
                                    errorToken = ExpectToken(TokenKind.Comma);
                                else
                                    expectComma = true;

                                Token parameterName = null;
                                var parameterType = ParseDecl(null, ref parameterName);
                                if (parameterName == null)
                                    throw new CompileError("Expected name in function parameter", errorToken.Position);
                                parameters.Add(new AstDeclaration(parameterName, parameterType));
                            }
                            baseType = new AstFunctionType(baseType, parameters);
                        }
                        else
                        {
                            Token maybeName = null;
                            baseType = ParseDecl(baseType, ref maybeName);
                            name = maybeName;
                        }
                        ExpectToken(TokenKind.CloseParenthesis);
                        break;

                    case TokenKind.Name:
                        name = ExpectToken(TokenKind.Name);
                        break;

                    default:
                        throw new InvalidOperationException();
                }
            }

            _position = endPos;
            return baseType;
        }

        private AstExpression ParseExpression()
        {
            throw new NotImplementedException();
        }
    }
}
