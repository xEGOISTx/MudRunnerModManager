using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModLauncher.Models
{
	internal static class AppPaths
	{
		public static string AppDataDir = @$"{Environment.CurrentDirectory}\appData";
		public static DirectoryInfo AppTempDir = new($@"{AppDataDir}\Temp");
		public static DirectoryInfo MudRunnerCacheDir =
			new($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{AppConsts.MUD_RUNNER}");

		public static string MRRootDirRepFile = @$"{AppDataDir}\MRRootDirectory.txt";
	}
}
