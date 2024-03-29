using System.Xml;

namespace MudRunnerModManager.Models.XmlWorker
{
	internal interface IXmlItem
	{
		string Name { get; }
		XmlNodeType NodeType { get; }
	}
}
