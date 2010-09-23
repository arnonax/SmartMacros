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
            string s = $MyMacroClass.HelloWorld();
            Console.WriteLine(s);
        }
    }
}
