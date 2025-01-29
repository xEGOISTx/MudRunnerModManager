using System.IO;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile
{
	public interface IModPathsEditor
	{
		void AddModPath(DirectoryInfo modDir);
		void DeleteModPath(DirectoryInfo modDir);
		void RenameMod(DirectoryInfo modDir, string newModName);
		void RenameChapter(DirectoryInfo chapter, string newName);
		void ChangeModChapter(DirectoryInfo modDir, DirectoryInfo newChapter);
		void DeleteChapter(DirectoryInfo chapter);
	}
}
