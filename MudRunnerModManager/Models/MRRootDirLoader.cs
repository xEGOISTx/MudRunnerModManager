using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModManager.Models
{
	public class MRRootDirLoader
	{
		public static void Save(string directoryPath)
		{
			if(!Directory.Exists(AppPaths.AppDataDir))
				Directory.CreateDirectory(AppPaths.AppDataDir);

			File.WriteAllText(AppPaths.MRRootDirRepFile, directoryPath);
		}

		public static string Load()
		{
			if (File.Exists(AppPaths.MRRootDirRepFile))
			{
				var directoryPath = File.ReadAllLines(AppPaths.MRRootDirRepFile).FirstOrDefault();
				if (!string.IsNullOrWhiteSpace(directoryPath))
				{
					return directoryPath;
				}
			}

			return string.Empty;
		}
	}
}
