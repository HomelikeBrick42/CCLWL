using System;

namespace CCLWL
{
    public class CompileError : Exception
    {
        public CompileError(string message, SourcePosition position) : base(message)
        {
            Position = position;
        }

        public SourcePosition Position { get; }
    }
}
