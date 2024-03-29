using System;
using System.IO;

namespace MudRunnerModManager.Models
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
