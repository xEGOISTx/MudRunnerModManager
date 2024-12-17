using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.AppSettings;
using MudRunnerModManager.Models.ArchiveWorker;
using Splat.ModeDetection;
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
		private readonly IChapterInfosRepo _chapterInfosRepo;

		public ModsModel(IChapterInfosRepo chapterInfosRepo, Settings settings)
		{
			_chapterInfosRepo = chapterInfosRepo;
			Settings = settings;
		}


		public SettingsBase Settings { get; }

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

		public Mod RelocateMod(Mod mod, ChapterBase chapter)
		{
			 _configManager.ChangeModChapter(mod.DirInfo, new DirectoryInfo(chapter.Path), Config);
			mod.DirInfo.MoveTo($@"{chapter.Path}\{mod.Name}");

			return new Mod(mod.DirInfo, chapter, mod.Size);


			//await Task.Run(async () =>
			//{
			//	await _configManager.ChangeModChapter(mod.DirInfo, chapter, Config);

			//	string toPath = @$"{Settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}";
			//	if(chapter.Name == AppConsts.MODS_ROOT_DIR)
			//	{
			//		toPath += @$"\{mod.DirInfo.Name}";
			//	}
			//	else
			//	{
			//		toPath += @$"\{chapter.Name}\{mod.DirInfo.Name}";
			//	}

			//	mod.DirInfo.MoveTo(toPath);
			//});
		}


		public List<Mod> GetMods()
		{
			List<ChapterBase> chapters = GetChapters();
			return GetMods(chapters);
		}

		public List<Mod> GetMods(IEnumerable<ChapterBase> chapters)
		{
			if (!IsCorrectMRRootDir)
				return [];

			List<Mod> mods = [];

			foreach (var chapter in chapters) 
			{
				if (!chapter.IsRoot)
					mods.AddRange(GetChapterMods(chapter));
				else
					mods.AddRange(GetRootChapterMods(chapter, chapters));
			}

			return mods;
		}

		public List<ChapterBase> GetChapters()
		{
			if (!IsCorrectMRRootDir)
				return [];

			var chapters = new List<ChapterBase>();

			IEnumerable<ChapterInfo> chapterInfos = _chapterInfosRepo.Get(Settings.MudRunnerRootDir);

			ChapterInfo rootChapterInfo = new(
				AppConsts.MODS_ROOT_DIR,
				$@"{Settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}"
			);
			chapters.Add(new ChapterBase(rootChapterInfo));

			foreach (var chapterInfo in chapterInfos)
			{
				chapters.Add(new ChapterBase(chapterInfo));
			}

			return chapters;
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

		private static List<Mod> GetChapterMods(ChapterBase chapter)
		{
			if (!chapter.Exists)
				return [];

			List<Mod> mods = [];

			DirectoryInfo chDirInfo = new(chapter.Path);

			DirectoryInfo[] chapterDirs = chDirInfo.GetDirectories();

			foreach (var dir in chapterDirs) 
			{
				mods.Add(new Mod(dir, chapter, GetModSize(dir)));
			}

			return mods;
		}

		private static List<Mod> GetRootChapterMods(ChapterBase rootChapter, IEnumerable<ChapterBase> allChapters)
		{
			if (!rootChapter.Exists)
				return [];

			List<Mod> mods = [];

			HashSet<string> chapterNames = new(allChapters.Where(ch => !ch.IsRoot).Select(ch => ch.Name));

			DirectoryInfo chDirInfo = new(rootChapter.Path);

			IEnumerable<DirectoryInfo> chapterDirs = chDirInfo.GetDirectories().Where(dir => !chapterNames.Contains(dir.Name));

			foreach (var dir in chapterDirs)
			{
				mods.Add(new Mod(dir, rootChapter, GetModSize(dir)));
			}

			return mods;
		}
		
		//todo: пока дубль. переделать
		private static long GetModSize(DirectoryInfo modDir)
		{
			long size = 0;

			FileInfo[] files = modDir.GetFiles();
			foreach (FileInfo file in files)
			{
				size += file.Length;
			}

			DirectoryInfo[] dirs = modDir.GetDirectories();
			foreach (DirectoryInfo dir in dirs)
			{
				size += GetModSize(dir);
			}
			return size;
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

	public class ModBase
	{
		public ModBase(DirectoryInfo directoryInfo)
		{
			DirInfo = directoryInfo;
		}

		public DirectoryInfo DirInfo { get; }

		public bool Exists => Directory.Exists(Path);

		public string Name => DirInfo.Name;

		public string Path => DirInfo.FullName;
	}

	public class Mod : ModBase
	{
		public Mod(DirectoryInfo directoryInfo, ChapterBase chapter, long size) : base(directoryInfo)
		{
			Chapter = chapter;
			Size = size;
		}

		public ChapterBase Chapter { get; }

		public long Size { get; }
	}

}
