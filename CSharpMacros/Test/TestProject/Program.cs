#line 1 "c:\temp\CSharpMacros\Test\TestProject\Program.csx" 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TestProjects
{
    class Program
    {
        static void Main(string[] args)
        {
            string s = 
#line default
/* Expansion of
$MyMacroClass.HelloWorld() */
"Hello world"
// End of macro expansion
#line 12 "c:\temp\CSharpMacros\Test\TestProject\Program.csx" 
;
            Console.WriteLine(s);
        }
    }
}
