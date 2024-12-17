using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;
using System;

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

        public bool IsEmpty => _xmlItems.Count == 0;


        public void Load()
        {
			if (!File.Exists(Path))
				return;

			_xmlItems = ReadXml();
			XmlItems = new ReadOnlyCollection<IXmlItem>(_xmlItems);
		}

        public async Task LoadAsync()
        {
            await Task.Run(Load);
        }

        public void LoadOrCreate()
        {
			if (!File.Exists(Path))
				File.Create(Path).Close();

            Load();
		}

		public async Task LoadOrCreateAsync()
        {
            await Task.Run(LoadOrCreate);
        }

        public void Delete()
        {
            if (!File.Exists(Path))
                return;

            File.Delete(Path);
            Clear();
        }

        public void AddRootXmlElem(XmlElem element)
        {
            if (_xmlItems.Count == 0)
                _xmlItems.Add(element);

            else
                _xmlItems.Insert(0, element);

            _xmlItems.Add(new XmlEndElem(element.Name));
        }

        public void Clear()
        {
            _xmlItems.Clear();
        }

        public void AddXmlNodeElem(XmlElem element, string parentName)
        {
            if (!TryGetEndElem(parentName, out XmlEndElem parentEndElem))
            {
                var parentElem = GetXmlItem<XmlElem>(endEl => endEl.Name == parentName);
                if (parentElem is null)
                    throw new Exception($"Impossible add xml node element to parent \"{parentName}\". Parent \"{parentName}\" not found");

                parentEndElem = AddEndElementTo(parentElem);
			}

            int endElemIndex = _xmlItems.IndexOf(parentEndElem);
            _xmlItems.Insert(endElemIndex, new XmlEndElem(element.Name));
            _xmlItems.Insert(endElemIndex, element);
        }

        public void AddXmlElem(XmlElem element, string parentName)
        {
            if (!TryGetEndElem(parentName, out XmlEndElem parentEndElem))
            {
                var parentElem = GetXmlItem<XmlElem>(endEl => endEl.Name == parentName);
                if (parentElem is null)
					throw new Exception($"Impossible add xml element to parent \"{parentName}\". Parent \"{parentName}\" not found");

				parentEndElem = AddEndElementTo(parentElem);
            }

            int endElemIndex = _xmlItems.IndexOf(parentEndElem);
            _xmlItems.Insert(endElemIndex, element);

        }

        public void AddRangeXmlElems(IEnumerable<XmlElem> elements, string parentName)
        {
            if (!TryGetEndElem(parentName, out XmlEndElem parentEndElem))
            {
                var parentElem = GetXmlItem<XmlElem>(endEl => endEl.Name == parentName);
                if (parentElem is null)
                    throw new Exception($"Impossible add xml elements to parent \"{parentName}\". Parent \"{parentName}\" not found");

                parentEndElem = AddEndElementTo(parentElem);
            }

            int endElemIndex = _xmlItems.IndexOf(parentEndElem);
            foreach (var element in elements.Reverse())
            {
                _xmlItems.Insert(endElemIndex, element);
            }
        }

        public void RemoveXmlElem(XmlElem element)
        {
            var rElem = GetXmlItem<XmlElem>(elem => elem == element);
			if (rElem is null)
				return;

            if(IsNode(rElem.Name))
				throw new Exception($"XmlElemet is node. For node removing use {nameof(RemoveXmlNodeElem)}");

            _xmlItems.Remove(rElem);
        }

        public void RemoveXmlNodeElem(XmlElem element, bool removeIfIsNotNode = false)
        {
			var rElem = GetXmlItem<XmlElem>(elem => elem == element);
            if (rElem is null)
                return;

            if(TryGetEndElem(rElem.Name, out XmlEndElem rEndElem))
            {
				var startNodeIndex = _xmlItems.IndexOf(rElem);
				var endNodeIndex = _xmlItems.IndexOf(rEndElem);
				_xmlItems.RemoveRange(startNodeIndex, endNodeIndex + 1 - startNodeIndex);
			}
            else
            {
                if (removeIfIsNotNode)
                    _xmlItems.Remove(rElem);
                else
                    throw new Exception($"XmlElemet is not node. Use parameter {nameof(removeIfIsNotNode)} or {nameof(RemoveXmlElem)}");

            }
		}

        public bool IsPresentElem(XmlElem element)
        {
            var elem = GetXmlItem<XmlElem>(elem => elem == element);
            return elem is not null;
        }

		public bool IsNode(string elementName)
		{
			var endElem = GetXmlItem<XmlEndElem>(elem => elem.Name == elementName);
			return endElem != null;
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

        public void Save(string destFileName)
        {
			using (Stream stream = File.Create(destFileName))
			{
				XmlWriterSettings settings = new XmlWriterSettings();
				settings.Async = true;
				settings.OmitXmlDeclaration = true;
				settings.Indent = true;
				settings.IndentChars = "\t";
                settings.Encoding = System.Text.Encoding.UTF8;

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
		}

        public void Save()
        {
            Save(Path);
        }

        public async Task SaveAsync()
        {
            await SaveAsync(Path);
        }

        public async Task SaveAsync(string destFileName)
        {
            await Task.Run(() =>
            {
                Save(destFileName);
            });
        }

        public void Copy(string destFileName, bool overwrite)
        {
			File.Copy(Path, destFileName, overwrite);
		}

        public async Task CopyAsync(string destFileName, bool overwrite)
        {
            await Task.Run(() =>
            {
                Copy(destFileName, overwrite);
            });
        }

        private List<IXmlItem> ReadXml()
        {
            List<IXmlItem> xmlItems = [];

            using (Stream stream = File.OpenRead(Path))
            {
                //BOM
                if(stream.Length <= 3)
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

        private XmlEndElem AddEndElementTo(XmlElem elem)
        {
			var endElem = new XmlEndElem(elem.Name);
			int elemIndex = _xmlItems.IndexOf(elem);
			_xmlItems.Insert(elemIndex + 1, endElem);
            return endElem;
		}

        private bool TryGetEndElem(string elemName, out XmlEndElem endElem)
        {           
			var elem = GetXmlItem<XmlEndElem>(elem => elem.Name == elemName);
            endElem = elem is not null ? elem : new XmlEndElem("");
			return endElem != null;
		}
    }
}
