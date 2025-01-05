using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppSettings;
using System.IO;
using System.Threading.Tasks;

namespace MudRunnerModManager.Models
{
    public class SettingsModel
	{
		private readonly Settings _settings;
		private readonly ConfigManager _configManager = new();

		public SettingsModel(Settings settings)
		{ 
			_settings = settings;
		}

		public ISettings Settings => _settings;

		public async Task Save()
		{
			//await Task.Run(() =>
			//{
				//на всякий случай скопируем конфиг пользователя. в будущем может не понадобится
			//	if (IsCorrectMRRootDir(_settings.MudRunnerRootDir))
			//	{
			//		var dir = new DirectoryInfo(@$"{AppPaths.AppDataDir}\cb");
			//		var file = new FileInfo(@$"{AppPaths.AppDataDir}\\cb\\{AppConsts.CONFIG_XML}");

			//		if (!dir.Exists)
			//		{
			//			dir.Create();
			//		}

			//		if (!file.Exists)
			//		{
			//			var userConf = new FileInfo(@$"{_settings.MudRunnerRootDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");
			//			userConf.CopyTo(file.FullName);
			//		}
			//	}
			//});


			await _settings.SaveAsync();
		}

		//public async Task SynhronizeWithOldVersion()
		//{
		//	var oldModsRootDir = new DirectoryInfo(@$"{_settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR_OLD}");
		//	if (oldModsRootDir.Exists)
		//		oldModsRootDir.MoveTo($@"{oldModsRootDir.Parent}\{AppConsts.MODS_ROOT_DIR}");

		//	await _configManager.ReplaseOldModsRootDirNameToNewName(new FileInfo($@"{_settings.MudRunnerRootDir}\{AppConsts.CONFIG_XML}"));
		//}
	}
}
