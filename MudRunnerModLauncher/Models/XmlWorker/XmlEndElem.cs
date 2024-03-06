using System.Xml;

namespace MudRunnerModLauncher.Models.XmlWorker
{
	internal class XmlEndElem(string name) : IXmlItem
	{
		public XmlNodeType NodeType => XmlNodeType.EndElement;

		public string Name => name;
	}
}
