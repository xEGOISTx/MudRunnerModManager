using MudRunnerModManager.Common.XmlWorker;
using MudRunnerModManager.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;

namespace MudRunnerModManager.Common
{
	public class VersionSapport
	{
		public void Execute()
		{
			RelocateItemsFromSettingsToReps_v2_0_0_v2_1_3();
		}


		private void RelocateItemsFromSettingsToReps_v2_0_0_v2_1_3()
		{
			XmlDoc settings = new(AppPaths.XmlSettingsFilePath);

			if (!settings.Exists)
				return;

			settings.Load();

			if (settings.IsEmpty)
				return;

			RelocateChapters(settings);
			RelocateMRRootPath(settings);

			settings.Save();
		}

		private void RelocateChapters(XmlDoc settings)
		{
			XmlDoc chapters = new(AppPaths.XmlChaptersFilePath);
			if (chapters.Exists)
				return;

			if (!settings.IsPresentElem(new XmlElem(AppConsts.CHAPTERS)))
				return;

			List<XmlElem> capterSettElems = settings.GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.CHAPTER);
			if (capterSettElems.Count == 0)
				return;

			chapters.LoadOrCreate();

			XmlElem chElem = new(AppConsts.CHAPTERS);
			chapters.AddRootXmlElem(chElem);

			List<XmlElem> chapterElems = [];
			foreach (var chapterSettElem in capterSettElems)
			{
				XmlElem chapter = new(AppConsts.CHAPTER);
				DirectoryInfo chInfo = new(chapterSettElem.Attributes.First().Value);
				chapter.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, chInfo.FullName));
				chapter.Attributes.Add(new XmlElemAttribute(AppConsts.NAME, chInfo.Name));

				chapterElems.Add(chapter);
			}

			chapters.AddRangeXmlElems(chapterElems, AppConsts.CHAPTERS);

			chapters.Save();

			settings.RemoveXmlNodeElem(chElem);
		}

		private void RelocateMRRootPath(XmlDoc settings)
		{
			XmlDoc gamesRootPaths = new(AppPaths.XmlGameRootPathsFilePath);
			if (gamesRootPaths.Exists)
				return;

			XmlElem? mrRootSettElem = settings.GetXmlItem<XmlElem>(elem => elem.Name == SettingsConsts.MUDRUNNER_ROOT);

			if(mrRootSettElem == null) 
				return;

			gamesRootPaths.LoadOrCreate();

			XmlElem rootElem = new(AppConsts.GAMES_ROOT_PATHS);
			gamesRootPaths.AddRootXmlElem(rootElem);

			XmlElem mrRootPath = new XmlElem(AppConsts.GAME);
			mrRootPath.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, mrRootSettElem.Attributes.First().Value));
			mrRootPath.Attributes.Add(new XmlElemAttribute(AppConsts.NAME, GameName.MUDRUNNER));
			gamesRootPaths.AddXmlElem(mrRootPath, AppConsts.GAMES_ROOT_PATHS);

			gamesRootPaths.Save();

			settings.RemoveXmlElem(mrRootSettElem);
		}
	}
}
