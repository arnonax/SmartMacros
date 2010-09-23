using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TextTemplating.VSHost;
using System.Runtime.InteropServices;
using System.Reflection;
using System.IO;

namespace CSharpMacroExpanderCustomTool
{
    [ComVisible(true)]
    [Guid("03EEAE74-3099-4b77-BAB1-DD6693DFC88F")]
    public class CSharpMacroExpander : BaseCodeGeneratorWithSite
    {
        protected override byte[] GenerateCode(string inputFileName, string inputFileContent)
        {
            CSharpMacroExpanderEngine engine = new CSharpMacroExpanderEngine(inputFileName, inputFileContent);
            engine.SyntaxError += (obj, args) => GeneratorErrorCallback(false, args.Level, args.Message, args.Line, args.Column);
            try
            {
                engine.Run();
            }
            catch (SyntaxErrorException ex)
            {
                GeneratorErrorCallback(false, 0, ex.Message, ex.Line, ex.Column);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return new UTF8Encoding().GetBytes(engine.GeneratedFileContent);
        }

        public override string GetDefaultExtension()
        {
            return ".cs";
        }
    }
}
