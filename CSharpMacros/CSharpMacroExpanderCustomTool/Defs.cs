using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp.Compiler.Syntax;

namespace CSharpMacroExpanderCustomTool
{
    public delegate void MacroExpandedEventHandler(object source, MacroExpandedEventArgs args);

    public class MacroExpandedEventArgs : EventArgs
    {
        public CodeSpan MacroSpan { get; private set; }
        public ISyntaxTree ExpandedTree { get; private set; }

        public MacroExpandedEventArgs(CodeSpan macroSpan, ISyntaxTree expandedTree)
        {
            MacroSpan = macroSpan;
            ExpandedTree = expandedTree;
        }
    }
}
