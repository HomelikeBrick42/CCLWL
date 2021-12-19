namespace CCLWL
{
    public sealed class Token
    {
        public Token(TokenKind kind, SourcePosition position, int length, object value = null)
        {
            Kind = kind;
            Position = position;
            Length = length;
            Value = value;
        }

        public TokenKind Kind { get; }
        public SourcePosition Position { get; }
        public int Length { get; }
        public object Value { get; }
    }
}
