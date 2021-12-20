﻿using System.Collections.Generic;

namespace CCLWL
{
    public sealed class AstFile : AstNode
    {
        public AstFile(IEnumerable<AstStatement> statements)
        {
            Statements = statements;
        }

        public override AstKind Kind => AstKind.File;
        public IEnumerable<AstStatement> Statements { get; }
    }
}