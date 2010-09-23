using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace CSharpMacroExpanderCustomTool
{
    static class AssemblyResolver
    {
        private static List<string> s_searchFolders = new List<string>();

        static AssemblyResolver()
        {
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            // first, search for the assembly in the memory:
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (assembly.FullName == args.Name)
                    return assembly;
            }

            string assemblyFileName = Path.GetFileName(args.Name);

            // then search the search folders
            foreach (string folder in s_searchFolders)
            {
                string assemblyFullPath = Path.Combine(folder, assemblyFileName);
                assemblyFullPath += ".dll";
                FileInfo assemblyFile = new FileInfo(assemblyFullPath);
                if (assemblyFile.Exists)
                    return Assembly.LoadFile(assemblyFile.FullName);
            }

            return null;
        }

        public static void AddPath(string searchFolder)
        {
            if (!s_searchFolders.Contains(searchFolder))
                s_searchFolders.Add(searchFolder);
        }
    }
}
