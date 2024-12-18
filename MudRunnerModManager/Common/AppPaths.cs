using System;
using System.IO;

namespace MudRunnerModManager.Common
{
    internal static class AppPaths
    {
        public static string AppDataDir = GetAppDataDir();
		public static DirectoryInfo AppTempDir = new($@"{AppDataDir}\Temp");
        public static DirectoryInfo MudRunnerCacheDir =
            new($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{AppConsts.MUD_RUNNER}");


        //public static string MRRootDirRepFile = @$"{AppDataDir}\MRRootDirectory.txt";

        public static string XmlSettingsFilePath = @$"{AppDataDir}\settings.xml";

		public static string XmlChaptersFilePath = @$"{AppDataDir}\chapters.xml";

		private static string GetAppDataDir()
        {
#if DEBUG
            return @$"{Environment.CurrentDirectory}\appData";
#else
            return $@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}";
#endif
		}
	}
}
