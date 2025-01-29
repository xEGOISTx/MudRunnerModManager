using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.Exstensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.Models
{
	public class ChaptersModel : IChaptersModel
	{
		private readonly IChapterInfosRepo _chapterInfosRepo;
		private readonly string _gameRootPath;
		private readonly string _relativePathToRootModsDir;

		public ChaptersModel(IChapterInfosRepo chapterInfosRepo, 
			string gameRootPath, 
			string relativePathToRootModsDir)
		{
			_chapterInfosRepo = chapterInfosRepo;
			_gameRootPath = gameRootPath;
			_relativePathToRootModsDir = relativePathToRootModsDir;
		}


		public Chapter AddChapter(string chapterName)
		{
			var modsRootDirName = new DirectoryInfo(_relativePathToRootModsDir).Name;

			if (string.IsNullOrWhiteSpace(chapterName) || modsRootDirName == null
				|| chapterName == modsRootDirName) 
				throw new Exception("Impossible add chapter");

			DirectoryInfo chapter = new($@"{_gameRootPath}\{_relativePathToRootModsDir}\{chapterName}");

			if (!chapter.Exists)
				chapter.Create();

			ChapterInfo newChapter = new(chapter.Name, chapter.FullName);
			_chapterInfosRepo.Add(newChapter);

			return new Chapter(newChapter, 0, false);
		}

		public void DeleteChapter(Chapter chapter)
		{
			if (chapter == null || chapter.IsRoot)
				return;

			DirectoryInfo chapterInfo = new(chapter.Path);
			if (chapterInfo.Exists)
				chapterInfo.Delete(true);

			_chapterInfosRepo.Delete(chapter.ChapterInfo);
		}

		public IEnumerable<Chapter> GetChapters()
		{
			var chapters = new List<Chapter>();

			IEnumerable<ChapterInfo> chapterInfos =  _chapterInfosRepo.Get(_gameRootPath);

			foreach (var chapterInfo in chapterInfos)
			{
				if(Directory.Exists(chapterInfo.Path))
				{
					Chapter chapter = new(chapterInfo, GetChapterSize(chapterInfo), false);
					chapters.Add(chapter);
				}
			}

			return chapters;
		}

		public Chapter RenameChapter(Chapter chapter, string newName)
		{
			if (chapter.IsRoot || !chapter.Exists || chapter.Name == newName)
				return chapter;

			DirectoryInfo chapterInfo = new(chapter.Path);

			string newChapterPath = $@"{chapterInfo.Parent.FullName}\{newName}";
			ChapterInfo chapterNewName = new(newName, newChapterPath);

			_chapterInfosRepo.Rename(chapter.ChapterInfo, chapterNewName);
			chapterInfo.MoveTo(newChapterPath);

			return new Chapter(chapterNewName, chapter.Size, chapter.IsRoot);
		}

		public HashSet<string> GetRootChapterModNames()
		{
			DirectoryInfo rootChDirInfo = new($@"{_gameRootPath}\{_relativePathToRootModsDir}");
			if (!rootChDirInfo.Exists)
				return [];

			List<string> mods = [];

			HashSet<string> chapterNames = new(_chapterInfosRepo.Get(_gameRootPath).Select(ch => ch.Name));

			return rootChDirInfo.GetDirectories().Where(dir => !chapterNames.Contains(dir.Name)).Select(dir => dir.Name).ToHashSet();
		}

		private static long GetChapterSize(ChapterInfo chapterInfo)
		{
			DirectoryInfo chapter = new(chapterInfo.Path);
			return chapter.GetSize();
		}
	}

	public class ChapterBase
	{
		public ChapterBase(ChapterInfo chapterInfo, bool isRoot) 
		{
			ChapterInfo = chapterInfo;
			IsRoot = isRoot; 
		}

		public ChapterInfo ChapterInfo { get; }

		public bool Exists => Directory.Exists(ChapterInfo.Path);

		public string Name => ChapterInfo.Name;

		public string Path => ChapterInfo.Path;

		public bool IsRoot { get; }
	}

	public class Chapter : ChapterBase
	{
		public Chapter(ChapterInfo chapterInfo, long size, bool isRoot) : base(chapterInfo, isRoot)
		{
			Size = size;
		}
		public long Size {  get; }
	}

}
