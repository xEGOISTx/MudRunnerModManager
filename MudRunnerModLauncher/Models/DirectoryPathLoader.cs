using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModLauncher.Models
{
	public class DirectoryPathLoader
	{
		private readonly string _repPath = @$"{Environment.CurrentDirectory}\MRRootDirectory.txt";

		public void Save(string directoryPath)
		{
			File.WriteAllText(_repPath, directoryPath);
		}

		public string Load()
		{
			if (File.Exists(_repPath))
			{
				var directoryPath = File.ReadAllLines(_repPath).FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(directoryPath))
				{
					return directoryPath;
				}
			}

			return string.Empty;
		}
	}
}
