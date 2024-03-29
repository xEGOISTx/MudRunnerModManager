namespace MudRunnerModManager.Models.XmlWorker
{
    internal class XmlElemAttribute(string name, string value)
    {
        public string Name { get; } = name;
        public string Value { get; } = value;


        public static bool operator ==(XmlElemAttribute left, XmlElemAttribute right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(XmlElemAttribute left, XmlElemAttribute right)
        {
            return !Equals(left, right);
        }

        public override bool Equals(object? obj)
        {
            if (obj is not XmlElemAttribute attr)
                return false;
            return attr.GetHashCode() == GetHashCode();
        }

        public override int GetHashCode()
        {
            return (Name + Value).GetHashCode();
        }
    }
}
