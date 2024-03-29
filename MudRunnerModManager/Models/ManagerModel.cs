using MudRunnerModManager.Models.ArchiveWorker;
using MudRunnerModManager.Models.XmlWorker;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.Models
{
    public class ManagerModel
	{
		private readonly ConfigManager _configManager = new();
		private readonly ModExtractor _modExtractor = new(new ArchiveExtractor());

		public ManagerModel()
		{
			MudRunnerRoorDir = MRRootDirLoader.Load();
		}


		public string MudRunnerRoorDir { get; private set; }

		public string[] AvailableExts { get; } = ArchiveExtractor.AvailableExts;

		public void SaveMudRunnerRoorDir(string mRRootDir)
		{
			MRRootDirLoader.Save(mRRootDir);
			MudRunnerRoorDir = MRRootDirLoader.Load();

			//на всякий случай скопируем конфиг пользователя. в будущем может не понадобится
			if(IsCorrectMRRootDir)
			{
				var dir = new DirectoryInfo(@$"{AppPaths.AppDataDir}\cb");
				var file = new FileInfo(@$"{AppPaths.AppDataDir}\\cb\\{AppConsts.CONFIG_XML}");

				if(!dir.Exists)
				{
					dir.Create();
				}
				
				if(!file.Exists)
				{
					var userConf = new FileInfo(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");
					userConf.CopyTo(file.FullName);
				}
			}
		}

		public bool IsCorrectMRRootDir => !string.IsNullOrEmpty(MudRunnerRoorDir) 
											&& File.Exists(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}")
											&& File.Exists(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");


		public async Task AddModAsync(FileInfo mod, string modName)
		{
			await Task.Run(async () =>
			{
				if (!mod.Exists || !IsCorrectMRRootDir)
					return;

				DirectoryInfo modDectinationDir = new($@"{GetModsDir()}\{modName}");

				var res = await _modExtractor.ExtractAsync(mod, modDectinationDir);
				if (!res)
					return;

				await _configManager.AddModPath(modDectinationDir, new FileInfo($@"{MudRunnerRoorDir}\{AppConsts.CONFIG_XML}"));

				ModFileCorrection(modDectinationDir);
			});
		}

		public async Task DeleteModAsync(DirectoryInfo mod)
		{
			await Task.Run(async () =>
			{
				if (!mod.Exists || !IsCorrectMRRootDir)
					return;

				Directory.Delete(mod.FullName, true);
				await _configManager.DeleteModPath(mod, new FileInfo($@"{MudRunnerRoorDir}\{AppConsts.CONFIG_XML}"));
			});
		}

		public async Task RenameMod(DirectoryInfo mod, string modName)
		{
			await _configManager.ReplaceModName(mod, modName, new FileInfo($@"{MudRunnerRoorDir}\{AppConsts.CONFIG_XML}"));
			mod.MoveTo(@$"{mod.Parent}\{modName}");
		}

		public async Task<List<DirectoryInfo>> GetAddedModsAsync()
		{
			return await Task.Run(() =>
			{
				if (!IsCorrectMRRootDir)
					return [];

				DirectoryInfo modsDir = GetModsDir();

				if (!modsDir.Exists)
					return [];

				List<DirectoryInfo> modDirs = Directory.GetDirectories(modsDir.FullName).Select(md => new DirectoryInfo(md)).ToList();

				return modDirs;
			});
		}

		public async Task<bool> IsPresentCache()
		{
			return await Task.Run(() =>
			{
				if(AppPaths.MudRunnerCacheDir.Exists)
				{
					if (AppPaths.MudRunnerCacheDir.GetDirectories().Length > 0 || AppPaths.MudRunnerCacheDir.GetFiles().Length > 0)
						return true;
				}

				return false;
			});
		}

		public async Task ClearCache()
		{
			await Task.Run(() =>
			{
				if (AppPaths.MudRunnerCacheDir.Exists)
				{
					foreach(var dir in AppPaths.MudRunnerCacheDir.GetDirectories())
					{
						dir.Delete(true);
					}

					foreach(var file in AppPaths.MudRunnerCacheDir.GetFiles())
					{
						file.Delete();
					}
				}
			});
		}

		private DirectoryInfo GetModsDir()
		{
			return new DirectoryInfo($@"{MudRunnerRoorDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}");
		}

		private void ModFileCorrection(DirectoryInfo modRootDir)
		{
			DirectoryInfo levelsDir = new DirectoryInfo(@$"{modRootDir.FullName}\levels");

			if (!levelsDir.Exists)
				return;

			var extensions = new[] { "*.dds", "*.stg" };
			var files = extensions.SelectMany(ext => levelsDir.GetFiles(ext));

			foreach (var file in files)
			{
				if (!file.Name.StartsWith("level_"))
				{
					file.MoveTo($@"{file.Directory}\level_{file.Name}", true);
				}
			}
		}

	}
}
