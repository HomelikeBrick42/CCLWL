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

                var lexer = new Lexer(filepath);
                while (true)
                {
                    var token = lexer.NextToken();
                    Console.WriteLine(token.Kind);
                    if (token.Kind == TokenKind.EndOfFile)
                        break;
                }

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
    }
}
