using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MudRunnerModManager.Common.XmlWorker;

namespace MudRunnerModManager.Common
{
    public class ConfigManager
    {
        public async Task AddModPath(DirectoryInfo modDir, FileInfo config)
        {
            await Execute(modDir, config, Add);
        }

        public async Task DeleteModPath(DirectoryInfo modDir, FileInfo config)
        {
            await Execute(modDir, config, Delete);
        }

        public async Task RenameMod(DirectoryInfo modDir, string newModName, FileInfo config)
        {
            await Task.Run(async () =>
            {
                XmlDoc xmlConfig = new(config.FullName);
                if (!xmlConfig.Exists)
                    return;

                await xmlConfig.LoadAsync();

                ReplaceMediaPaths(xmlConfig,
                    $@"{modDir.Parent?.Name}\{modDir.Name}",
                    target => target.Replace(modDir.Name, newModName),
                    true);

                await SaveConfigAsync(xmlConfig);
            });
        }

        public void RenameChapter(DirectoryInfo chapter, string newName, FileInfo config)
        {
			XmlDoc xmlConfig = new(config.FullName);
			if (!xmlConfig.Exists)
				return;

			xmlConfig.Load();

			ReplaceMediaPaths(
				xmlConfig,
				$@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapter.Name}",
				target => target.Replace(chapter.Name, newName),
				true);

			SaveConfig(xmlConfig);
		}

		public async Task RenameChapterAsync(DirectoryInfo chapter, string newName, FileInfo config)
        {
            await Task.Run(() =>
            {
				RenameChapter(chapter, newName, config);
				//XmlDoc xmlConfig = new(config.FullName);
				//if (!xmlConfig.Exists)
				//    return;

				//await xmlConfig.LoadAsync();

				//ReplaceMediaPaths(
				//    xmlConfig, 
				//    $@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapter.Name}",
				//    target => target.Replace(chapter.Name, newName),
				//    true);

				//await SaveConfigAsync(xmlConfig);
			});
        }

		public void ChangeModChapter(DirectoryInfo modDir, DirectoryInfo newChapter, FileInfo config)
		{
			XmlDoc xmlConfig = new(config.FullName);
			if (!xmlConfig.Exists)
				return;

			xmlConfig.Load();

			var targetPath = $@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modDir.Parent.Name}\{modDir.Name}";
			var newVal = $@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{newChapter.Name}\{modDir.Name}";

