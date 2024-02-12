using System.Collections.Generic;
using System.Linq;

namespace MudRunnerModLauncher.Models
{
	internal class ConfigElement(string name)
	{
		public string Name { get; } = name;

		public List<ConfigElementAttribute> Attributes { get; } = [];


		public static bool operator==(ConfigElement left, ConfigElement right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ConfigElement left, ConfigElement right)
		{
			return !Equals(left, right);
		}


		public override bool Equals(object? obj)
		{
			if(obj is not ConfigElement elem)
				return false;

			return elem.GetHashCode() == this.GetHashCode();
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
