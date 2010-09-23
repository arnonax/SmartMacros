using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Xml;
using System.IO;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    class CSharpGrammar : Grammar
    {
        private Dictionary<string, NonTerminal> m_nonTerminals = new Dictionary<string, NonTerminal>();
        
        internal void AddMacro(MethodInfo macroMethod)
        {
            // TODO: this is only a temporary implementation. Look at the spec and implement it accordingly.
            NonTerminal nonTerminal = new MacroNonTerminal(macroMethod);
            nonTerminal.Rule = ToTerm("$") + ToTerm(macroMethod.DeclaringType.Name) + ToTerm(".") + 
                ToTerm(macroMethod.Name) + ToTerm("(") + ToTerm(")");
            NonTerminal parent = GetNonTerminalByType(macroMethod.ReturnType);
            parent.Rule |= nonTerminal;
        }

        private NonTerminal GetNonTerminalByType(Type type)
        {
            // TODO temp:
            return m_nonTerminals["literal"];
        }

        // used by GrammarExplorer
        public CSharpGrammar()
            : this("CSharpGrammar.xml")
        {
        }

        public CSharpGrammar(string xmlFilename)
        {
            BnfTerm[] terminals = CreateLexicalStructure();

            var grammarDef = LoadGrammar(xmlFilename, terminals);
            foreach (BnfTerm term in grammarDef)
            {
                NonTerminal nonTerminal = term as NonTerminal;
                if (nonTerminal == null)
                {
                    nonTerminal = new NonTerminal(term.Name);
                    nonTerminal.Rule = new BnfExpression(term);
                }
                m_nonTerminals.Add(nonTerminal.Name, nonTerminal);
            }
            this.Root = m_nonTerminals["compilation-unit"];
        }

        private BnfTerm[] CreateLexicalStructure()
        {
            StringLiteral string_literal = TerminalFactory.CreateCSharpString("string-literal");
            StringLiteral character_literal = TerminalFactory.CreateCSharpChar("character-literal");
            NumberLiteral integer_literal = CreateIntegerLiteral();
            NumberLiteral real_literal = CreateRealLiteral();
            NonTerminal boolean_literal = new NonTerminal("boolean-literal", ToTerm("true") | "false");
            KeyTerm null_literal = ToTerm("null");
            IdentifierTerminal identifier = TerminalFactory.CreateCSharpIdentifier("identifier");
            StringLiteral backquote_token = CreateBackquoteToken();
            NonTerminal literal = new NonTerminal("literal",
                boolean_literal
                | integer_literal
                | real_literal
                | character_literal
                | string_literal
                | null_literal);

            CommentTerminal SingleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            CommentTerminal DelimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(SingleLineComment);
            NonGrammarTerminals.Add(DelimitedComment);
            //Temporarily, treat preprocessor instructions like comments
            CommentTerminal ppInstruction = new CommentTerminal("ppInstruction", "#", "\n");
            NonGrammarTerminals.Add(ppInstruction);

            BnfTerm rightShift, rightShiftAssignment;
            CreateOperatorsPunctuatorsAndDelimiters(out rightShift, out rightShiftAssignment);

            return new BnfTerm[] {
                identifier,
                literal,
                rightShift,
                rightShiftAssignment,
                backquote_token,
                boolean_literal,
                integer_literal,
                real_literal,
                character_literal,
                string_literal,
                null_literal};
        }

        private void CreateOperatorsPunctuatorsAndDelimiters(out BnfTerm rightShift, out BnfTerm rightShiftAssignment)
        {
            RegisterOperators(1, "||");
            RegisterOperators(2, "&&");
            RegisterOperators(3, "|");
            RegisterOperators(4, "^");
            RegisterOperators(5, "&");
            RegisterOperators(6, "==", "!=");
            RegisterOperators(7, "<", ">", "<=", ">=", "is", "as");
            rightShift = new KeyTerm(">>", "right-shift");
            RegisterOperators(8, "<<");
            RegisterOperators(8, rightShift);
            RegisterOperators(9, "+", "-");
            RegisterOperators(10, "*", "/", "%");
            RegisterOperators(11, ".");
            // RegisterOperators(12, "++", "--");

            #region comments

            //The following makes sense, if you think about "?" in context of operator precedence. 
            // What we say here is that "?" has the lowest priority among arithm operators.
            // Therefore, the parser should prefer reduce over shift when input symbol is "?".
            // For ex., when seeing ? in expression "a + b?...", the parser will perform Reduce:
            //  (a + b)->expr
            // and not shift the "?" symbol.  
            // Same goes for ?? symbol

            #endregion comments

            RegisterOperators(-3, "=", "+=", "-=", "*=", "/=", "%=", "&=", "|=", "^=", "<<=");
            rightShiftAssignment = new KeyTerm(">>=", "right-shift-assignment");
            RegisterOperators(-3, rightShiftAssignment);
            RegisterOperators(-2, "?");
            RegisterOperators(-1, "??");

            this.Delimiters = "{}[](),:;+-*/%&|^!~<>=";
            this.MarkPunctuation(";", ",", "(", ")", "{", "}", "[", "]", ":");
            //Whitespace and NewLine characters
            //TODO: 
            // 1. In addition to "normal" whitespace chars, the spec mentions "any char of unicode class Z" -
            //   need to create special comment-based terminal that simply eats these category-based whitechars and produces comment token. 
            // 2. Add support for multiple line terminators to LineComment
            this.LineTerminators = "\r\n\u2085\u2028\u2029"; //CR, linefeed, nextLine, LineSeparator, paragraphSeparator
            this.WhitespaceChars = " \t\r\n\v\u2085\u2028\u2029"; //add extra line terminators
        }

        private static NumberLiteral CreateIntegerLiteral()
        {
            NumberLiteral term = new NumberLiteral("integer-literal");
            term.DefaultIntTypes = new TypeCode[] { TypeCode.Int32, TypeCode.UInt32, TypeCode.Int64, TypeCode.UInt64 };
            term.AddPrefix("0x", NumberOptions.Hex);
            term.AddSuffix("u", TypeCode.UInt32, TypeCode.UInt64);
            term.AddSuffix("l", TypeCode.Int64, TypeCode.UInt64);
            term.AddSuffix("ul", TypeCode.UInt64);
            return term;
        }

        private static NumberLiteral CreateRealLiteral()
        {
            NumberLiteral term = new NumberLiteral("real-literal", NumberOptions.AllowStartEndDot | NumberOptions.AllowSign | NumberOptions.AllowLetterAfter);
            term.DefaultFloatType = TypeCode.Double;
            term.AddSuffix("f", TypeCode.Single);
            term.AddSuffix("d", TypeCode.Double);
            term.AddSuffix("m", TypeCode.Decimal);
            return term;
        }

        private StringLiteral CreateBackquoteToken()
        {
            return new StringLiteral("backquote-token", "`", StringOptions.AllowsLineBreak | StringOptions.NoEscapes);
        }

        IEnumerable<BnfTerm> LoadGrammar(string xmlFileName, params BnfTerm[] terminals)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(GetFullPath(xmlFileName));
            return new XmlGrammarReader(doc, terminals);
        }

        private string GetFullPath(string xmlFileName)
        {
            Assembly currentAssembly = Assembly.GetExecutingAssembly();
            return Path.Combine(Path.GetDirectoryName(currentAssembly.Location), xmlFileName);
        }
    }
}
