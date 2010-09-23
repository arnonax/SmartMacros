using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GrammarGenerator
{
    interface IProductionDefinition : IEquatable<IProductionDefinition>
    {
        IEnumerable<IProductionElement> TargetElements { get; }
    }
}
