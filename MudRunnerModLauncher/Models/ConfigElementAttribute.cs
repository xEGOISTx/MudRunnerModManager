namespace MudRunnerModLauncher.Models
{
	internal class ConfigElementAttribute(string name, string value)
	{
		public string Name { get; } = name;
		public string Value { get;} = value;


		public static bool operator ==(ConfigElementAttribute left, ConfigElementAttribute right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ConfigElementAttribute left, ConfigElementAttribute right)
		{
			return !Equals(left, right);
		}

		public override bool Equals(object? obj)
		{
			if (obj is not ConfigElementAttribute attr) 
				return false;
			return attr.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			return (Name + Value).GetHashCode();
		}
	}
}
