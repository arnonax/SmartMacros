using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CSharp.Compiler.Syntax
{
    public static class ParsingTreeFactory
    {
        public static IStringLiteral CreateStringLiteral(string content)
        {
            return new StringLiteral(content);
        }
    }
}
