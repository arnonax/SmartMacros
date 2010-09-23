using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMacroExpanderCustomTool
{
    public class SpanContext
    {
        private string m_code;

        internal SpanContext(string code)
        {
            m_code = code;
        }

        internal string GetText(CodeSpan span)
        {
            return m_code.Substring(span.Start.AbsolutePosition, span.RawLength);
        }

        internal CodeLocation GetEndLocation(CodeLocation startLocation, int length)
        {
            return startLocation.AddText(m_code.Substring(startLocation.AbsolutePosition, length));
        }
    }
}
