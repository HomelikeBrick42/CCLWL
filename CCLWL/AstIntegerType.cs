﻿namespace CCLWL
{
    public sealed class AstIntegerType : AstType
    {
        public AstIntegerType(int size)
        {
            Size = size;
        }

        public override AstTypeKind TypeKind => AstTypeKind.Integer;

        public int Size { get; }

        public override AstType Clone()
        {
            return new AstIntegerType(Size);
        }

        public override bool Matches(AstType other)
        {
            return this == other;
        }
    }
}