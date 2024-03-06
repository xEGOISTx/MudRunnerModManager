using System.Xml;

namespace MudRunnerModLauncher.Models.XmlWorker
{
	internal interface IXmlItem
	{
		string Name { get; }
		XmlNodeType NodeType { get; }
	}
}
