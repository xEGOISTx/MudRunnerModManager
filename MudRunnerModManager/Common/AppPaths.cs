using System;
using System.IO;

namespace MudRunnerModManager.Common
{
    internal static class AppPaths
    {
        public static string AppDataDir = GetAppDataDir();
		public static DirectoryInfo AppTempDir = new($@"{AppDataDir}\Temp");
        public static DirectoryInfo MudRunnerCacheDir =
            new($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{GameName.MUD_RUNNER}");
		public static DirectoryInfo SpinTiresCacheDir =
			new($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{GameName.SPIN_TIRES}");

		public static string XmlSettingsFilePath = @$"{AppDataDir}\{XmlConsts.SETTINGS_XML}";

		public static string XmlChaptersFilePath = @$"{AppDataDir}\{XmlConsts.CHAPTERS_XML}";

		public static string XmlGameRootPathsFilePath = @$"{AppDataDir}\{XmlConsts.GAMES_ROOT_PATHS_XML}";

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
