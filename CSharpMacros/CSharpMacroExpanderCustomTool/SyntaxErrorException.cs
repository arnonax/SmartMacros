using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    public class SyntaxErrorException : Exception
    {
        int m_line;
        int m_column;

        internal SyntaxErrorException(string message, SourceLocation location)
            : base(message)
        {
            m_line = location.Line;
            m_column = location.Position;
        }

        public int Line
        {
            get { return m_line; }
        }

        public int Column
        {
            get { return m_column; }
        }
    }
}
