using MudRunnerModManager.Common.XmlWorker;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.Common
{
	public class VersionSapport
	{
		public void Execute()
		{
			RelocateChaptersFromSettingsToChaptersRep_v2_0_0_v2_1_3();
		}


		private void RelocateChaptersFromSettingsToChaptersRep_v2_0_0_v2_1_3()
		{
			XmlDoc chapters = new(AppPaths.XmlChaptersFilePath);
			if (chapters.Exists)
				return;

			XmlDoc settings = new(AppPaths.XmlSettingsFilePath);

			if (!settings.Exists)
				return;

			settings.Load();

			if (settings.IsEmpty || !settings.IsPresentElem(new XmlElem(AppConsts.CHAPTERS)))
				return;

			List<XmlElem> capterSettElems = settings.GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.CHAPTER);

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
			settings.Save();
		}
	}
}
