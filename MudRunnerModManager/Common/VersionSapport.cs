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

			var chaptersRelocated = RelocateChapters(settings);
			var rootPathRelocated = RelocateMRRootPath(settings);

			if (chaptersRelocated || rootPathRelocated)
				settings.Save();
		}

		private bool RelocateChapters(XmlDoc settings)
		{
			XmlDoc chapters = new(AppPaths.XmlChaptersFilePath);
			if (chapters.Exists)
				return false;

			if (!settings.IsPresentElem(new XmlElem(XmlConsts.CHAPTERS)))
				return false;

			List<XmlElem> capterSettElems = settings.GetXmlItems<XmlElem>(elem => elem.Name == XmlConsts.CHAPTER);
			if (capterSettElems.Count == 0)
				return false;

			chapters.LoadOrCreate();

			XmlElem chElem = new(XmlConsts.CHAPTERS);
			chapters.AddRootXmlElem(chElem);

			List<XmlElem> chapterElems = [];
			foreach (var chapterSettElem in capterSettElems)
			{
				XmlElem chapter = new(XmlConsts.CHAPTER);
				DirectoryInfo chInfo = new(chapterSettElem.Attributes.First().Value);
				chapter.Attributes.Add(new XmlElemAttribute(XmlConsts.PATH, chInfo.FullName));
				chapter.Attributes.Add(new XmlElemAttribute(XmlConsts.NAME, chInfo.Name));

				chapterElems.Add(chapter);
			}

			chapters.AddRangeXmlElems(chapterElems, XmlConsts.CHAPTERS);

			chapters.Save();

			settings.RemoveXmlNodeElem(chElem);

			return true;
		}

		private bool RelocateMRRootPath(XmlDoc settings)
		{
			XmlDoc gamesRootPaths = new(AppPaths.XmlGameRootPathsFilePath);
			if (gamesRootPaths.Exists)
				return false;

			XmlElem? mrRootSettElem = settings.GetXmlItem<XmlElem>(elem => elem.Name == XmlConsts.MUDRUNNER_ROOT);

			if(mrRootSettElem == null) 
				return false;

			gamesRootPaths.LoadOrCreate();

			XmlElem rootElem = new(XmlConsts.GAMES_ROOT_PATHS);
			gamesRootPaths.AddRootXmlElem(rootElem);

			XmlElem mrRootPath = new XmlElem(XmlConsts.GAME);
			mrRootPath.Attributes.Add(new XmlElemAttribute(XmlConsts.PATH, mrRootSettElem.Attributes.First().Value));
			mrRootPath.Attributes.Add(new XmlElemAttribute(XmlConsts.NAME, GameName.MUD_RUNNER));
			gamesRootPaths.AddXmlElem(mrRootPath, XmlConsts.GAMES_ROOT_PATHS);

			gamesRootPaths.Save();

			settings.RemoveXmlElem(mrRootSettElem);

			return true;
		}
	}
}
