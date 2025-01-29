using System.Collections.Generic;

namespace MudRunnerModManager.Models
{
	public interface IChaptersModel
	{
		Chapter AddChapter(string chapterName);
		void DeleteChapter(Chapter chapter);
		IEnumerable<Chapter> GetChapters();
		Chapter RenameChapter(Chapter chapter, string newName);
		HashSet<string> GetRootChapterModNames();
	}
}
