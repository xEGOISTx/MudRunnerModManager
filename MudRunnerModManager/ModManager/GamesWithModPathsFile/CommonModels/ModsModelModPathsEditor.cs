using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.CommonModels
{
	public class ModsModelModPathsEditor : IModsModel
	{
		private readonly IModsModel _modsModel;
		private readonly IModPathsEditor _modPathsEditor;

		public ModsModelModPathsEditor(IModsModel modsModel, IModPathsEditor modPathsEditor)
		{
			_modsModel = modsModel;
			_modPathsEditor = modPathsEditor;
		}


		public Mod AddMod(FileInfo modArchive, string modName, ChapterBase chapter)
		{
			DirectoryInfo modDectinationDir = new($@"{chapter.Path}\{modName}");
			Mod newMod = _modsModel.AddMod(modArchive, modName, chapter);
			ModFileCorrection(modDectinationDir);
			_modPathsEditor.AddModPath(modDectinationDir);
			return newMod;
		}

		public void DeleteMod(Mod mod)
		{
			_modsModel.DeleteMod(mod);
			_modPathsEditor.DeleteModPath(mod.DirInfo);
		}

		public List<ChapterBase> GetChapters()
		{
			return _modsModel.GetChapters();
		}

		public List<Mod> GetMods()
		{
			return _modsModel.GetMods();
		}

		public List<Mod> GetMods(IEnumerable<ChapterBase> chapters)
		{
			return _modsModel.GetMods(chapters);
		}

		public Settings GetSettings()
		{
			return _modsModel.GetSettings();
		}

		public Mod RelocateMod(Mod mod, ChapterBase chapter)
		{
			DirectoryInfo oldDir = new(mod.DirInfo.FullName);
			Mod relocMod = _modsModel.RelocateMod(mod, chapter);
			_modPathsEditor.ChangeModChapter(oldDir, new DirectoryInfo(chapter.Path));
			return relocMod;
		}

		public Mod RenameMod(Mod mod, string modName)
		{
			DirectoryInfo oldDir = new(mod.DirInfo.FullName);
			Mod renamedMod = _modsModel.RenameMod(mod, modName);
			_modPathsEditor.RenameMod(oldDir, modName);
			return renamedMod;
		}

		//todo: пока будет тут
		private void ModFileCorrection(DirectoryInfo modRootDir)
		{
			DirectoryInfo levelsDir = new DirectoryInfo(@$"{modRootDir.FullName}\levels");

			if (!levelsDir.Exists)
				return;

			var extensions = new[] { "*.dds", "*.stg" };
			var files = extensions.SelectMany(ext => levelsDir.GetFiles(ext));

			foreach (var file in files)
			{
				if (!file.Name.StartsWith("level_"))
				{
					file.MoveTo($@"{file.Directory}\level_{file.Name}", true);
				}
			}
		}
	}
}
