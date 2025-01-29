using MudRunnerModManager.Common.XmlWorker;
using System.Collections.Generic;
using System.Linq;

namespace MudRunnerModManager.Common.AppRepo
{
	public class XmlChapterInfosRepo : IChapterInfosRepo
	{
		private readonly string _filePath;

		public XmlChapterInfosRepo(string filePath) 
		{ 
			_filePath = filePath;
		}

		public void Add(ChapterInfo chapter)
		{
			if (chapter == null 
				|| string.IsNullOrWhiteSpace(chapter.Path)
				|| string.IsNullOrWhiteSpace(chapter.Name))
				return;

			XmlDoc xmlChapters = new(_filePath);

			xmlChapters.LoadOrCreate();

			void AddRoot()
			{
				XmlElem chElem = new(XmlConsts.CHAPTERS);
				xmlChapters.AddRootXmlElem(chElem);
			}
		
			if (xmlChapters.IsEmpty)
			{
				AddRoot();
			}
			else if(!xmlChapters.IsPresentElem(new(XmlConsts.CHAPTERS)))
			{
				xmlChapters.Clear();
				AddRoot();
			}

			XmlElem chapterElem = CreateChapterElem(chapter);

			if (!xmlChapters.IsPresentElem(chapterElem))
			{
				xmlChapters.AddXmlElem(chapterElem, XmlConsts.CHAPTERS);
				xmlChapters.Save();
			}
		}

		public void Delete(ChapterInfo chapter)
		{
			if (chapter == null)
				return;

			XmlDoc xmlChapters = new(_filePath);
			if (!xmlChapters.Exists)
				return;

			xmlChapters.Load();

			if (xmlChapters.IsEmpty)
				return;

			var delChapter = CreateChapterElem(chapter);

			if (xmlChapters.IsPresentElem(delChapter))
			{
				xmlChapters.RemoveXmlElem(delChapter);
				xmlChapters.Save();
			}				
		}

		public IEnumerable<ChapterInfo> GetAll()
		{
			XmlDoc xmlChapters = new(_filePath);
			if (!xmlChapters.Exists)
				return [];

			xmlChapters.Load();

			if (xmlChapters.IsEmpty)
				return [];

			IEnumerable<XmlElem> chapters = GetChapterXmlElems(xmlChapters);
			return CreateChapterInfos(chapters);
		}

		public IEnumerable<ChapterInfo> Get(string gameRootPath)
		{
			XmlDoc xmlChapters = new(_filePath);
			if (!xmlChapters.Exists)
				return [];

			xmlChapters.Load();

			if (xmlChapters.IsEmpty)
				return [];

			IEnumerable<XmlElem> chapters = GetChapterXmlElems(xmlChapters, gameRootPath);
			return CreateChapterInfos(chapters);
		}

		public void Rename(ChapterInfo chapter, ChapterInfo newName)
		{
			if (chapter == null 
				|| string.IsNullOrWhiteSpace(newName.Path)
				|| string.IsNullOrWhiteSpace(newName.Name))
				return;

			XmlDoc xmlChapters = new(_filePath);
			if (!xmlChapters.Exists)
				return;

			xmlChapters.Load();

			if (xmlChapters.IsEmpty)
				return;

			var oldElem = CreateChapterElem(chapter);

			if (!xmlChapters.IsPresentElem(oldElem))
				return;

			var newElem = CreateChapterElem(newName);

			xmlChapters.ReplaceXmlElem(oldElem, newElem, XmlConsts.CHAPTERS);

			xmlChapters.Save();
		}

		private XmlElem CreateChapterElem(ChapterInfo chapter)
		{
			XmlElem chapterElem = new(XmlConsts.CHAPTER);
			chapterElem.Attributes.Add(new XmlElemAttribute(XmlConsts.PATH, chapter.Path));
			chapterElem.Attributes.Add(new XmlElemAttribute(XmlConsts.NAME, chapter.Name));
			return chapterElem;
		}

		private IEnumerable<ChapterInfo> CreateChapterInfos(IEnumerable<XmlElem> chapterXmlElens)
		{
			List<ChapterInfo> chapterInfos = [];

			foreach (XmlElem chapter in chapterXmlElens)
			{
				string? chapterPath = chapter.Attributes.FirstOrDefault(atr => atr.Name == XmlConsts.PATH)?.Value;
				string? chapterName = chapter.Attributes.FirstOrDefault(atr => atr.Name == XmlConsts.NAME)?.Value;
				if (!string.IsNullOrWhiteSpace(chapterPath) && !string.IsNullOrWhiteSpace(chapterName))
					chapterInfos.Add(new(chapterName, chapterPath));
			}

			return chapterInfos;
		}

		private IEnumerable<XmlElem> GetChapterXmlElems(XmlDoc xmlChapters, string? gameRootPaht = null)
		{
			if(gameRootPaht == null)
				return xmlChapters.GetXmlItems<XmlElem>(elem => elem.Name == XmlConsts.CHAPTER);
			else
				return xmlChapters.GetXmlItems<XmlElem>(elem => elem.Name == XmlConsts.CHAPTER
						&& elem.Attributes.Count > 0
						&& elem.Attributes.FirstOrDefault(atr => atr.Name == XmlConsts.PATH) != null
						&& elem.Attributes.First(atr => atr.Name == XmlConsts.PATH).Value.Contains(gameRootPaht));
		}
	}
}
