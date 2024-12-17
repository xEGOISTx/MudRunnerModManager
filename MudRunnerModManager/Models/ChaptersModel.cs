using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Common.AppSettings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.Models
{
	public class ChaptersModel
	{
		private readonly IChapterInfosRepo _chapterInfosRepo;
		private readonly SettingsBase _settings;
		private readonly ConfigManager _configManager = new();

		public ChaptersModel(IChapterInfosRepo chapterInfosRepo, SettingsBase settings)
		{
			_chapterInfosRepo = chapterInfosRepo;
			_settings = settings;
		}

		public bool IsCorrectMRRootDir => !string.IsNullOrEmpty(_settings.MudRunnerRootDir)
											&& File.Exists(@$"{_settings.MudRunnerRootDir.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}")
											&& File.Exists(@$"{_settings.MudRunnerRootDir.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}");

		public Chapter AddChapter(string chapterName)
		{
			if (string.IsNullOrWhiteSpace(chapterName) 
				|| !IsCorrectMRRootDir
				|| chapterName == AppConsts.MODS_ROOT_DIR) 
				throw new Exception("Impossible add chapter");

			DirectoryInfo chapter = new($@"{_settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapterName}");

			if (!chapter.Exists)
				chapter.Create();

			ChapterInfo newChapter = new(chapter.Name, chapter.FullName);
			_chapterInfosRepo.Add(newChapter);

			return new Chapter(newChapter, 0);
		}

		public void DeleteChapter(Chapter chapter)
		{
			if (chapter == null || chapter.IsRoot || !IsCorrectMRRootDir)
				return;

			DirectoryInfo chapterInfo = new(chapter.Path);
			if (chapterInfo.Exists)
			{
				chapterInfo.Delete(true);
				_configManager.DeleteChapter(chapterInfo, new FileInfo($@"{_settings.MudRunnerRootDir}\{AppConsts.CONFIG_XML}"));
			}

			_chapterInfosRepo.Delete(chapter.ChapterInfo);
		}

		public IEnumerable<Chapter> GetChapters()
		{
			var chapters = new List<Chapter>();

			if(!IsCorrectMRRootDir)
				return chapters;

			IEnumerable<ChapterInfo> chapterInfos =  _chapterInfosRepo.Get(_settings.MudRunnerRootDir);

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
			if (chapter.IsRoot || !IsCorrectMRRootDir || !chapter.Exists || chapter.Name == newName)
				return chapter;

			DirectoryInfo chapterInfo = new(chapter.Path);
			_configManager.RenameChapter(chapterInfo, newName, new FileInfo($@"{_settings.MudRunnerRootDir}\{AppConsts.CONFIG_XML}"));

			string newChapterPath = $@"{chapterInfo.Parent.FullName}\{newName}";
			ChapterInfo chapterNewName = new(newName, newChapterPath);

			_chapterInfosRepo.Rename(chapter.ChapterInfo, chapterNewName);
			chapterInfo.MoveTo(newChapterPath);

			return new Chapter(chapterNewName, chapter.Size);
		}

		public HashSet<string> GetRootChapterModNames()
		{
			if (!IsCorrectMRRootDir)
				return [];

			List<string> mods = [];

			HashSet<string> chapterNames = new(_chapterInfosRepo.Get(_settings.MudRunnerRootDir).Select(ch => ch.Name));

			DirectoryInfo rootChDirInfo = new($@"{_settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}");

			return rootChDirInfo.GetDirectories().Where(dir => !chapterNames.Contains(dir.Name)).Select(dir => dir.Name).ToHashSet();
		}

		private static long GetChapterSize(ChapterInfo chapterInfo)
		{
			DirectoryInfo chapter = new(chapterInfo.Path);
			return GetChapterSize(chapter);
		}

		//todo: пока дубль. переделать
		private static long GetChapterSize(DirectoryInfo chapter)
		{
			long size = 0;

			FileInfo[] files = chapter.GetFiles();
			foreach (FileInfo file in files)
			{
				size += file.Length;
			}

			DirectoryInfo[] dirs = chapter.GetDirectories();
			foreach (DirectoryInfo dir in dirs)
			{
				size += GetChapterSize(dir);
			}
			return size;
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
