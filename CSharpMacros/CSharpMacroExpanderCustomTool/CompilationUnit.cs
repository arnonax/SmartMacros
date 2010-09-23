using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CSharp.Compiler.Syntax;
using System.Reflection;
using Microsoft.CSharp.Compiler.Macros;
using Irony.Parsing;

namespace CSharpMacroExpanderCustomTool
{
    internal class CompilationUnit
    {
        public event MacroExpandedEventHandler MacroExpanded;
        public event EventHandler Completed;

        private ParseTreeNode m_compilationUnitNode;
        private SpanContext m_spanContext;

        public CompilationUnit(ParseTreeNode compilationUnitNode, SpanContext spanContext)
        {
            m_compilationUnitNode = compilationUnitNode;
            m_spanContext = spanContext;
        }

        public void ExpandMacros()
        {
            foreach(ParseTreeNode node in TraverseTree())
            {
                IMacroTerm macroTerm = node.Term as IMacroTerm;
                if (macroTerm != null)
                {
                    ExpandMacro(new MacroNode(macroTerm.MethodInfo, node, m_spanContext));
                }
            }

            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        private void ExpandMacro(MacroNode macroNode)
        {
            MethodInfo macroMethod = macroNode.MacroMethod;
            ConstructorInfo constructor = macroMethod.DeclaringType.GetConstructor(Type.EmptyTypes);
            MacrosContainer macroObject = (MacrosContainer)constructor.Invoke(null);
            ISyntaxTree expandedTree = (ISyntaxTree)macroMethod.Invoke(macroObject, macroNode.Parameters);
            
            if (MacroExpanded != null)
                MacroExpanded(this, new MacroExpandedEventArgs(macroNode.Span, expandedTree));
        }

        private IEnumerable<ParseTreeNode> TraverseTree()
        {
            return TraverseTree(m_compilationUnitNode);
        }

        private IEnumerable<ParseTreeNode> TraverseTree(ParseTreeNode root)
        {
            if (null == root)
                yield break;

            yield return root;

            foreach (var node in root.ChildNodes)
            {
                foreach (ParseTreeNode childNode in TraverseTree(node))
                {
                    yield return childNode;
                }
            }
        }
    }
}
