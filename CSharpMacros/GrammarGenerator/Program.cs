using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;
using System.Reflection;
using System.IO;

namespace GrammarGenerator
{
    class Program
    {
        static object s_empty = Type.Missing;
        static Application s_wordApp = new ApplicationClass();

        static void Main(string[] args)
        {
            //Console.WriteLine("Attach debugger and press any key to continue");
            //Console.ReadKey();

            Document syntacticGrammarDocument = OpenGrammarDoc("Syntactic grammar.docx");
            GenerateGrammar(syntacticGrammarDocument, "CSharpGrammar.xml");

            QuitWord();
        }

        private static void QuitWord()
        {
            #pragma warning disable 0467
            s_wordApp.Quit(ref s_empty, ref s_empty, ref s_empty);
        }

        private static void GenerateGrammar(Document grammarDoc, string xmlFileName)
        {
            GrammarXmlCreator xmlCreator = new GrammarXmlCreator(grammarDoc);
            xmlCreator.Create(xmlFileName);

            CloseDocument(grammarDoc);
        }

        private static void CloseDocument(_Document lexicalGrammarDoc)
        {
            lexicalGrammarDoc.Close(ref s_empty, ref s_empty, ref s_empty);
        }

        private static Document OpenGrammarDoc(string grammarDocumentFileName)
        {
            string exePath = Assembly.GetEntryAssembly().Location;
            exePath = Path.GetDirectoryName(exePath);
            object fileName = Path.Combine(exePath, grammarDocumentFileName);

            object trueValue = true;
            object falseValue = false;

            return s_wordApp.Documents.Open(
                ref fileName,
                ref s_empty,
                ref trueValue,
                ref falseValue,
                ref s_empty,
                ref s_empty,
                ref s_empty,
                ref s_empty,
                ref s_empty,
                ref s_empty,
                ref s_empty,
                ref falseValue,
                ref falseValue,
                ref s_empty,
                ref s_empty,
                ref s_empty);
        }
    }
}
