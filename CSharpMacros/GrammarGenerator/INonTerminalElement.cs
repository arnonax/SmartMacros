using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGenerator
{
    interface INonTerminalElement : IProductionElement
    {
        string Name { get; }        
    }
}
