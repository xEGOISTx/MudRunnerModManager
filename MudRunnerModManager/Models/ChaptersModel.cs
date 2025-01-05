using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.Exstensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.Models
{
	public class ChaptersModel
	{
		private readonly IChapterInfosRepo _chapterInfosRepo;
		private readonly ConfigManager _configManager = new();
		private readonly string _gameRootPath;

		public ChaptersModel(IChapterInfosRepo chapterInfosRepo, string gameRootPath)
		{
			_chapterInfosRepo = chapterInfosRepo;
			_gameRootPath = gameRootPath;
		}


		public Chapter AddChapter(string chapterName)
		{
			if (string.IsNullOrWhiteSpace(chapterName) 
				|| chapterName == AppConsts.MODS_ROOT_DIR) 
				throw new Exception("Impossible add chapter");

			DirectoryInfo chapter = new($@"{_gameRootPath}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapterName}");

			if (!chapter.Exists)
				chapter.Create();

			ChapterInfo newChapter = new(chapter.Name, chapter.FullName);
			_chapterInfosRepo.Add(newChapter);

			return new Chapter(newChapter, 0);
		}

		public void DeleteChapter(Chapter chapter)
		{
			if (chapter == null || chapter.IsRoot)
				return;

			DirectoryInfo chapterInfo = new(chapter.Path);
			if (chapterInfo.Exists)
			{
				chapterInfo.Delete(true);
				_configManager.DeleteChapter(chapterInfo, new FileInfo($@"{_gameRootPath}\{AppConsts.CONFIG_XML}"));
			}

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
					Chapter chapter = new(chapterInfo, GetChapterSize(chapterInfo));
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
			_configManager.RenameChapter(chapterInfo, newName, new FileInfo($@"{_gameRootPath}\{AppConsts.CONFIG_XML}"));

			string newChapterPath = $@"{chapterInfo.Parent.FullName}\{newName}";
			ChapterInfo chapterNewName = new(newName, newChapterPath);

			_chapterInfosRepo.Rename(chapter.ChapterInfo, chapterNewName);
			chapterInfo.MoveTo(newChapterPath);

			return new Chapter(chapterNewName, chapter.Size);
		}

		public HashSet<string> GetRootChapterModNames()
		{
			List<string> mods = [];

			HashSet<string> chapterNames = new(_chapterInfosRepo.Get(_gameRootPath).Select(ch => ch.Name));

			DirectoryInfo rootChDirInfo = new($@"{_gameRootPath}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}");

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
		public ChapterBase(ChapterInfo chapterInfo) 
		{
			ChapterInfo = chapterInfo;
		}

		public ChapterInfo ChapterInfo { get; }

		public bool Exists => Directory.Exists(ChapterInfo.Path);

		public string Name => ChapterInfo.Name;

		public string Path => ChapterInfo.Path;

		public bool IsRoot => ChapterInfo.Name == AppConsts.MODS_ROOT_DIR;
	}

	public class Chapter : ChapterBase
	{
		public Chapter(ChapterInfo chapterInfo, long size) : base(chapterInfo)
		{
			Size = size;
		}
		public long Size {  get; }
	}

}
