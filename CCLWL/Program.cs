using System;

namespace CCLWL
{
    internal class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Usage: cclwl.exe <file>");
                    return 1;
                }

                var filepath = args[0];

                /*
                var lexer = new Lexer(filepath);
                while (true)
                {
                    var token = lexer.NextToken();
                    Console.WriteLine(token.Kind);
                    if (token.Kind == TokenKind.EndOfFile)
                        break;
                }
                */

                var parser = new Parser(filepath);
                var ast = parser.Parse();

                return 0;
            }
            catch (CompileError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"{e.Position.Filepath}:{e.Position.Line}:{e.Position.Column}: {e.Message}");
                return 1;
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(e);
                return 1;
            }
        }

        private static void PrintTypes(AstNode ast)
        {
            switch (ast.Kind)
            {
                case AstKind.File:
                {
                    var file = (AstFile) ast;
                    foreach (var statement in file.Statements)
                    {
                        PrintTypes(statement);
                        Console.WriteLine();
                    }

                    break;
                }

                case AstKind.Statement:
                {
                    var statement = (AstStatement) ast;
                    switch (statement.StatementKind)
                    {
                        case AstStatementKind.Declaration:
                        {
                            var declaration = (AstDeclaration) ast;
                            Console.Write($"declare {(string) declaration.Name.Value} as ");
                            PrintTypes(declaration.Type);
                            break;
                        }

                        case AstStatementKind.Expression:
                            throw new NotImplementedException();

                        default:
                            throw new InvalidOperationException();
                    }

                    break;
                }

                case AstKind.Expression:
                    throw new NotImplementedException();

                case AstKind.Type:
                {
                    var type = (AstType) ast;
                    switch (type.TypeKind)
                    {
                        case AstTypeKind.Integer:
                        {
                            var integer = (AstIntegerType) type;
                            Console.Write($"{integer.Size}-byte integer ");
                            break;
                        }

                        case AstTypeKind.Pointer:
                        {
                            var pointer = (AstPointerType) type;
                            Console.Write("pointer to ");
                            PrintTypes(pointer.PointedTo);
                            break;
                        }

                        case AstTypeKind.Array:
                        {
                            var array = (AstArrayType) type;
                            Console.Write($"array {array.Count} of ");
                            PrintTypes(array.InnerType);
                            break;
                        }

                        case AstTypeKind.Function:
                        {
                            var function = (AstFunctionType) type;
                            Console.Write("function ( ");
                            var useComma = false;
                            foreach (var decl in function.Parameters)
                            {
                                if (useComma)
                                    Console.Write(", ");
                                else
                                    useComma = true;
                                PrintTypes(decl.Type);
                            }

                            Console.Write(") returning ");
                            PrintTypes(function.ReturnType);
                            break;
                        }

                        case AstTypeKind.Void:
                        {
                            Console.Write("void ");
                            break;
                        }

                        default:
                            throw new InvalidOperationException();
                    }

                    break;
                }

                default:
                    throw new InvalidOperationException();
            }
        }
    }
}
