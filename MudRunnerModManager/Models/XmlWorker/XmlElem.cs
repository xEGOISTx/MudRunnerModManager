using System.Collections.Generic;
using System.Linq;
using System.Xml;

namespace MudRunnerModManager.Models.XmlWorker
{
    internal class XmlElem(string name) : IXmlItem
    {
        public string Name { get; } = name;

        public List<XmlElemAttribute> Attributes { get; } = [];

		public XmlNodeType NodeType => XmlNodeType.Element;

		public static bool operator ==(XmlElem left, XmlElem right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XmlElem left, XmlElem right)
        {
            return !Equals(left, right);
        }


        public override bool Equals(object? obj)
        {
            if (obj is not XmlElem elem)
                return false;

            return elem.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            string forHash = Name;

            var sortedAttrs = from attr in Attributes orderby attr.Name select attr;
            foreach (var attr in sortedAttrs)
            {
                forHash += attr.GetHashCode().ToString();
            }

            return forHash.GetHashCode();
        }
    }
}
