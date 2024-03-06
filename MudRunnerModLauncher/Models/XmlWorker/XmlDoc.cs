using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Collections.ObjectModel;
using System.Linq;
using System;

namespace MudRunnerModLauncher.Models.XmlWorker
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
					File.Create(Path);

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
			_xmlItems.Clear();
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
						HashSet<string> endElemNames = new(_xmlItems.Where(item => item is XmlEndElem)
																	.Select(item => (item as XmlEndElem).Name));

						foreach(var xmlItem in _xmlItems)
						{
							if(xmlItem is XmlElem xmlElem)
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
							else if(xmlItem is XmlEndElem endElem)
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
