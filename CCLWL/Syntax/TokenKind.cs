namespace CCLWL.Syntax
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
        WhileKeyword,
        ReturnKeyword,
        VarKeyword,

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
        Ampersand,

        PlusEquals,
        MinusEquals,
        AsteriskEquals,
        SlashEquals,
        EqualsEquals,
        LessThanEquals,
        GreaterThanEquals
    }
}
