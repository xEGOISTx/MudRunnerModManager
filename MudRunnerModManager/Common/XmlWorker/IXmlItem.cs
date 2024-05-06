using System.Xml;

namespace MudRunnerModManager.Common.XmlWorker
{
    internal interface IXmlItem
    {
        string Name { get; }
        XmlNodeType NodeType { get; }
    }
}
