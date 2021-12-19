namespace CCLWL
{
    public struct SourcePosition
    {
        public SourcePosition(string filepath, string source, int position, int line, int column)
        {
            Filepath = filepath;
            Source = source;
            Position = position;
            Line = line;
            Column = column;
        }

        public string Filepath { get; }
        public string Source { get; }
        public int Position { get; }
        public int Line { get; }
        public int Column { get; }
    }
}