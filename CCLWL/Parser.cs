using System;
using System.Collections.Generic;

namespace CCLWL
{
    public sealed class Parser
    {
        private int _position;
        private List<Dictionary<string, AstDeclaration>> _scopes;

        private List<Dictionary<string, AstType>> _types;

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

            _types = new List<Dictionary<string, AstType>>
            {
                new()
                {
                    {"void", new AstVoidType()},
                    {"int", new AstIntegerType(4)},
                    {"char", new AstIntegerType(1)}
                }
            };

            _scopes = new List<Dictionary<string, AstDeclaration>> {new()};
        }

        private List<Token> Tokens { get; }

        private Token Current => _position < Tokens.Count ? Tokens[_position] : Tokens[^1];

        private AstType GetType(string name)
        {
            for (var i = _types.Count - 1; i >= 0; i--)
                if (_types[i].TryGetValue(name, out var type))
                    return type;
            return null;
        }

        private AstDeclaration GetDeclaration(string name)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
                if (_scopes[i].TryGetValue(name, out var declaration))
                    return declaration;
            return null;
        }

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

        public AstFile Parse()
        {
            var statements = new List<AstStatement>();
            while (Current.Kind != TokenKind.EndOfFile)
                statements.Add(ParseStatement());
            return new AstFile(statements);
        }

        private AstStatement ParseStatement()
        {
            if (Current.Kind == TokenKind.Name && GetType((string) Current.Value) != null)
            {
                var firstTokenForErrorMessage = Current;
                Token name = null;
                var type = ParseDecl(null, ref name);
                if (name == null)
                    throw new CompileError("Expected name in declaration", firstTokenForErrorMessage.Position);
                if (type.TypeKind == AstTypeKind.Function && Current.Kind == TokenKind.OpenBrace)
                {
                    var declaration = new AstDeclaration(name, type, null);
                    if (!_scopes[^1].TryAdd((string) name.Value, declaration))
                        throw new CompileError($"'{(string) name.Value}' is already declared in this scope",
                            name.Position);

                    if (_scopes.Count > 1)
                        throw new CompileError("Cannot define nested functions", Current.Position);

                    var parameters = new Dictionary<string, AstDeclaration>();
                    foreach (var parameter in ((AstFunctionType) type).Parameters)
                        parameters.Add((string) parameter.Name.Value, parameter);

                    var scope = ParseScope(parameters);
                    return new AstFunction((string) name.Value, type, scope);
                }
                else
                {
                    AstExpression value = null;
                    if (Current.Kind == TokenKind.Equals)
                    {
                        ExpectToken(TokenKind.Equals);
                        value = ParseExpression(type);
                    }

                    var declaration = new AstDeclaration(name, type, value);
                    if (!_scopes[^1].TryAdd((string) name.Value, declaration))
                        throw new CompileError($"'{(string) name.Value}' is already declared in this scope",
                            name.Position);

                    ExpectToken(TokenKind.Semicolon);
                    return declaration;
                }
            }

            switch (Current.Kind)
            {
                case TokenKind.TypedefKeyword:
                {
                    var typedefKeyword = ExpectToken(TokenKind.TypedefKeyword);
                    Token name = null;
                    var type = ParseDecl(null, ref name);
                    if (name == null)
                        throw new CompileError("Expected name for typedef", typedefKeyword.Position);
                    ExpectToken(TokenKind.Semicolon);
                    var nameString = (string) name.Value;
                    if (!_types[^1].TryAdd(nameString, type))
                        throw new CompileError($"type '{(string) name.Value}' is already defined in this scope",
                            name.Position);
                    return new AstTypedef(nameString, type);
                }

                case TokenKind.DistinctKeyword:
                {
                    var distinctKeyword = ExpectToken(TokenKind.DistinctKeyword);
                    Token name = null;
                    var type = ParseDecl(null, ref name);
                    if (name == null)
                        throw new CompileError("Expected name for distinct", distinctKeyword.Position);
                    ExpectToken(TokenKind.Semicolon);
                    var nameString = (string) name.Value;
                    var typeClone = type.Clone();
                    if (!_types[^1].TryAdd(nameString, typeClone))
                        throw new CompileError($"type '{(string) name.Value}' is already defined in this scope",
                            name.Position);
                    return new AstDistinct(nameString, typeClone);
                }

                default:
                {
                    var expression = ParseExpression(null);
                    ExpectToken(TokenKind.Semicolon);
                    return new AstExpressionStatement(expression);
                }
            }
        }

        private AstScope ParseScope(Dictionary<string, AstDeclaration> procedureParameters = null)
        {
            _scopes.Add(procedureParameters ?? new Dictionary<string, AstDeclaration>());
            _types.Add(new Dictionary<string, AstType>());
            ExpectToken(TokenKind.OpenBrace);
            var statements = new List<AstStatement>();
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
                statements.Add(ParseStatement());
            ExpectToken(TokenKind.CloseBrace);
            _types.RemoveAt(_types.Count - 1);
            _scopes.RemoveAt(_scopes.Count - 1);
            return new AstScope(statements);
        }

        private AstType ParseDecl(AstType baseType, ref Token name)
        {
            if (name == null && Current.Kind == TokenKind.Name && GetType((string) Current.Value) != null)
                baseType = GetType((string) ExpectToken(TokenKind.Name).Value);

            while (Current.Kind == TokenKind.Asterisk)
            {
                ExpectToken(TokenKind.Asterisk);
                baseType = new AstPointerType(baseType);
            }

            var depth = 0;
            var todoPositions = new Stack<int>();
            while (Current.Kind != TokenKind.Semicolon && Current.Kind != TokenKind.Equals &&
                   Current.Kind != TokenKind.EndOfFile)
            {
                if (depth == 0 && Current.Kind is TokenKind.Name or TokenKind.OpenBracket or TokenKind.OpenParenthesis)
                    todoPositions.Push(_position);
                if (Current.Kind == TokenKind.OpenParenthesis)
                    depth++;
                if (Current.Kind == TokenKind.CloseParenthesis)
                    depth--;
                if (depth < 0)
                    break;
                if (depth == 0 && Current.Kind is TokenKind.Comma or TokenKind.OpenBrace)
                    break;
                NextToken();
            }

            var endPos = _position;

            while (todoPositions.Count > 0)
            {
                _position = todoPositions.Pop();
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
                        if (Current.Kind == TokenKind.Name && GetType((string) Current.Value) != null ||
                            Current.Kind == TokenKind.CloseParenthesis)
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
                                parameters.Add(new AstDeclaration(parameterName, parameterType, null));
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

        private AstExpression ParseExpression(AstType suggestedType)
        {
            return ParseBinaryExpression(suggestedType, 0);
        }

        private AstExpression ParsePrimaryExpression(AstType suggestedType)
        {
            switch (Current.Kind)
            {
                case TokenKind.Integer:
                {
                    var token = ExpectToken(TokenKind.Integer);
                    AstIntegerType type;
                    if (suggestedType != null && suggestedType.TypeKind == AstTypeKind.Integer)
                        type = (AstIntegerType) suggestedType;
                    else
                        type = (AstIntegerType) GetType("int");
                    return new AstInteger(type, (long) token.Value);
                }

                case TokenKind.Name:
                {
                    var token = ExpectToken(TokenKind.Name);
                    var name = (string) token.Value;
                    var declaration = GetDeclaration(name);
                    if (declaration == null)
                        throw new CompileError($"Unable to find name '{name}'", token.Position);
                    return new AstName(declaration, name);
                }

                default:
                    throw new CompileError($"Unexpected '{Current.Kind}' in expression", Current.Position);
            }
        }

        private static int GetUnaryPrecedence(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Plus => 3,
                TokenKind.Minus => 3,
                _ => 0
            };
        }

        private static int GetBinaryPrecedence(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Asterisk => 2,
                TokenKind.Slash => 2,
                TokenKind.Plus => 1,
                TokenKind.Minus => 1,
                _ => 0
            };
        }

        private static AstType GetUnaryResultType(TokenKind @operator, AstType operand)
        {
            // TODO: Better type checking
            if (operand.TypeKind == AstTypeKind.Integer)
                return operand;
            return null;
        }

        private static AstType GetBinaryResultType(TokenKind @operator, AstType left, AstType right)
        {
            // TODO: Better type checking
            if (left.TypeKind == AstTypeKind.Integer && left == right)
                return left;
            return null;
        }

        private AstExpression ParseBinaryExpression(AstType suggestedType, int parentPrecedence)
        {
            AstExpression left;
            var unaryPrecedence = GetUnaryPrecedence(Current.Kind);
            if (unaryPrecedence > 0)
            {
                var @operator = NextToken();
                var operand = ParseBinaryExpression(suggestedType, parentPrecedence);
                var resultType = GetUnaryResultType(@operator.Kind, operand.Type);
                if (resultType == null)
                    throw new CompileError("Unable to find unary operator for given types", @operator.Position);
                left = new AstUnary(resultType, @operator, operand);
            }
            else
            {
                left = ParsePrimaryExpression(suggestedType);
            }

            while (true)
            {
                var binaryPrecedence = GetBinaryPrecedence(Current.Kind);
                if (binaryPrecedence <= parentPrecedence)
                    break;

                var @operator = NextToken();
                var right = ParseBinaryExpression(suggestedType ?? left.Type, binaryPrecedence);
                var resultType = GetBinaryResultType(@operator.Kind, left.Type, right.Type);
                if (resultType == null)
                    throw new CompileError("Unable to find binary operator for given types", @operator.Position);
                left = new AstBinary(resultType, left, @operator, right);
            }

            return left;
        }
    }
}
