using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.Exstensions;
using MudRunnerModManager.Models.ArchiveWorker;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.Models
{
    public class ModsModel : IModsModel
	{
		//todo: в будущем нужна абстракция для ModExtractor
		private readonly ModExtractor _modExtractor = new(new ArchiveExtractor());
		private readonly IChapterInfosRepo _chapterInfosRepo;
		private readonly string _gameRootPath;
		private readonly ISettingsRepo _settingsRepo;
		private readonly string _relativePathToRootModsDir;

		public ModsModel(IChapterInfosRepo chapterInfosRepo, 
			ISettingsRepo settingsRepo, 
			string gameRootPath, 
			string relativePathToRootModsDir)
		{
			_chapterInfosRepo = chapterInfosRepo;
			_settingsRepo = settingsRepo;
			_gameRootPath = gameRootPath;
			_relativePathToRootModsDir = relativePathToRootModsDir;
		}

		public Mod AddMod(FileInfo modArchive, string modName, ChapterBase chapter)
		{
			if (!modArchive.Exists || string.IsNullOrWhiteSpace(modName))
				throw new System.Exception("Impossible add mod");

			DirectoryInfo modDectinationDir = new($@"{chapter.Path}\{modName}");

			var res = _modExtractor.Extract(modArchive, modDectinationDir);
			if (!res)
				throw new System.Exception("Failed unzip archive");

			return new Mod(modDectinationDir, chapter, modDectinationDir.GetSize());
		}

		public void DeleteMod(Mod mod)
		{
			if (!mod.DirInfo.Exists)
				return;

			Directory.Delete(mod.DirInfo.FullName, true);
		}

		public Mod RenameMod(Mod mod, string modName)
		{
			mod.DirInfo.MoveTo(@$"{mod.DirInfo.Parent}\{modName}");
			return new Mod(mod.DirInfo, mod.Chapter, mod.Size);
		}

		public Mod RelocateMod(Mod mod, ChapterBase chapter)
		{
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
			var chapters = new List<ChapterBase>();

			IEnumerable<ChapterInfo> chapterInfos = _chapterInfosRepo.Get(_gameRootPath);

			var modsRootDirName = new DirectoryInfo(_relativePathToRootModsDir).Name;

			ChapterInfo rootChapterInfo = new(
				modsRootDirName ?? throw new System.Exception($"Invalid path {_relativePathToRootModsDir}"),
				$@"{_gameRootPath}\{_relativePathToRootModsDir}"
			);
			chapters.Add(new ChapterBase(rootChapterInfo, true));

			foreach (var chapterInfo in chapterInfos)
			{
				chapters.Add(new ChapterBase(chapterInfo, false));
			}

			return chapters;
		}

		public Settings GetSettings()
		{
			return _settingsRepo.Load();
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
