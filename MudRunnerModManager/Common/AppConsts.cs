using System;

namespace MudRunnerModManager.Common
{
    internal static class AppConsts
    {
        public const string MODS_ROOT_DIR = "MRMManagerMods";
        [Obsolete("This mods root dir name is obsolete", false)]
        public const string MODS_ROOT_DIR_OLD = "MRMLauncherMods";
        public const string MEDIA = "Media";
        public const string MUD_RUNNER = "MudRunner";
        public const string MUD_RUNNER_EXE = MUD_RUNNER + ".exe";
        public const string CONFIG = "Config";
        public const string CONFIG_XML = "Config.xml";
        public const string SEVEN_ZIP_EXT = ".7z";
        public const string MEDIA_PATH = "MediaPath";
		public const string PATH = "Path";
		public const string CHAPTERS = "Chapters";
		public const string CHAPTER = "Chapter";
		public const string NAME = "Name";
	}

    internal static class SettingsConsts
    {
        public const string SETTINGS = "Settings";
        public const string CHAPTERS = "Chapters";
        public const string CHAPTER = "Chapter";
        public const string MUDRUNNER_ROOT = "MudRunnerRoot";
        public const string NAME = "Name";
        public const string PATH = AppConsts.PATH;
        public const string ALWAYS_CLEAR_CACHE = "AlwaysClearCache";
        public const string DELETE_MOD_WITHOUT_WARNING = "DeleteModWithoutWarning";
        public const string VALUE = "Value";
	}
}
