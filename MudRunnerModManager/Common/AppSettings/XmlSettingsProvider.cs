using MudRunnerModManager.Common.XmlWorker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SConsts = MudRunnerModManager.Common.SettingsConsts;

namespace MudRunnerModManager.Common.AppSettings
{
    public class XmlSettingsProvider : ISettingsProvider
    {
        private readonly string _filePath;

        public XmlSettingsProvider(string filePath)
        {
            _filePath = filePath;
        }

        public async Task LoadAsync(ISettings settings)
        {
            XmlDoc xmlSettings = new(_filePath);
            if (!xmlSettings.Exists)
                return;

            await xmlSettings.LoadAsync();

            await Task.Run(() =>
            {
                XmlElem? mudRunnerRoot = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.MUDRUNNER_ROOT);
                if (mudRunnerRoot is not null)
                {
                    string? mrRoot = mudRunnerRoot.Attributes.FirstOrDefault(atr => atr.Name == SConsts.PATH)?.Value;
                    if (mrRoot != null)
                        settings.MudRunnerRootDir = mudRunnerRoot.Attributes.First().Value;
                }


                List<XmlElem> chapters = xmlSettings.GetXmlItems<XmlElem>(elem => elem.Name == SConsts.CHAPTER);
                settings.Chapters.Clear();
                foreach (XmlElem chapter in chapters)
                {
                    string? chapterName = chapter.Attributes.FirstOrDefault(atr => atr.Name == SConsts.NAME)?.Value;
                    if (!string.IsNullOrWhiteSpace(chapterName))
                        settings.Chapters.Add(new DirectoryInfo(chapterName));
                }

				XmlElem? alwaysClearCache = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.ALWAYS_CLEAR_CACHE);
                if(alwaysClearCache is not null)
                {
					string? value = alwaysClearCache.Attributes.FirstOrDefault(atr => atr.Name == SConsts.VALUE)?.Value;
                    if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool val))
                        settings.AlwaysClearCache = val;
				}

				XmlElem? delWithoutWarning = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.DELETE_MOD_WITHOUT_WARNING);
				if (delWithoutWarning is not null)
				{
					string? value = delWithoutWarning.Attributes.FirstOrDefault(atr => atr.Name == SConsts.VALUE)?.Value;
					if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool val))
						settings.DeleteModWithoutWarning = val;
				}

			});
        }

        public async Task SaveAsync(ISettings settings)
        {
            XmlDoc xmlSettings = new(_filePath);

            await xmlSettings.LoadOrCreateAsync();

            await Task.Run(() =>
            {
                xmlSettings.Clear();

                XmlElem settElem = new(SConsts.SETTINGS);
                XmlEndElem settEndElem = new(SConsts.SETTINGS);
                xmlSettings.AddRootXmlElem(settElem, settEndElem);

                XmlElem mudRunnerRoot = new(SConsts.MUDRUNNER_ROOT);
                mudRunnerRoot.Attributes.Add(new XmlElemAttribute(SConsts.PATH, settings.MudRunnerRootDir));
                xmlSettings.AddXmlElem(mudRunnerRoot, SConsts.SETTINGS);

                if (settings.Chapters.Count > 0)
                {
                    XmlElem chaptersElem = new(SConsts.CHAPTERS);
                    XmlEndElem chaptersEndElem = new(SConsts.CHAPTERS);
                    xmlSettings.AddXmlElem(chaptersElem, chaptersEndElem, SConsts.SETTINGS);

                    List<XmlElem> chapters = [];
                    foreach (var chapterName in settings.Chapters)
                    {
                        XmlElem chapterElem = new(SConsts.CHAPTER);
                        chapterElem.Attributes.Add(new XmlElemAttribute(SConsts.NAME, chapterName.FullName));
                        chapters.Add(chapterElem);
                    }

                    xmlSettings.AddRangeXmlElems(chapters, SConsts.CHAPTERS);
                }

                XmlElem alwaysClearCache = new(SConsts.ALWAYS_CLEAR_CACHE);
				alwaysClearCache.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.AlwaysClearCache.ToString()));
				xmlSettings.AddXmlElem(alwaysClearCache, SConsts.SETTINGS);

				XmlElem delWithoutWarning = new(SConsts.DELETE_MOD_WITHOUT_WARNING);
				delWithoutWarning.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.DeleteModWithoutWarning.ToString()));
				xmlSettings.AddXmlElem(delWithoutWarning, SConsts.SETTINGS);
			});

            await xmlSettings.SaveAsync();
        }
    }
}
