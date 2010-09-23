using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    class CSharpParser
    {
        public event EventHandler<SyntaxErrorEventArgs> SyntaxError;
        
        private CSharpGrammar m_grammar = new CSharpGrammar("CSharpGrammar.xml");

        private CSharpParser(IEnumerable<MethodInfo> macros)
        {
            foreach (MethodInfo macro in macros)
            {
                m_grammar.AddMacro(macro);
            }
        }

        internal static CSharpParser Create(IEnumerable<MethodInfo> macros)
        {
            return new CSharpParser(macros);
        }

        internal CompilationUnit Parse(string inputFileContent, string inputFileName)
        {
            var parser = new Parser(m_grammar);
            var spanContext = new SpanContext(inputFileContent);
            var parseTree = parser.Parse(inputFileContent, inputFileName);
            if (parseTree.HasErrors() && SyntaxError != null)
            {
                foreach (var message in parseTree.ParserMessages)
                {
                    SyntaxError(this, new SyntaxErrorEventArgs(message));
                }
            }
            var compilationUnit = new CompilationUnit(parseTree.Root, spanContext);
            return compilationUnit;
        }
    }
}
