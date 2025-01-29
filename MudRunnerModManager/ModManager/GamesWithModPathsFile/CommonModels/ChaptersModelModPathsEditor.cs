using MudRunnerModManager.Models;
using System.Collections.Generic;
using System.IO;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.CommonModels
{
	public class ChaptersModelModPathsEditor : IChaptersModel
	{
		private readonly IChaptersModel _chaptersModel;
		private readonly IModPathsEditor _modPathsEditor;

		public ChaptersModelModPathsEditor(IChaptersModel model, IModPathsEditor modPathsEditor)
		{
			_chaptersModel = model;
			_modPathsEditor = modPathsEditor;
		}


		public Chapter AddChapter(string chapterName)
		{
			return _chaptersModel.AddChapter(chapterName);
		}

		public void DeleteChapter(Chapter chapter)
		{
			_chaptersModel.DeleteChapter(chapter);
			_modPathsEditor.DeleteChapter(new DirectoryInfo(chapter.Path));
		}

		public IEnumerable<Chapter> GetChapters()
		{
			return _chaptersModel.GetChapters();
		}

		public HashSet<string> GetRootChapterModNames()
		{
			return _chaptersModel.GetRootChapterModNames();
		}

		public Chapter RenameChapter(Chapter chapter, string newName)
		{
			DirectoryInfo oldChapter = new(chapter.Path);
			Chapter renamedChapter = _chaptersModel.RenameChapter(chapter, newName);
			_modPathsEditor.RenameChapter(oldChapter, newName);
			return renamedChapter;
		}
	}
}
