using System;

namespace MudRunnerModManager.Models
{
	internal static class AppConsts
	{
		public const string MODS_ROOT_DIR = "MRMManagerMods";
		[ObsoleteAttribute("This mods root dir name is obsolete", false)]
		public const string MODS_ROOT_DIR_OLD = "MRMLauncherMods";
		public const string MEDIA = "Media";
		public const string MUD_RUNNER = "MudRunner";
		public const string MUD_RUNNER_EXE = MUD_RUNNER + ".exe";
		public const string CONFIG = "Config";
		public const string CONFIG_XML = "Config.xml";
		public const string SEVEN_ZIP_EXT = ".7z";
	}
}
