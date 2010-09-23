using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Word;
using System.Xml;
using System.IO;

namespace GrammarGenerator
{
    class GrammarXmlCreator : IGrammarDefinition
    {
        private Document m_grammarDoc;
        private XmlDocument m_doc;

        private class XmlNames
        {
            public static readonly string Grammar = "Grammar";
            public static readonly string Name = "Name";
            public static readonly string Terminal = "Terminal";
            public static readonly string NonTerminal = "NonTerminal";
            public static readonly string Optional = "Optional";
        }

        private class NonTerminalDefinition : INonTerminalDefinition
        {
            XmlElement m_productionXmlElement;

            public NonTerminalDefinition(XmlElement productionXmlElement)
            {
                m_productionXmlElement = productionXmlElement;
            }

            #region INonTerminalDefinition Members

            public string Name
            {
                get { return m_productionXmlElement.Attributes[XmlNames.Name].Value; }
            }

            public IEnumerable<IProductionDefinition> Productions
            {
                get 
                {
                    foreach (XmlNode productionNode in m_productionXmlElement.ChildNodes)
                    {
                        XmlElement productionXmlElement = productionNode as XmlElement;
                        if (productionXmlElement == null)
                            continue;

                        yield return new ProductionDefinition(productionXmlElement);
                    }
                }
            }

            #endregion
        }

        private class ProductionDefinition : IProductionDefinition
        {
            XmlElement m_productionXmlElement;

            public ProductionDefinition(XmlElement productionXmlElement)
            {
                m_productionXmlElement = productionXmlElement;
            }

            #region IProductionDefinition Members

            IEnumerable<IProductionElement> IProductionDefinition.TargetElements
            {
                get 
                {
                    foreach (XmlNode xmlNode in m_productionXmlElement.ChildNodes)
                    {
                        XmlElement xmlElement = xmlNode as XmlElement;
                        if (xmlElement == null)
                            continue;

                        if (xmlNode.Name == XmlNames.Terminal)
                            yield return new TerminalElement(xmlElement);

                        else
                            yield return new NonTerminalElement(xmlElement);
                    }
                }
            }

            #endregion

            //#region IEqualityComparer<IProductionDefinition> Members

            //public bool Equals(IProductionDefinition x, IProductionDefinition y)
            //{
            //    return GrammarXmlCreator.Equals(x, y);
            //}

            //public int GetHashCode(IProductionDefinition productionDefinition)
            //{
            //    return GrammarXmlCreator.GetHashCode(productionDefinition);
            //}

            //#endregion

            #region IEquatable<IProductionDefinition> Members

            public bool Equals(IProductionDefinition other)
            {
                return GrammarXmlCreator.Equals(this, other);
            }

            #endregion
        }

        private class TerminalElement : ITerminalElement
        {
            XmlElement m_xmlElement;

            internal TerminalElement(XmlElement xmlElement)
            {
                m_xmlElement = xmlElement;
            }

            #region ITerminalElement Members

            string ITerminalElement.Value
            {
                get { return m_xmlElement.InnerText; }
            }

            #endregion

            #region IProductionElement Members

            public bool IsOptional
            {
                get { return bool.Parse(m_xmlElement.Attributes[XmlNames.Optional].Value); }
            }

            #endregion

            //#region IEqualityComparer<IProductionElement> Members

            //public bool Equals(IProductionElement x, IProductionElement y)
            //{
            //    return GrammarXmlCreator.Equals(x, y);
            //}

            //public int GetHashCode(IProductionElement element)
            //{
            //    return GrammarXmlCreator.GetHashCode(element);
            //}

            //#endregion

            #region IEquatable<IProductionElement> Members

            public bool Equals(IProductionElement other)
            {
                return GrammarXmlCreator.Equals(this, other);
            }

            #endregion
        }

        private class NonTerminalElement : INonTerminalElement
        {
            XmlElement m_xmlElement;

            internal NonTerminalElement(XmlElement xmlElement)
            {
                m_xmlElement = xmlElement;
            }

            #region INonTerminalElement Members

            string INonTerminalElement.Name
            {
                get { return m_xmlElement.Attributes[XmlNames.Name].Value; }
            }

            bool IProductionElement.IsOptional
            {
                get { return bool.Parse(m_xmlElement.Attributes[XmlNames.Optional].Value); }
            }

            #endregion

            //#region IEqualityComparer<IProductionElement> Members

            //public bool Equals(IProductionElement x, IProductionElement y)
            //{
            //    return GrammarXmlCreator.Equals(x, y);
            //}

            //public int GetHashCode(IProductionElement element)
            //{
            //    return GrammarXmlCreator.GetHashCode(element);
            //}

            //#endregion

            #region IEquatable<IProductionElement> Members

