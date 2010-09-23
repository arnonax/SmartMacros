using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    public class CodeLocation
    {
        int m_line;
        int m_column;
        int m_absolutePosition;

        private static CodeLocation m_startLocation = new CodeLocation();

        public CodeLocation()
        {
            m_line = 1;
            m_column = 1;
        }

        private CodeLocation(CodeLocation other)
        {
            m_line = other.m_line;
            m_column = other.m_column;
            m_absolutePosition = other.m_absolutePosition;
        }

        public CodeLocation(SourceLocation sourceLocation)
        {
            m_line = sourceLocation.Line + 1;
            m_column = sourceLocation.Column + 1;
            m_absolutePosition = sourceLocation.Position;
        }

        internal CodeLocation AddChar(char ch)
        {
            CodeLocation newLocation = new CodeLocation(this);
            AddCharInternal(ch, newLocation);

            return newLocation;
        }

        private static void AddCharInternal(char ch, CodeLocation newLocation)
        {
            if (ch == '\n')
            {
                newLocation.m_line++;
                newLocation.m_column = 0;
            }
            else if (ch == '\t')
                newLocation.m_column += 4;
            else if (ch != '\r')
                newLocation.m_column++;

            newLocation.m_absolutePosition++;
        }

        public int AbsolutePosition
        {
            get { return m_absolutePosition; }
        }

        public override string ToString()
        {
            return string.Format("({0},{1})", m_line, m_column);
        }

        public int Line
        {
            get { return m_line; }
        }

        public int Column
        {
            get { return m_column; }
        }

        public static CodeLocation Start
        {
            get
            {
                return m_startLocation;
            }
        }

        internal CodeLocation AddText(string text)
        {
            CodeLocation endLocation = new CodeLocation(this);
            foreach (char ch in text)
                AddCharInternal(ch, endLocation);

            return endLocation;
        }
    }
}
