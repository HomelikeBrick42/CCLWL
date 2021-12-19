namespace CCLWL
{
    public enum TokenKind
    {
        EndOfFile,

        Name,
        Integer,
        
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

        PlusEquals,
        MinusEquals,
        AsteriskEquals,
        SlashEquals
    }
}
