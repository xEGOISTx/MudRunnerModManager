﻿//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using MudRunnerModManager.Common.XmlWorker;

//namespace MudRunnerModManager.Common
//{
//    public class ConfigManager
//    {
//		public void AddModPath(DirectoryInfo modDir, FileInfo config)
//		{
//			Execute(modDir, config, Add);
//		}

//		public void DeleteModPath(DirectoryInfo modDir, FileInfo config)
//		{
//			Execute(modDir, config, Delete);
//		}

//		//public async Task AddModPathAsync(DirectoryInfo modDir, FileInfo config)
//  //      {
//		//	await Task.Run(() => AddModPath(modDir, config));
//  //      }

//  //      public async Task DeleteModPathAsync(DirectoryInfo modDir, FileInfo config)
//  //      {
//		//	await Task.Run(() => DeleteModPath(modDir, config));
//  //      }

//		public void RenameMod(DirectoryInfo modDir, string newModName, FileInfo config)
//		{
//			XmlDoc xmlConfig = new(config.FullName);
//			if (!xmlConfig.Exists)
//				return;

//			xmlConfig.Load();

//			ReplaceMediaPaths(xmlConfig,
//				$@"{modDir.Parent?.Name}\{modDir.Name}",
//				target => target.Replace(modDir.Name, newModName),
//				true);

//			SaveConfig(xmlConfig);
//		}


//		//public async Task RenameModAsync(DirectoryInfo modDir, string newModName, FileInfo config)
//  //      {
//  //          await Task.Run(() => RenameMod(modDir, newModName, config));
//  //      }

//        public void RenameChapter(DirectoryInfo chapter, string newName, FileInfo config)
//        {
//			XmlDoc xmlConfig = new(config.FullName);
//			if (!xmlConfig.Exists)
//				return;

//			xmlConfig.Load();

//			ReplaceMediaPaths(
//				xmlConfig,
//				$@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{chapter.Name}",
//				target => target.Replace(chapter.Name, newName),
//				true);

//			SaveConfig(xmlConfig);
//		}

//		//public async Task RenameChapterAsync(DirectoryInfo chapter, string newName, FileInfo config)
//  //      {
//  //          await Task.Run(() =>
//  //          {
//		//		RenameChapter(chapter, newName, config);
//		//	});
//  //      }

//		public void ChangeModChapter(DirectoryInfo modDir, DirectoryInfo newChapter, FileInfo config)
//		{
//			XmlDoc xmlConfig = new(config.FullName);
//			if (!xmlConfig.Exists)
//				return;

//			xmlConfig.Load();

//			var targetPath = $@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{modDir.Parent.Name}\{modDir.Name}";
//			var newVal = $@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{newChapter.Name}\{modDir.Name}";

//			if (newChapter.Name == AppConsts.MR_MODS_ROOT_DIR_NAME)
//			{
//				newVal = $@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{modDir.Name}";
//			}

//			if (modDir.Parent.Name == AppConsts.MR_MODS_ROOT_DIR_NAME)
//			{
//				targetPath = $@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{modDir.Name}";
//			}

//			ReplaceMediaPaths(
//				xmlConfig,
//				targetPath,
//				target => newVal,
//				true);

//			SaveConfig(xmlConfig);
//		}

//		public void DeleteChapter(DirectoryInfo chapter, FileInfo config)
//		{
//			XmlDoc xmlConfig = new(config.FullName);
//			if (!xmlConfig.Exists)
//				return;

//			xmlConfig.Load();

//			var chapterXmlElems = xmlConfig
//				.GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.MEDIA_PATH
//				&& elem.Attributes.Count > 0
//				&& elem.Attributes.First().Name == AppConsts.PATH
//				&& elem.Attributes.First().Value.Contains($@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{chapter.Name}"));

//			if (chapterXmlElems.Count == 0)
//				return;

//			foreach (var xmlElem in chapterXmlElems)
//			{
//				xmlConfig.RemoveXmlElem(xmlElem);
//			}

//			SaveConfig(xmlConfig);
//		}

//		//public async Task DeleteChapterAsync(DirectoryInfo chapter, FileInfo config)
//  //      {
//  //          await Task.Run(() =>
//  //          {
//		//		DeleteChapter(chapter, config);
//		//	});
//  //      }

//        private bool Add(XmlDoc xmlConfig, XmlElem elem)
//        {
//            if (xmlConfig.IsPresentElem(elem))
//                return false;

//            xmlConfig.AddXmlElem(elem, AppConsts.CONFIG);
//            return true;
//        }

//        private bool Delete(XmlDoc xmlConfig, XmlElem elem)
//        {
//            if (!xmlConfig.IsPresentElem(elem))
//                return false;

//            xmlConfig.RemoveXmlElem(elem);
//            return true;
//        }

//		private void Execute(DirectoryInfo modDir, FileInfo config, Func<XmlDoc, XmlElem, bool> action)
//		{
//			XmlDoc xmlConfig = new(config.FullName);
//			if (!xmlConfig.Exists)
//				return;

//			xmlConfig.Load();

//			string path = AppConsts.MEDIA;

//			if (modDir.Parent.Name != AppConsts.MR_MODS_ROOT_DIR_NAME)
//			{
//				path += $@"\{AppConsts.MR_MODS_ROOT_DIR_NAME}\{modDir.Parent.Name}\{modDir.Name}";
//			}
//			else
//			{
//				path += $@"\{modDir.Parent.Name}\{modDir.Name}";
//			}

//			var elem = new XmlElem(AppConsts.MEDIA_PATH);
//			elem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, path));

//			if (!action(xmlConfig, elem))
//				return;

//			SaveConfig(xmlConfig);
//		}

//        private void SaveConfig(XmlDoc xmlConfig)
//        {
//			string curPath = xmlConfig.Path;
//			if (!AppPaths.AppTempDir.Exists)
//			{
//				AppPaths.AppTempDir.Create();
//			}
//			xmlConfig.Save(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
//			xmlConfig.Copy(curPath, true);
//			xmlConfig.Delete();
//		}

//		private void ReplaceMediaPaths(
//            XmlDoc doc, 
//            string target, 
//            Func<string, string> repFunc, 
//            bool contains)
//        {

//            Func<string, string, bool> comparator = (value, target) => value == target;
//            if (contains)
//            {
//				comparator = (value, target) => value.Contains(target);
//			}

//			var xmlElems = doc
//                .GetXmlItems<XmlElem>(elem => elem.Name == AppConsts.MEDIA_PATH
//                && elem.Attributes.Count == 1
//                && elem.Attributes.First().Name == AppConsts.PATH
//                && comparator(elem.Attributes.First().Value, target));

//            if (xmlElems.Count < 1)
//                return;

//			List<XmlElem> replacedXmlElems = [];

//			foreach ( var elem in xmlElems )
//            {
//                string newVal = repFunc(elem.Attributes.First().Value);

//				var newElem = new XmlElem(AppConsts.MEDIA_PATH);
//				newElem.Attributes.Add(new XmlElemAttribute(AppConsts.PATH, newVal));
//				replacedXmlElems.Add(newElem);
//				doc.RemoveXmlElem(elem);
//			}

//            doc.AddRangeXmlElems(replacedXmlElems, AppConsts.CONFIG);
//		}
//    }
//}
