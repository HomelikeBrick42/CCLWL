using System;
using System.Collections.Generic;
using System.IO;

namespace CCLWL
{
    public sealed class Lexer
    {
        public static readonly Dictionary<char, Dictionary<char, TokenKind>> DoubleChars = new()
        {
            {'+', new Dictionary<char, TokenKind> {{'=', TokenKind.PlusEquals}}},
            {'-', new Dictionary<char, TokenKind> {{'=', TokenKind.MinusEquals}}},
            {'*', new Dictionary<char, TokenKind> {{'=', TokenKind.AsteriskEquals}}},
            {'/', new Dictionary<char, TokenKind> {{'=', TokenKind.SlashEquals}}},
            {'=', new Dictionary<char, TokenKind> {{'=', TokenKind.EqualsEquals}}},
            {'<', new Dictionary<char, TokenKind> {{'=', TokenKind.LessThanEquals}}},
            {'>', new Dictionary<char, TokenKind> {{'=', TokenKind.GreaterThanEquals}}}
        };

        public static readonly Dictionary<char, TokenKind> SingleChars = new()
        {
            {',', TokenKind.Comma},
            {';', TokenKind.Semicolon},
            {'(', TokenKind.OpenParenthesis},
            {')', TokenKind.CloseParenthesis},
            {'[', TokenKind.OpenBracket},
            {']', TokenKind.CloseBracket},
            {'{', TokenKind.OpenBrace},
            {'}', TokenKind.CloseBrace},

            {'+', TokenKind.Plus},
            {'-', TokenKind.Minus},
            {'*', TokenKind.Asterisk},
            {'/', TokenKind.Slash},
            {'=', TokenKind.Equals},
            {'<', TokenKind.LessThan},
            {'>', TokenKind.GreaterThan}
        };

        public static readonly Dictionary<string, TokenKind> Keywords = new()
        {
            {"typedef", TokenKind.TypedefKeyword},
            {"distinct", TokenKind.DistinctKeyword},
            {"if", TokenKind.IfKeyword},
            {"else", TokenKind.ElseKeyword},
            {"while", TokenKind.WhileKeyword},
            {"do", TokenKind.DoKeyword},
            {"return", TokenKind.ReturnKeyword}
        };

        private int _column;
        private int _line;
        private int _position;

        public Lexer(string filepath)
        {
            Filepath = filepath;
            Source = File.ReadAllText(filepath);
            _position = 0;
            _line = 1;
            _column = 1;
        }

        public string Filepath { get; }
        public string Source { get; }

        private char Current => _position < Source.Length ? Source[_position] : '\0';

        private char NextChar()
        {
            var current = Current;
            _position++;
            _column++;
            if (current == '\n')
            {
                _line++;
                _column = 1;
            }

            return current;
        }

        public Token NextToken()
        {
            start:
            var startPos = new SourcePosition(Filepath, Source, _position, _line, _column);

            switch (Current)
            {
                case '\0':
                    return new Token(TokenKind.EndOfFile, startPos, _position - startPos.Position);

                case >= '0' and <= '9':
                {
                    long intValue = 0;
                    long intBase = 10;
                    if (Current == '0')
                    {
                        NextChar();
                        intBase = Current switch
                        {
                            'b' => 2,
                            'o' => 8,
                            'd' => 10,
                            'x' => 16,
                            _ => intBase
                        };
                    }

                    while (Current is >= '0' and <= '9' or >= 'A' and <= 'Z' or >= 'a' and <= 'z' or '_')
                    {
                        long value;
                        switch (Current)
                        {
                            case >= '0' and <= '9':
                                value = Current - '0';
                                break;
                            case >= 'A' and <= 'Z':
                                value = Current - 'A';
                                break;
                            case >= 'a' and <= 'z':
                                value = Current - 'a';
                                break;
                            case '_':
                                NextChar();
                                continue;
                            default:
                                throw new InvalidOperationException();
                        }

                        if (value >= intBase)
                        {
                            var pos = new SourcePosition(Filepath, Source, _position, _line, _column);
                            throw new CompileError($"Digit '{NextChar()}' too big for base '{intBase}'", pos);
                        }

                        intValue *= intBase;
                        intValue += value;

                        NextChar();
                    }

                    return new Token(TokenKind.Integer, startPos, _position - startPos.Position, intValue);
                }
            }

            if (char.IsLetter(Current) || char.IsDigit(Current) || Current == '_')
            {
                var name = "";
                while (char.IsLetter(Current) || char.IsDigit(Current) || Current == '_') name += NextChar();
                return Keywords.ContainsKey(name)
                    ? new Token(Keywords[name], startPos, _position - startPos.Position)
                    : new Token(TokenKind.Name, startPos, _position - startPos.Position, name);
            }

            var character = NextChar();

            if (char.IsWhiteSpace(character))
                goto start;

            if (character == '/' && Current == '/')
            {
                while (Current != '\n' && Current != '\0')
                    NextChar();
                goto start;
            }

            if (DoubleChars.ContainsKey(character) && DoubleChars[character].ContainsKey(Current))
            {
                var second = NextChar();
                return new Token(DoubleChars[character][second], startPos, _position - startPos.Position);
            }

            if (SingleChars.ContainsKey(character))
                return new Token(SingleChars[character], startPos, _position - startPos.Position);

            throw new CompileError($"Unknown character '{character}'", startPos);
        }
    }
}