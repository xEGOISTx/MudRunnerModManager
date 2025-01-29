using MudRunnerModManager.Common.AppRepo;
using System.Collections.Generic;
using System.IO;

namespace MudRunnerModManager.Models
{
	public interface IModsModel
	{
		Mod AddMod(FileInfo modArchive, string modName, ChapterBase chapter);
		void DeleteMod(Mod mod);
		Mod RenameMod(Mod mod, string modName);
		Mod RelocateMod(Mod mod, ChapterBase chapter);
		List<Mod> GetMods();
		List<Mod> GetMods(IEnumerable<ChapterBase> chapters);
		List<ChapterBase> GetChapters();
		Settings GetSettings();
	}
}
