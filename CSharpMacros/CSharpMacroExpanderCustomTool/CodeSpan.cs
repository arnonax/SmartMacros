using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CSharpMacroExpanderCustomTool
{
    public class CodeSpan
    {
        CodeLocation m_start;
        CodeLocation m_end;
        SpanContext m_context;

        static CodeSpan s_emptySpan = new CodeSpan(null, null, null);

        public static CodeSpan Empty
        {
            get { return s_emptySpan; }
        }

        public CodeSpan(SpanContext context, CodeLocation start, CodeLocation end)
        {
            m_start = start;
            m_end = end;
            m_context = context;
        }

        public int RawLength
        {
            get { return m_end.AbsolutePosition - m_start.AbsolutePosition; }
        }

        public override string ToString()
        {
            return m_start.ToString() + " - " + m_end.ToString();
        }

        public CodeLocation Start
        {
            get { return m_start; }
        }

        public CodeLocation End
        {
            get { return m_end; }
        }

        public string GetText()
        {
            return m_context.GetText(this);
        }

        public SpanContext Context
        {
            get { return m_context; }
        }
    }
}
