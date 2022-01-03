using System;
using System.Collections.Generic;

namespace CCLWL.Syntax
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
                    {"bool", new AstBoolType()},
                    {"int64", new AstIntegerType(64, true)},
                    {"int32", new AstIntegerType(32, true)},
                    {"int16", new AstIntegerType(16, true)},
                    {"int8", new AstIntegerType(8, true)}
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
                statements.Add(ParseStatement(null));
            return new AstFile(statements);
        }

        private AstStatement ParseStatement(AstFunctionType currentFunction)
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

                    var scope = ParseScope((AstFunctionType) type, true, parameters);
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

                case TokenKind.IfKeyword:
                {
                    var ifKeyword = ExpectToken(TokenKind.IfKeyword);
                    var condition = ParseExpression(GetType("bool"));
                    if (condition.Type.TypeKind != AstTypeKind.Bool)
                        throw new CompileError("Expected bool type for `if` condition", ifKeyword.Position);
                    
                    var thenScope = ParseScope(currentFunction);
                    AstScope elseScope = null;
                    if (Current.Kind == TokenKind.ElseKeyword)
                    {
                        ExpectToken(TokenKind.ElseKeyword);
                        elseScope = ParseScope(currentFunction);
                    }

                    return new AstIf(condition, thenScope, elseScope);
                }

                case TokenKind.WhileKeyword:
                {
                    var whileKeyword = ExpectToken(TokenKind.WhileKeyword);
                    var condition = ParseExpression(GetType("bool"));
                    if (condition.Type.TypeKind != AstTypeKind.Bool)
                        throw new CompileError("Expected bool type for `while` condition", whileKeyword.Position);
                    var body = ParseScope(currentFunction);
                    return new AstWhile(condition, body);
                }

                case TokenKind.ReturnKeyword:
                {
                    var returnKeyword = ExpectToken(TokenKind.ReturnKeyword);
                    AstExpression expression = null;
                    if (Current.Kind != TokenKind.Semicolon)
                        expression = ParseExpression(currentFunction.ReturnType);
                    ExpectToken(TokenKind.Semicolon);
                    if (expression == null && currentFunction.ReturnType.TypeKind == AstTypeKind.Void ||
                        expression != null && expression.Type != currentFunction.ReturnType)
                        throw new CompileError("Function return type and return expression type do not match",
                            returnKeyword.Position);
                    return new AstReturn(expression);
                }

                case TokenKind.VarKeyword:
                {
                    var varKeyword = ExpectToken(TokenKind.VarKeyword);
                    var name = ExpectToken(TokenKind.Name);
                    ExpectToken(TokenKind.Equals);
                    var value = ParseExpression(null);
                    ExpectToken(TokenKind.Semicolon);
                    var declaration = new AstDeclaration(name, value.Type, value);
                    if (!_scopes[^1].TryAdd((string) name.Value, declaration))
                        throw new CompileError($"'{(string) name.Value}' is already declared in this scope",
                            name.Position);
                    return declaration;
                }

                default:
                {
                    var expression = ParseExpression(null);
                    if (IsAssignmentOperator(Current.Kind))
                    {
                        var @operator = NextToken();
                        if (!IsAssignable(expression))
                            throw new CompileError("Expression is not assignable", @operator.Position);
                        var value = ParseExpression(expression.Type);
                        if (@operator.Kind != TokenKind.Equals)
                        {
                            var resultType = GetBinaryResultType(AssignmentOperatorToBinaryOperator(@operator.Kind),
                                expression.Type, value.Type);
                            if (resultType == null)
                                throw new CompileError("Unable to find binary operator for given types",
                                    @operator.Position);
                        }
                        else if (!expression.Type.Matches(value.Type))
                        {
                            throw new CompileError("Value type does not match operand type", @operator.Position);
                        }

                        ExpectToken(TokenKind.Semicolon);
                        return new AstAssignment(expression, @operator, value);
                    }

                    ExpectToken(TokenKind.Semicolon);
                    return new AstExpressionStatement(expression);
                }
            }
        }

        private bool IsAssignable(AstExpression expression)
        {
            switch (expression.ExpressionKind)
            {
                case AstExpressionKind.Name:
                    return true;
                case AstExpressionKind.Unary:
                    return ((AstUnary) expression).Operator.Kind == TokenKind.Asterisk;
                default:
                    return false;
            }
        }

        private TokenKind AssignmentOperatorToBinaryOperator(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Equals => TokenKind.Equals,
                TokenKind.PlusEquals => TokenKind.Plus,
                TokenKind.MinusEquals => TokenKind.Minus,
                TokenKind.AsteriskEquals => TokenKind.Asterisk,
                TokenKind.SlashEquals => TokenKind.Slash,
                _ => throw new InvalidOperationException()
            };
        }

        private bool IsAssignmentOperator(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Equals => true,
                TokenKind.PlusEquals => true,
                TokenKind.MinusEquals => true,
                TokenKind.AsteriskEquals => true,
                TokenKind.SlashEquals => true,
                _ => false
            };
        }

        private AstScope ParseScope(AstFunctionType currentFunction, bool mainFunctionScope = false,
            Dictionary<string, AstDeclaration> procedureParameters = null)
        {
            _scopes.Add(procedureParameters ?? new Dictionary<string, AstDeclaration>());
            _types.Add(new Dictionary<string, AstType>());
            ExpectToken(TokenKind.OpenBrace);
            var statements = new List<AstStatement>();
            while (Current.Kind != TokenKind.CloseBrace && Current.Kind != TokenKind.EndOfFile)
                statements.Add(ParseStatement(currentFunction));
            var closeBrace = ExpectToken(TokenKind.CloseBrace);
            _types.RemoveAt(_types.Count - 1);
            _scopes.RemoveAt(_scopes.Count - 1);
            var scope = new AstScope(statements);
            if (mainFunctionScope && !ReturnsInAllCodePaths(scope))
            {
                if (currentFunction.ReturnType.TypeKind == AstTypeKind.Void)
                    statements.Add(new AstReturn(null));
                else
                    throw new CompileError("Function does not return in all code paths", closeBrace.Position);
            }

            return scope;
        }

        private AstType ParseDecl(AstType baseType, ref Token name)
        {
            if (name == null && Current.Kind == TokenKind.Name && GetType((string)Current.Value) != null)
            {
                var token = ExpectToken(TokenKind.Name);
                var nameString = (string) token.Value;
                baseType = GetType(nameString);
                if (baseType == null)
                    throw new CompileError($"Unable to find type '{nameString}'", token.Position);
            }

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

        private bool ReturnsInAllCodePaths(AstScope scope)
        {
            foreach (var statement in scope.Statements)
                switch (statement.StatementKind)
                {
                    case AstStatementKind.If:
                        var iff = (AstIf) statement;
                        if (iff.ElseStatement != null)
                            if (ReturnsInAllCodePaths((AstScope) iff.ThenStatement) &&
                                ReturnsInAllCodePaths((AstScope) iff.ElseStatement))
                                return true;
                        break;

                    case AstStatementKind.Scope:
                        if (ReturnsInAllCodePaths((AstScope) statement))
                            return true;
                        break;

                    case AstStatementKind.Return:
                        return true;
                }

            return false;
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
                        throw new CompileError("Ambiguous type for integer literal", token.Position);
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

                case TokenKind.OpenParenthesis:
                {
                    var openParenthesis = ExpectToken(TokenKind.OpenParenthesis);
                    if (Current.Kind == TokenKind.Name && GetType((string) Current.Value) != null)
                    {
                        Token name = null;
                        var type = ParseDecl(null, ref name);
                        if (name != null)
                            throw new CompileError("Unexpected name in cast", name.Position);
                        ExpectToken(TokenKind.CloseParenthesis);
                        var expression = ParseBinaryExpression(type, 100000);
                        // TODO: Disallow struct/union casts
                        if (type.TypeKind != expression.Type.TypeKind && type.Size != expression.Type.Size)
                            throw new CompileError("Invalid cast", openParenthesis.Position);
                        return new AstCast(type, expression);
                    }
                    else
                    {
                        var expression = ParseExpression(suggestedType);
                        ExpectToken(TokenKind.CloseParenthesis);
                        return expression;
                    }
                }

                default:
                    throw new CompileError($"Unexpected '{Current.Kind}' in expression", Current.Position);
            }
        }

        private static int GetUnaryPrecedence(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Plus => 5,
                TokenKind.Minus => 5,
                TokenKind.Ampersand => 5,
                TokenKind.Asterisk => 5,
                _ => 0
            };
        }

        private static int GetBinaryPrecedence(TokenKind kind)
        {
            return kind switch
            {
                TokenKind.Asterisk => 4,
                TokenKind.Slash => 4,
                TokenKind.Plus => 3,
                TokenKind.Minus => 3,
                TokenKind.LessThan => 2,
                TokenKind.LessThanEquals => 2,
                TokenKind.GreaterThan => 2,
                TokenKind.GreaterThanEquals => 2,
                TokenKind.EqualsEquals => 1,
                _ => 0
            };
        }

        private AstType GetUnaryResultType(TokenKind @operator, AstType operand)
        {
            switch (@operator)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                    if (operand.TypeKind == AstTypeKind.Integer)
                        return operand;
                    return null;

                case TokenKind.Ampersand:
                    return new AstPointerType(operand);

                case TokenKind.Asterisk:
                    if (operand.TypeKind == AstTypeKind.Pointer)
                        return ((AstPointerType) operand).PointedTo;
                    return null;

                default:
                    throw new InvalidOperationException();
            }
        }

        private AstType GetBinaryResultType(TokenKind @operator, AstType left, AstType right)
        {
            switch (@operator)
            {
                case TokenKind.Plus:
                case TokenKind.Minus:
                case TokenKind.Asterisk:
                case TokenKind.Slash:
                    if (left.TypeKind == AstTypeKind.Integer && left.Matches(right))
                        return left;
                    return null;

                case TokenKind.EqualsEquals:
                    if (left.Matches(right))
                        return GetType("bool");
                    return null;

                case TokenKind.LessThan:
                case TokenKind.LessThanEquals:
                case TokenKind.GreaterThan:
                case TokenKind.GreaterThanEquals:
                    if (left.TypeKind == AstTypeKind.Integer && left.Matches(right))
                        return GetType("bool");
                    return null;

                default:
                    throw new InvalidOperationException();
            }
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
                while (Current.Kind == TokenKind.OpenParenthesis)
                {
                    if (left.Type.TypeKind != AstTypeKind.Function)
                        throw new CompileError("Cannot call a non-function", Current.Position);
                    var functionType = (AstFunctionType) left.Type;

                    var arguments = new List<AstExpression>();
                    var argIndex = 0;
                    var errorToken = ExpectToken(TokenKind.OpenParenthesis);
                    foreach (var parameter in functionType.Parameters)
                    {
                        if (argIndex > 0)
                            errorToken = ExpectToken(TokenKind.Comma);
                        var argument = ParseExpression(parameter.Type);
                        if (argument.Type != parameter.Type)
                            throw new CompileError($"Argument {argIndex} type does not match paramter type",
                                errorToken.Position);
                        arguments.Add(argument);
                        argIndex++;
                    }

                    ExpectToken(TokenKind.CloseParenthesis);
                    left = new AstFunctionCall(functionType, arguments);
                }

                var binaryPrecedence = GetBinaryPrecedence(Current.Kind);
                if (binaryPrecedence <= parentPrecedence)
                    break;

                var @operator = NextToken();
                var right = ParseBinaryExpression(left.Type, binaryPrecedence);
                var resultType = GetBinaryResultType(@operator.Kind, left.Type, right.Type);
                if (resultType == null)
                    throw new CompileError("Unable to find binary operator for given types", @operator.Position);
                left = new AstBinary(resultType, left, @operator, right);
            }

            return left;
        }
    }
}
