namespace CCLWL
{
    public enum TokenKind
    {
        EndOfFile,

        Name,
        Integer,

        TypedefKeyword,
        DistinctKeyword,
        IfKeyword,
        ElseKeyword,
        DoKeyword,
        ReturnKeyword,

        Comma,
        Semicolon,
        OpenParenthesis,
        CloseParenthesis,
        OpenBracket,
        CloseBracket,
        OpenBrace,
        CloseBrace,

        Plus,
        Minus,
        Asterisk,
        Slash,
        Equals,
        LessThan,
        GreaterThan,

        PlusEquals,
        MinusEquals,
        AsteriskEquals,
        SlashEquals,
        EqualsEquals,
        LessThanEquals,
        GreaterThanEquals
    }
}