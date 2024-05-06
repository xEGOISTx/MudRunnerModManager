using System.Xml;

namespace MudRunnerModManager.Common.XmlWorker
{
    internal class XmlEndElem(string name) : IXmlItem
    {
        public XmlNodeType NodeType => XmlNodeType.EndElement;

        public string Name => name;
    }
}
