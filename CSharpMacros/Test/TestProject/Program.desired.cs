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
#line default
            string s = /* $MyMacroClass.HelloWorld() */
                       "Hello world"
                // End of macro expansion
                                    ;
#line 13 "c:\temp\CSharpMacros\Test\TestProject\Program.csx"
            Console.WriteLine(s);
        }
    }
}
