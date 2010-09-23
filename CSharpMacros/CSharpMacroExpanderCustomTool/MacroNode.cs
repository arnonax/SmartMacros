using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    class MacroNode
    {
        private ParseTreeNode m_node;
        private SpanContext m_spanContext;

        public MacroNode(MethodInfo macroMethod, ParseTreeNode node, SpanContext spanContext)
        {
            MacroMethod = macroMethod;
            m_node = node;
            m_spanContext = spanContext;
        }

        public MethodInfo MacroMethod { get; private set; }

        public CodeSpan Span
        {
            get
            {
                CodeLocation startLocation = new CodeLocation(m_node.Span.Location);
                return new CodeSpan(m_spanContext, startLocation, m_spanContext.GetEndLocation(startLocation, m_node.Span.Length));
            }
        }

        public object[] Parameters
        {
            get
            {
                return null; // temp
            }
        }
    }
}
