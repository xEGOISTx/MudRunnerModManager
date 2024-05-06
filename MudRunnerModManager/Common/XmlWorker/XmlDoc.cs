using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;

namespace MudRunnerModManager.Common.XmlWorker
{
    internal class XmlDoc
    {
        private List<IXmlItem> _xmlItems = [];

        public XmlDoc(string path)
        {
            Path = path;
            XmlItems = new ReadOnlyCollection<IXmlItem>(_xmlItems);
        }


        public string Path { get; private set; }

        public ReadOnlyCollection<IXmlItem> XmlItems { get; private set; }

        public bool Exists => File.Exists(Path);

        public async Task LoadOrCreateAsync()
        {
            await Task.Run(async () =>
            {
                if (!File.Exists(Path))
                    File.Create(Path).Close();

                await LoadAsync();
            });
        }

        public async Task LoadAsync()
        {
            await Task.Run(() =>
            {
                if (!File.Exists(Path))
                    return;

                _xmlItems = ReadXml();
                XmlItems = new ReadOnlyCollection<IXmlItem>(_xmlItems);
            });
        }

        public void Delete()
        {
            if (!File.Exists(Path))
                return;

            File.Delete(Path);
            Clear();
        }

        public void AddRootXmlElem(XmlElem element, XmlEndElem endElem)
        {
            if (_xmlItems.Count > 0)
                throw new InvalidOperationException("Can`t add root element. Xml doc is not empty");

            _xmlItems.Add(element);
            _xmlItems.Add(endElem);
        }

        public void Clear()
        {
            _xmlItems.Clear();
        }

        public void AddXmlElem(XmlElem element, XmlEndElem endElem, string parentName)
        {
            var parentEndElem = GetXmlItem<XmlEndElem>(endEl => endEl.Name == parentName);
            if (parentEndElem != null)
            {
                int endElemIndex = _xmlItems.IndexOf(parentEndElem);
                _xmlItems.Insert(endElemIndex, endElem);
                _xmlItems.Insert(endElemIndex, element);
            }
        }

        public void AddXmlElem(XmlElem element, string parentName)
        {
            var endElem = GetXmlItem<XmlEndElem>(endEl => endEl.Name == parentName);
            if (endElem != null)
            {
                int endElemIndex = _xmlItems.IndexOf(endElem);
                _xmlItems.Insert(endElemIndex, element);
            }
        }

        public void AddRangeXmlElems(IEnumerable<XmlElem> elements, string parentName)
        {
            var endElem = GetXmlItem<XmlEndElem>(endEl => endEl.Name == parentName);
            if (endElem != null)
            {
                var revElems = elements.Reverse();
                int endElemIndex = _xmlItems.IndexOf(endElem);
                foreach (var element in revElems)
                {
                    _xmlItems.Insert(endElemIndex, element);
                }
            }
        }

        public void RemoveXmlElem(XmlElem element)
        {
            var rElem = GetXmlItem<XmlElem>(elem => elem == element);
            if (rElem is not null)
                _xmlItems.Remove(rElem);
        }

        public bool IsPresentElem(XmlElem element)
        {
            var elem = GetXmlItem<XmlElem>(elem => elem == element);
            if (elem is not null)
                return true;

            return false;
        }

        public void ReplaceXmlElem(XmlElem element, XmlElem toElement, string parentName)
        {
            RemoveXmlElem(element);
            AddXmlElem(toElement, parentName);
        }

        public T? GetXmlItem<T>(Func<T, bool> condition) where T : IXmlItem
        {
            List<T> filteredByTypeItems = [];
            foreach (var item in _xmlItems)
            {
                if (item is T tItem)
                    filteredByTypeItems.Add(tItem);
            }

            return filteredByTypeItems.FirstOrDefault(condition);
        }

        public List<T> GetXmlItems<T>(Func<T, bool> condition) where T : IXmlItem
        {
            List<T> res = [];
            foreach (var item in _xmlItems)
            {
                if (item is T tItem && condition(tItem))
                    res.Add(tItem);
            }

            return res;
        }

        public async Task SaveAsync()
        {
            await SaveAsync(Path);
        }

        public async Task SaveAsync(string destFileName)
        {
            await Task.Run(() =>
            {
                using (Stream stream = File.Create(destFileName))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();
                    settings.Async = true;
                    settings.OmitXmlDeclaration = true;
                    settings.Indent = true;
                    settings.IndentChars = "\t";

                    using (XmlWriter writer = XmlWriter.Create(stream, settings))
                    {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
						HashSet<string> endElemNames = new(_xmlItems.Where(item => item is XmlEndElem)
                                                                    .Select(item => (item as XmlEndElem).Name));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

						foreach (var xmlItem in _xmlItems)
                        {
                            if (xmlItem is XmlElem xmlElem)
                            {
                                settings.NewLineOnAttributes = true;
                                writer.WriteStartElement(null, xmlElem.Name, null);
                                settings.NewLineOnAttributes = false;

                                foreach (var attribute in xmlElem.Attributes)
                                {
                                    writer.WriteAttributeString(null, attribute.Name, null, attribute.Value);
                                }

                                if (!endElemNames.Contains(xmlElem.Name))
                                    writer.WriteEndElement();

                            }
                            else if (xmlItem is XmlEndElem endElem)
                            {
                                writer.WriteEndElement();
                            }
                        }

                        writer.Flush();
                    }
                }

                Path = destFileName;
            });
        }

        public async Task CopyAsync(string destFileName, bool overwrite)
        {
            await Task.Run(() =>
            {
                File.Copy(Path, destFileName, overwrite);
            });
        }

        private List<IXmlItem> ReadXml()
        {
            List<IXmlItem> xmlItems = [];

            using (Stream stream = File.OpenRead(Path))
            {
                if(stream.Length == 0)
                    return xmlItems;

                XmlReaderSettings settings = new()
                {
                    Async = true
                };

                using (XmlReader reader = XmlReader.Create(stream, settings))
                {
                    while (reader.Read())
                    {
                        if (reader.NodeType == XmlNodeType.Element)
                        {
                            XmlElem element = new XmlElem(reader.Name);
                            if (reader.AttributeCount > 0)
                            {
                                while (reader.MoveToNextAttribute())
                                {
                                    element.Attributes.Add(new XmlElemAttribute(reader.Name, reader.Value));
                                }
                            }

                            xmlItems.Add(element);
                        }
                        else if (reader.NodeType == XmlNodeType.EndElement)
                        {
                            xmlItems.Add(new XmlEndElem(reader.Name));
                        }
                    }
                }
            }

            return xmlItems;
        }
    }
}
