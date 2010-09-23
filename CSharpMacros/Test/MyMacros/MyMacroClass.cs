using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.CSharp.Compiler.Macros;
using Microsoft.CSharp.Compiler.Syntax;

namespace MyMacros
{
    [MacroClass]
    public class MyMacroClass : MacrosContainer
    {
        [Macro]
        public IStringLiteral HelloWorld()
        {
            return ParsingTreeFactory.CreateStringLiteral("Hello world");
        }
    }
}
