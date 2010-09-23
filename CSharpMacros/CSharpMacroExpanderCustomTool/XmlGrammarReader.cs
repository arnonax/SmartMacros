using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Diagnostics;
using Irony.Parsing;
using System.Collections;

namespace CSharpMacroExpanderCustomTool
{
    class XmlGrammarReader : IEnumerable<BnfTerm>
    {
        private XmlDocument m_doc;
        private IDictionary<string, BnfTerm> m_terms = null;
        private IEnumerable<BnfTerm> m_terminals;

        public XmlGrammarReader(XmlDocument doc, IEnumerable<BnfTerm> terminals)
        {
            m_doc = doc;
            m_terminals = terminals;
            ReadNonTerminals();
        }

        private void ReadNonTerminals()
        {
            m_terms = new Dictionary<string, BnfTerm>();
            FillTerminals();

            var nonTerminalNodes = m_doc.SelectNodes("Grammar/NonTerminal");
            foreach (XmlElement nonTerminalElement in nonTerminalNodes)
            {
                string name = GetNonTerminalName(nonTerminalElement);
                if (!m_terms.ContainsKey(name))
                {
                    m_terms.Add(name, new NonTerminal(name));
                }
            }
            foreach (XmlElement nonTerminalElement in nonTerminalNodes)
            {
                NonTerminal nonTerminal = (NonTerminal)m_terms[GetNonTerminalName(nonTerminalElement)];
                foreach(XmlElement productionElement in nonTerminalElement.SelectNodes("Production"))
                {
                    BnfExpression expr = null;
                    foreach (XmlElement term in productionElement.ChildNodes)
                    {
                        BnfTerm destTerm = null;
                        switch(term.Name)
                        {
                            case "NonTerminal":
                                string destName = GetNonTerminalName(term);
                                if (!m_terms.ContainsKey(destName))
                                    throw new Exception(string.Format("term '{0}' does not exist.", destName));

                                destTerm = m_terms[destName];
                                break;
                            
                            case "Terminal":
                                destTerm = Grammar.CurrentGrammar.ToTerm(term.InnerText);
                                break;

                            default:
                                throw new InvalidOperationException("unrecognized XmlElement " + term.Name);
                        }

                        if (bool.Parse(term.Attributes["Optional"].Value))
                            destTerm = destTerm.Q();

                        if (expr == null)
                            expr = new BnfExpression(destTerm);
                        else
                            expr += destTerm;
                    }
                    if (nonTerminal.Rule == null)
                        nonTerminal.Rule = expr;
                    else
                        nonTerminal.Rule |= expr;
                }

                Debug.Assert(nonTerminal.Rule != null);
            }
        }

        private void FillTerminals()
        {
            foreach (BnfTerm term in m_terminals)
            {
                m_terms.Add(term.Name, term);
            }
        }

        private static string GetNonTerminalName(XmlElement nonTerminalElement)
        {
            string name = nonTerminalElement.Attributes["Name"].Value;
            return name;
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_terms.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable<BnfTerm> Members

        public IEnumerator<BnfTerm> GetEnumerator()
        {
            return m_terms.Values.GetEnumerator();
        }

        #endregion
    }
}