            public bool Equals(IProductionElement other)
            {
                return GrammarXmlCreator.Equals(this, other);
            }

            #endregion
        }

        internal GrammarXmlCreator(Document grammarDoc)
        {
            m_grammarDoc = grammarDoc;
            m_doc = new XmlDocument();
        }

        internal void Create(string fileName)
        {
            FileInfo existingXmlFile = new FileInfo(fileName);
            FileInfo grammarFile = new FileInfo(m_grammarDoc.FullName);
            if (existingXmlFile.Exists && existingXmlFile.LastWriteTime > grammarFile.LastWriteTime)
            {
                m_doc.Load(fileName);
                return;
            }

            XmlElement rootElement = m_doc.CreateElement(XmlNames.Grammar);
            m_doc.AppendChild(rootElement);

            foreach (Paragraph paragraph in m_grammarDoc.Paragraphs)
            {
                XmlNode node = ConvertToXmlNode(paragraph);
                rootElement.AppendChild(node);
            }

            m_doc.Save(fileName);
        }

        private XmlNode ConvertToXmlNode(Paragraph paragraph)
        {
            Style style = paragraph.Format.get_Style() as Style;
            string text = paragraph.Range.Text;
            if (style.NameLocal == "Grammar")
            {
                string[] lines = text.Split(new char[] {'\v', '\r'}, StringSplitOptions.RemoveEmptyEntries);
                XmlElement nonTerminalElement = m_doc.CreateElement(XmlNames.NonTerminal);
                XmlAttribute nameAttribute = m_doc.CreateAttribute(XmlNames.Name);
                string sourceLine = lines[0];
                int endOfNonTerminalName = sourceLine.IndexOf(':');
                string nonTerminalName = sourceLine.Remove(endOfNonTerminalName);
                nameAttribute.Value = nonTerminalName;
                nonTerminalElement.Attributes.Append(nameAttribute);
                Console.WriteLine(nonTerminalName);

                if (sourceLine.IndexOf("one of", endOfNonTerminalName) >= endOfNonTerminalName)
                {
                    CreateOneOfProductions(nonTerminalElement, paragraph, lines);
                }
                else
                {
                    CreateProductions(nonTerminalElement, paragraph, lines);
                }

                return nonTerminalElement;
            }

            return m_doc.CreateComment(paragraph.Range.Text);
        }

        private void CreateOneOfProductions(XmlElement nonTerminalElement, Paragraph paragraph, string[] lines)
        {
            for(int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                string[] terminals = line.Split(new char[] { ' ', '\t', '\v' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string terminal in terminals)
                {
                    XmlElement productionElement = m_doc.CreateElement("Production");
                    productionElement.AppendChild(CreateTerminalProductionElementXmlElement(terminal, false));
                    nonTerminalElement.AppendChild(productionElement);
                }
            }
        }

        private void CreateProductions(XmlElement nonTerminalElement, Paragraph paragraph, string[] lines)
        {
            Range lineRange = FindRange(paragraph.Range, lines[0], 0);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                lineRange = FindRange(paragraph.Range, line, lineRange.End);
                XmlNode productionXmlNode = CreateProduction(line, lineRange);

                nonTerminalElement.AppendChild(productionXmlNode);
            }
        }

        private Range FindRange(Range containingRange, string textToSearch, int startIndex)
        {
            Range newRange = containingRange.Duplicate;
            object units = WdUnits.wdCharacter;

            if (startIndex == 0)
                startIndex = containingRange.Start;
            object startOffset = containingRange.Text.IndexOf(textToSearch, startIndex - containingRange.Start);
            newRange.MoveStart(ref units, ref startOffset);
            object endOffset = textToSearch.Length - newRange.Characters.Count; // negative (backwards)
            newRange.MoveEnd(ref units, ref endOffset);

            return newRange;
        }

        private XmlNode CreateProduction(string line, Range lineRange)
        {
            XmlElement productionXmlElement = m_doc.CreateElement("Production");

            string[] productionElements = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            int startIndex = 0;
            foreach (string productionElement in productionElements)
            {
                Range productionElementRange = FindRange(lineRange, productionElement, startIndex);
                startIndex = productionElementRange.End;
                bool optional =
                    productionElementRange.Text.EndsWith("opt") &&
                    productionElementRange.Characters[productionElement.Length].Font.Subscript != 0;

                string productionElementName;
                if (optional)
                {
                    productionElementName = productionElement.Remove(productionElement.Length - 3);
                    productionElementRange.SetRange(productionElementRange.Start, productionElementRange.End - 3);
                }
                else
                    productionElementName = productionElement;

                Style textStyle = (Style)productionElementRange.CharacterStyle;
                string textStyleName = null;
                if (textStyle != null)
                    textStyleName = textStyle.NameLocal;

                if (textStyleName == "Default Paragraph Font")
                {
                    XmlElement productionElementElement =
                        CreateNonTerminalProductionElementElement(productionElementName, optional);

                    productionXmlElement.AppendChild(productionElementElement);
                }
                else if (textStyleName == "Terminal")
                {
                    XmlElement terminalElement = CreateTerminalProductionElementXmlElement(productionElementName, optional);
                    productionXmlElement.AppendChild(terminalElement);
                }
                else
                {
                    // if the line contains elements which are not of "Terminal" or "Default Paragraph Font" style, then put the whole line in comment.
                    XmlComment comment = m_doc.CreateComment(line);
                    return comment;
                }
            }

            return productionXmlElement;
        }

