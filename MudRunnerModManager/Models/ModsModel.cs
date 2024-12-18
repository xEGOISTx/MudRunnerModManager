using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.AppSettings;
using MudRunnerModManager.Common.Exstensions;
using MudRunnerModManager.Models.ArchiveWorker;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		public Mod AddMod(FileInfo modArchive, string modName, ChapterBase chapter)
		{
			if (!modArchive.Exists || !IsCorrectMRRootDir || string.IsNullOrWhiteSpace(modName))
				throw new System.Exception("Impossible add mod");

			DirectoryInfo modDectinationDir = new($@"{chapter.Path}\{modName}");

			var res = _modExtractor.Extract(modArchive, modDectinationDir);
			if (!res)
				throw new System.Exception("Failed unzip archive");

			_configManager.AddModPath(modDectinationDir, Config);

			ModFileCorrection(modDectinationDir);

			return new Mod(modDectinationDir, chapter, modDectinationDir.GetSize());
		}

		public void DeleteMod(Mod mod)
		{
			if (!mod.DirInfo.Exists || !IsCorrectMRRootDir)
				return;

			Directory.Delete(mod.DirInfo.FullName, true);
			_configManager.DeleteModPath(mod.DirInfo, Config);
		}

		public Mod RenameMod(Mod mod, string modName)
		{
			_configManager.RenameMod(mod.DirInfo, modName, Config);
			mod.DirInfo.MoveTo(@$"{mod.DirInfo.Parent}\{modName}");

			return new Mod(mod.DirInfo, mod.Chapter, mod.Size);
		}

		public Mod RelocateMod(Mod mod, ChapterBase chapter)
		{
			 _configManager.ChangeModChapter(mod.DirInfo, new DirectoryInfo(chapter.Path), Config);
			mod.DirInfo.MoveTo($@"{chapter.Path}\{mod.Name}");

			return new Mod(mod.DirInfo, chapter, mod.Size);
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

		private static List<Mod> GetChapterMods(ChapterBase chapter)
		{
			if (!chapter.Exists)
				return [];

			List<Mod> mods = [];

			DirectoryInfo chDirInfo = new(chapter.Path);

			DirectoryInfo[] chapterDirs = chDirInfo.GetDirectories();

			foreach (var modDir in chapterDirs) 
			{
				mods.Add(new Mod(modDir, chapter, modDir.GetSize()));
			}

			return mods;
		}

		private static List<Mod> GetRootChapterMods(ChapterBase rootChapter, IEnumerable<ChapterBase> allChapters)
		{
			if (!rootChapter.Exists)
				return [];

			List<Mod> mods = [];

			HashSet<string> chapterNames = new(allChapters.Where(ch => !ch.IsRoot).Select(ch => ch.Name));

			DirectoryInfo rootChDirInfo = new(rootChapter.Path);

			IEnumerable<DirectoryInfo> rootChapterDirs = rootChDirInfo.GetDirectories().Where(dir => !chapterNames.Contains(dir.Name));

			foreach (var modDir in rootChapterDirs)
			{
				mods.Add(new Mod(modDir, rootChapter, modDir.GetSize()));
			}

			return mods;
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
