﻿using System;
using System.IO;

namespace MudRunnerModManager.Common
{
    internal static class AppPaths
    {
        public static string AppDataDir = GetAppDataDir();//@$"{Environment.CurrentDirectory}\appData";
		public static DirectoryInfo AppTempDir = new($@"{AppDataDir}\Temp");
        public static DirectoryInfo MudRunnerCacheDir =
            new($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\{AppConsts.MUD_RUNNER}");

        //todo: удалить
        public static string MRRootDirRepFile = @$"{AppDataDir}\MRRootDirectory.txt";

        public static string XmlSettingsFilePath = @$"{AppDataDir}\settings.xml";

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
