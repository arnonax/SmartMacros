using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    class MacroNonTerminal : NonTerminal, IMacroTerm
    {
        public MacroNonTerminal(MethodInfo methodInfo)
            : base(string.Format("macro: {0}", methodInfo.Name))
        {
            this.MethodInfo = methodInfo;
        }

        #region IMacroNode Members

        public MethodInfo MethodInfo { get; private set; }

        #endregion
    }
}
