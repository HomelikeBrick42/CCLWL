namespace CCLWL
{
    public sealed class Parser
    {
        public Parser(string filepath)
        {
            _lexer = new Lexer(filepath);
            _current = _lexer.NextToken();
        }
        
        private Lexer _lexer;
        private Token _current;
    }
}