        private XmlElement CreateTerminalProductionElementXmlElement(string productionElementName, bool optional)
        {
            XmlElement terminalElement = m_doc.CreateElement(XmlNames.Terminal);
            terminalElement.InnerText = productionElementName;
            XmlAttribute optionalAttribute = CreateOptionalAttribute(optional);
            terminalElement.Attributes.Append(optionalAttribute);
            return terminalElement;
        }

        private XmlAttribute CreateOptionalAttribute(bool optional)
        {
            XmlAttribute optionalAttribute = m_doc.CreateAttribute(XmlNames.Optional);
            optionalAttribute.Value = optional.ToString();
            return optionalAttribute;
        }

        private XmlElement CreateNonTerminalProductionElementElement(string productionElementName, bool optional)
        {
            XmlElement productionElementElement = m_doc.CreateElement(XmlNames.NonTerminal);
            XmlAttribute nameAttribute = m_doc.CreateAttribute("Name");
            nameAttribute.Value = productionElementName;
            productionElementElement.Attributes.Append(nameAttribute);
            XmlAttribute optionalAttribute = CreateOptionalAttribute(optional);
            productionElementElement.Attributes.Append(optionalAttribute);

            //productionElementElement.Attributes.Append(CreateOptionalAttribute(optional));
            return productionElementElement;
        }

        #region IGrammarDefinition Members

        IEnumerable<INonTerminalDefinition> IGrammarDefinition.GetNonTerminalDefinitions()
        {
            XmlElement rootElement = m_doc.DocumentElement;
            foreach (XmlNode productionNode in rootElement.ChildNodes)
            {
                XmlElement productionXmlElement = productionNode as XmlElement;
                if (productionXmlElement != null)
                {
                    yield return new NonTerminalDefinition(productionXmlElement);
                }
            }
        }

        #endregion

        public static bool Equals(IProductionElement x, IProductionElement y)
        {
            INonTerminalElement nonTerminalX = x as INonTerminalElement;
            INonTerminalElement nonTerminalY = y as INonTerminalElement;
            if (nonTerminalX != null && nonTerminalY != null && nonTerminalX.Name == nonTerminalY.Name && nonTerminalX.IsOptional == nonTerminalY.IsOptional)
                return true;

            ITerminalElement terminalX = x as ITerminalElement;
            ITerminalElement terminalY = y as ITerminalElement;

            if (terminalX != null && terminalY != null && terminalX.Value == terminalY.Value && terminalX.IsOptional == terminalY.IsOptional)
                return true;

            return false;
        }

        //public static int GetHashCode(IProductionElement element)
        //{
        //    INonTerminalElement nonTerminalElement = element as INonTerminalElement;
        //    if (nonTerminalElement != null)
        //        return nonTerminalElement.Name.GetHashCode() ^ nonTerminalElement.IsOptional.GetHashCode();

        //    ITerminalElement terminalElement = (ITerminalElement)element;
        //    return terminalElement.Value.GetHashCode() ^ terminalElement.IsOptional.GetHashCode();
        //}

        //internal static int GetHashCode(IProductionDefinition productionDefinition)
        //{
        //    int hashCode = 0;
        //    foreach (var element in productionDefinition.TargetElements)
        //    {
        //        hashCode ^= GetHashCode(element);
        //    }
        //    return hashCode;
        //}

        internal static bool Equals(IProductionDefinition x, IProductionDefinition y)
        {
            var xElementsEnumerator = x.TargetElements.GetEnumerator();
            var yElementsEnumerator = y.TargetElements.GetEnumerator();
            
            bool xHasMoreItems = xElementsEnumerator.MoveNext();
            bool yHasMoreItems = yElementsEnumerator.MoveNext();

            while (xHasMoreItems && yHasMoreItems)
            {
                if (!xElementsEnumerator.Current.Equals(yElementsEnumerator.Current))
                    return false;

                xHasMoreItems = xElementsEnumerator.MoveNext();
                yHasMoreItems = yElementsEnumerator.MoveNext();
            }

            return xHasMoreItems == yHasMoreItems;
        }
    }
}