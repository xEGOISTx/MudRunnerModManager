using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppSettings;
using MudRunnerModManager.Models.ArchiveWorker;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.Models
{
    public class ModsModel
	{
		private readonly ConfigManager _configManager = new();
		private readonly ModExtractor _modExtractor = new(new ArchiveExtractor());

		public ModsModel(Settings settings)
		{
			Settings = settings;
		}


		public SettingsBase Settings { get; }


		//public void SaveMudRunnerRoorDir(string mRRootDir)
		//{
		//	MRRootDirLoader.Save(mRRootDir);
		//	MudRunnerRoorDir = MRRootDirLoader.Load();

		//	//на всякий случай скопируем конфиг пользователя. в будущем может не понадобится
		//	if(IsCorrectMRRootDir)
		//	{
		//		var dir = new DirectoryInfo(@$"{AppPaths.AppDataDir}\cb");
		//		var file = new FileInfo(@$"{AppPaths.AppDataDir}\\cb\\{AppConsts.CONFIG_XML}");

		//		if(!dir.Exists)
		//		{
		//			dir.Create();
		//		}
				
		//		if(!file.Exists)
		//		{
		//			var userConf = new FileInfo(@$"{MudRunnerRoorDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");
		//			userConf.CopyTo(file.FullName);
		//		}
		//	}
		//}

		public bool IsCorrectMRRootDir => !string.IsNullOrEmpty(Settings.MudRunnerRootDir) 
											&& File.Exists(@$"{Settings.MudRunnerRootDir.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}")
											&& File.Exists(@$"{Settings.MudRunnerRootDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");


		private FileInfo Config => new($@"{Settings.MudRunnerRootDir}\{AppConsts.CONFIG_XML}");

		public async Task AddModAsync(FileInfo mod, string chapter, string modName)
		{
			await Task.Run(async () =>
			{
				if (!mod.Exists || !IsCorrectMRRootDir)
					return;

				if(chapter == Res.RootChapter)
					chapter = string.Empty;
				else
					chapter += "\\";

				DirectoryInfo modDectinationDir = new($@"{GetModsDir()}\{chapter}{modName}");

				var res = await _modExtractor.ExtractAsync(mod, modDectinationDir);
				if (!res)
					return;

				await _configManager.AddModPath(modDectinationDir, Config);

				ModFileCorrection(modDectinationDir);
			});
		}

		public async Task DeleteModAsync(Mod mod)
		{
			await Task.Run(async () =>
			{
				if (!mod.DirInfo.Exists || !IsCorrectMRRootDir)
					return;

				Directory.Delete(mod.DirInfo.FullName, true);
				await _configManager.DeleteModPath(mod.DirInfo, Config);
			});
		}

		public async Task RenameModAsync(Mod mod, string modName)
		{
			await Task.Run(async () =>
			{
				await _configManager.RenameMod(mod.DirInfo, modName, Config);
				mod.DirInfo.MoveTo(@$"{mod.DirInfo.Parent}\{modName}");
			});
		}

		public async Task RelocateModAsync(Mod mod, DirectoryInfo chapter)
		{
			await Task.Run(async () =>
			{
				await _configManager.ChangeModChapter(mod.DirInfo, chapter, Config);

				string toPath = @$"{Settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}";
				if(chapter.Name == AppConsts.MODS_ROOT_DIR)
				{
					toPath += @$"\{mod.DirInfo.Name}";
				}
				else
				{
					toPath += @$"\{chapter.Name}\{mod.DirInfo.Name}";
				}

				mod.DirInfo.MoveTo(toPath);
			});
		}

		public List<Mod> GetAddedMods()
		{
			if (!IsCorrectMRRootDir)
				return [];

			DirectoryInfo modsDir = GetModsDir();

			if (!modsDir.Exists)
				return [];

			List<Mod> mods = [];
			var dirs = modsDir.GetDirectories();
			HashSet<string> chapters = new(Settings.Chapters.Select(ch => ch.FullName));

			foreach (var dir in dirs)
			{
				if (chapters.Contains(dir.FullName))
				{
					foreach (var modDir in dir.GetDirectories())
					{
						mods.Add(new Mod(modDir));
					}

					continue;
				}
				else
				{
					mods.Add(new Mod(dir));
				}
			}

			return mods;
		}

		public async Task<List<Mod>> GetAddedModsAsync()
		{
			return await Task.Run(GetAddedMods);
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
			return new DirectoryInfo($@"{Settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}");
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

	public class Mod
	{
		public Mod(DirectoryInfo directoryInfo) 
		{
			DirInfo = directoryInfo;

			if (directoryInfo.Parent == null)
				throw new System.Exception("Incorrect mod path");

			if (directoryInfo.Parent.Name == AppConsts.MODS_ROOT_DIR)
				Chapter = Res.RootChapter;
			else
				Chapter = directoryInfo.Parent.Name;
		}

		public DirectoryInfo DirInfo { get; }

		public string Name => DirInfo.Name;

		public string Chapter { get; }
	}

}
