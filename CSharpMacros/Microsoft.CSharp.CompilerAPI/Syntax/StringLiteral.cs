using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.CSharp.Compiler.Syntax
{
    internal class StringLiteral : IStringLiteral
    {
        private string m_content;
        public StringLiteral(string content)
        {
            m_content = content;
        }

        public override string ToString()
        {
            return string.Format("\"{0}\"", m_content);
        }
    }
}
