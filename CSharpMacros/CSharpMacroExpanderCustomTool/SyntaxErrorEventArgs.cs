using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    class SyntaxErrorEventArgs : EventArgs
    {
        public int Line { get; private set; }
        public int Column { get; private set; }
        public string Message { get; private set; }
        public int Level { get; private set; }

        public SyntaxErrorEventArgs(ParserMessage message)
        {
            Line = message.Location.Line+1;
            Column = message.Location.Column+1;
            Message = message.Message;
            Level = (int)message.Level;
        }
    }
}
