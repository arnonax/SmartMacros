using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace CSharpMacroExpanderCustomTool
{
    class CSharpMacroExpanderEngine
    {
        public event EventHandler<SyntaxErrorEventArgs> SyntaxError;

        private StringBuilder m_generatedFileContent = new StringBuilder();
        private string m_inputFileName;
        private string m_inputFileContent;
        private int m_lastLocation;

        public CSharpMacroExpanderEngine(string inputFileName, string inputFileContent)
        {
            m_inputFileName = inputFileName;
            m_inputFileContent = inputFileContent;

            if (m_inputFileContent.EndsWith("\x001A"))
                m_inputFileContent = m_inputFileContent.Substring(0, m_inputFileContent.Length - 1);

            if (m_inputFileContent.Length > 0)
            {
                char lastChar = m_inputFileContent[m_inputFileContent.Length - 1];
                if ("\x000D\x000A\x2028\x2029".IndexOf(lastChar) == -1)
                {
                    m_inputFileContent += '\x000D';
                }
            }
        }

        public void Run()
        {
            AppendLineDirective(CodeLocation.Start);

            IEnumerable<MethodInfo> macros = GetAllMacros();
            CSharpParser parser = CSharpParser.Create(macros);
            if (SyntaxError != null)
                parser.SyntaxError += (obj, args) => SyntaxError(this, args);
            
            CompilationUnit compilationUnit = parser.Parse(m_inputFileContent, m_inputFileName);
            if (compilationUnit == null)
                return;

            compilationUnit.MacroExpanded += OnMacroExpanded;
            compilationUnit.Completed += OnCompleted;

            compilationUnit.ExpandMacros();

            string debuggableFileName = m_inputFileName + ".csm";
            FileInfo fi = new FileInfo(debuggableFileName);

            if (fi.Exists)
            {
                fi.Attributes &= ~FileAttributes.ReadOnly;
                fi.Delete();
            }
            using (StreamWriter debuggableFileWriter = new StreamWriter(debuggableFileName))
            {
                debuggableFileWriter.Write(m_generatedFileContent);
            }
            fi.Attributes |= FileAttributes.ReadOnly;
        }

        private IEnumerable<MethodInfo> GetAllMacros()
        {
            IEnumerable<Assembly> macroAssemblies = GetMacroModules();

            List<MethodInfo> macroMethods = new List<MethodInfo>();
            foreach (Assembly assembly in macroAssemblies)
            {
                macroMethods.AddRange(GetMacroMethods(assembly));
            }

            return macroMethods;
        }

        private IEnumerable<MethodInfo> GetMacroMethods(Assembly assembly)
        {
            List<MethodInfo> macroMethods = new List<MethodInfo>();

            Type[] allPublicTypes = assembly.GetExportedTypes();
            foreach (Type type in allPublicTypes)
            {
                if (type.IsAbstract)
                    continue;

                if (type.BaseType != typeof(Microsoft.CSharp.Compiler.Macros.MacrosContainer))
                    continue;

                if (type.GetCustomAttributes(typeof(Microsoft.CSharp.Compiler.Macros.MacroClassAttribute), true).Length == 0)
                    continue;

                if (type.GetConstructor(new Type[] {}) == null)
                    continue;

                macroMethods.AddRange(GetMacroMethods(type));
            }

            return macroMethods;
        }

        private IEnumerable<MethodInfo> GetMacroMethods(Type type)
        {
            List<MethodInfo> macroMethods = new List<MethodInfo>();

            MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.InvokeMethod);
            foreach (MethodInfo methodInfo in methods)
            {
                if (methodInfo.GetCustomAttributes(typeof(Microsoft.CSharp.Compiler.Macros.MacroAttribute), true).Length == 0)
                    continue;

                macroMethods.Add(methodInfo);
            }

            return macroMethods;
        }

        private IEnumerable<Assembly> GetMacroModules()
        {
            List<Assembly> macroAssemblies = new List<Assembly>();

            string dir = Path.GetDirectoryName(m_inputFileName);
            string fullPath = Path.Combine(dir, "MacroModules.lst");
            FileInfo fi = new FileInfo(fullPath);
            if (fi.Exists)
            {
                using (StreamReader fileReader = fi.OpenText())
                {
                    string assemblyName = fileReader.ReadLine();
                    Assembly newAssembly = Assembly.LoadFile(assemblyName);
                    AssemblyResolver.AddPath(Path.GetDirectoryName(assemblyName));
                    macroAssemblies.Add(newAssembly);
                }
            }

            return macroAssemblies;
        }

        public string GeneratedFileContent
        {
            get { return m_generatedFileContent.ToString(); }
        }

        private void OnMacroExpanded(object source, MacroExpandedEventArgs args)
        {
            m_generatedFileContent.AppendLine(m_inputFileContent.Substring(m_lastLocation, args.MacroSpan.Start.AbsolutePosition));
            m_generatedFileContent.AppendLine("#line default");
            m_generatedFileContent.AppendLine("/* Expansion of");
            m_generatedFileContent.Append(args.MacroSpan.GetText());
            m_generatedFileContent.AppendLine(" */");
            m_generatedFileContent.AppendLine(args.ExpandedTree.ToString());
            m_generatedFileContent.AppendLine("// End of macro expansion");
            AppendLineDirective(args.MacroSpan.End);
            m_lastLocation = args.MacroSpan.End.AbsolutePosition;
        }

        private void OnCompleted(object source, EventArgs args)
        {
            m_generatedFileContent.Append(m_inputFileContent.Substring(m_lastLocation));
        }

        private void AppendLineDirective(CodeLocation codeLocation)
        {
            m_generatedFileContent.AppendFormat("#line {0} \"{1}\" \r\n", codeLocation.Line, m_inputFileName);
        }
    }
}
