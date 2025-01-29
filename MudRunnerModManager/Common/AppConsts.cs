using System;

namespace MudRunnerModManager.Common
{
    internal static class AppConsts
    {
        public const string MR_MODS_ROOT_DIR_NAME = "MRMManagerMods";
        public const string ST_MODS_ROOT_DIR_NAME = "STMManagerMods";
		public const string MEDIA = "Media";
        public const string MUD_RUNNER_EXE = GameName.MUD_RUNNER + ".exe";
		public const string SPIN_TIRES_EXE = GameName.SPIN_TIRES + ".exe";
        public const string SEVEN_ZIP_EXT = ".7z";
        public const string USER = "User";
	}

    internal static class XmlConsts
    {
		public const string CONFIG_XML = "Config.xml";
		public const string CONFIG = "Config";
		public const string MEDIA_PATH = "MediaPath";
        public const string PATH = "Path";

		public const string USER_MEDIA_PATHS_XML = "UserMediaPaths.xml";
		public const string DATA_SOURCES = "DataSources";

        public const string SETTINGS_XML = "settings.xml";
		public const string SETTINGS = "Settings";
        public const string VALUE = "Value";
        public const string ALWAYS_CLEAR_CACHE = "AlwaysClearCache";
        public const string DELETE_MOD_WITHOUT_WARNING = "DeleteModWithoutWarning";

        public const string CHAPTERS_XML = "chapters.xml";
		public const string CHAPTERS = "Chapters";
        public const string CHAPTER = "Chapter";

        public const string GAMES_ROOT_PATHS_XML = "gamesRootPaths.xml";
		public const string GAMES_ROOT_PATHS = "GamesRootPaths";
		public const string GAME = "Game";
        public const string NAME = "Name";

        [Obsolete]
        public const string MUDRUNNER_ROOT = "MudRunnerRoot";
	}

	internal static class GameName
	{
		public const string MUD_RUNNER = "MudRunner";
		public const string SPIN_TIRES = "SpinTires";
	}
}