			if (newChapter.Name == AppConsts.MODS_ROOT_DIR)
			{
				newVal = $@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modDir.Name}";
			}

			if (modDir.Parent.Name == AppConsts.MODS_ROOT_DIR)
			{
				targetPath = $@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modDir.Name}";
			}

			ReplaceMediaPaths(
				xmlConfig,
				targetPath,
				target => newVal,
				true);

			SaveConfig(xmlConfig);
		}

        public void DeleteChapter(DirectoryInfo chapter, FileInfo config)
        {
			XmlDoc xmlConfig = new(config.FullName);
			if (!xmlConfig.Exists)
				return;

			xmlConfig.Load();

			var chapterXmlElems = xmlConfig
				.GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.MEDIA_PATH
				&& elem.Attributes.Count > 0
				&& elem.Attributes.First().Name == AppConsts.PATH
				&& elem.Attributes.First().Value.Contains($@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapter.Name}"));

			foreach (var xmlElem in chapterXmlElems)
			{
				xmlConfig.RemoveXmlElem(xmlElem);
			}

			SaveConfig(xmlConfig);
		}

		public async Task DeleteChapterAsync(DirectoryInfo chapter, FileInfo config)
        {
            await Task.Run(() =>
            {
				DeleteChapter(chapter, config);
				//XmlDoc xmlConfig = new(config.FullName);
				//if (!xmlConfig.Exists)
				//    return;

				//await xmlConfig.LoadAsync();

				//var chapterXmlElems = xmlConfig
				//    .GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.MEDIA_PATH
				//    && elem.Attributes.Count > 0
				//    && elem.Attributes.First().Name == AppConsts.PATH
				//    && elem.Attributes.First().Value.Contains($@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{chapter.Name}"));

				//foreach (var xmlElem in chapterXmlElems)
				//{
				//    xmlConfig.RemoveXmlElem(xmlElem);
				//}


				//await SaveConfigAsync(xmlConfig);
			});
        }

        //todo: временно для синхронизации со старыми версиями
        public async Task ReplaseOldModsRootDirNameToNewName(FileInfo config)
        {
			await Task.Run(async () =>
			{
				XmlDoc xmlConfig = new(config.FullName);
				if (!xmlConfig.Exists)
					return;

				await xmlConfig.LoadAsync();

				ReplaceMediaPaths(
					xmlConfig,
					$@"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR_OLD}",
					target => target.Replace(AppConsts.MODS_ROOT_DIR_OLD, AppConsts.MODS_ROOT_DIR),
					true);

				await SaveConfigAsync(xmlConfig);
			});
		}

        private bool Add(XmlDoc xmlConfig, XmlElem elem)
        {
            if (xmlConfig.IsPresentElem(elem))
                return false;

            xmlConfig.AddXmlElem(elem, AppConsts.CONFIG);
            return true;
        }

        private bool Delete(XmlDoc xmlConfig, XmlElem elem)
        {
            if (!xmlConfig.IsPresentElem(elem))
                return false;

            xmlConfig.RemoveXmlElem(elem);
            return true;
        }

        private async Task Execute(DirectoryInfo modDir, FileInfo config, Func<XmlDoc, XmlElem, bool> action)
        {
            await Task.Run(async () =>
            {
                XmlDoc xmlConfig = new(config.FullName);
                if (!xmlConfig.Exists)
                    return;

                await xmlConfig.LoadAsync();

                string path = AppConsts.MEDIA;

				if (modDir.Parent.Name != AppConsts.MODS_ROOT_DIR)
                {
                    path += $@"\{AppConsts.MODS_ROOT_DIR}\{modDir.Parent.Name}\{modDir.Name}";
                }
                else
                {
					path += $@"\{modDir.Parent.Name}\{modDir.Name}";
				}

				var elem = new XmlElem(AppConsts.MEDIA_PATH);
				elem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, path));

				if (!action(xmlConfig, elem))
                    return;

                await SaveConfigAsync(xmlConfig);
            });
        }

        private async Task SaveConfigAsync(XmlDoc xmlConfig)
        {
            string curPath = xmlConfig.Path;
            if(!AppPaths.AppTempDir.Exists)
            {
				AppPaths.AppTempDir.Create();
			}
            await xmlConfig.SaveAsync(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
            await xmlConfig.CopyAsync(curPath, true);
            xmlConfig.Delete();
        }

        private void SaveConfig(XmlDoc xmlConfig)
        {
			string curPath = xmlConfig.Path;
			if (!AppPaths.AppTempDir.Exists)
			{
				AppPaths.AppTempDir.Create();
			}
			xmlConfig.Save(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
			xmlConfig.Copy(curPath, true);
			xmlConfig.Delete();
		}


		//private XmlElem CreateMediaPathElement(string modName)
		//{
		//    var elem = new XmlElem(AppConsts.MEDIA_PATH);
		//    elem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, @$"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modName}"));
		//    return elem;
		//}


		private void ReplaceMediaPaths(
            XmlDoc doc, 
            string target, 
            Func<string, string> repFunc, 
            bool contains)
        {

            Func<string, string, bool> comparator = (value, target) => value == target;
            if (contains)
            {
				comparator = (value, target) => value.Contains(target);
			}

			var xmlElems = doc
                .GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.MEDIA_PATH
                && elem.Attributes.Count == 1
                && elem.Attributes.First().Name == AppConsts.PATH
                && comparator(elem.Attributes.First().Value, target));

            if (xmlElems.Count < 1)
                return;

			List<XmlElem> replacedXmlElems = [];

			foreach ( var elem in xmlElems )
            {
                string newVal = repFunc(elem.Attributes.First().Value);

				var newElem = new XmlElem(AppConsts.MEDIA_PATH);
				newElem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, newVal));
				replacedXmlElems.Add(newElem);
				doc.RemoveXmlElem(elem);
			}

            doc.AddRangeXmlElems(replacedXmlElems, AppConsts.CONFIG);
		}
    }
}
