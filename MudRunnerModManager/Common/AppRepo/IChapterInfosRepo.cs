using System.Collections.Generic;
using System.Threading.Tasks;

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
}
