using System.Collections.Generic;

namespace MudRunnerModManager.Common.AppRepo
{
	public interface IChapterInfosRepo
	{
		void Add(ChapterInfo chapter);
		void Delete(ChapterInfo chapter);
		IEnumerable<ChapterInfo> GetAll();
		IEnumerable<ChapterInfo> Get(string gameRootPaht);
		void Rename(ChapterInfo chapter, ChapterInfo newName);
	}

	public class ChapterInfo(string name, string path)
	{
		public string Name { get; } = name;
		public string Path { get; } = path;
	}
}
