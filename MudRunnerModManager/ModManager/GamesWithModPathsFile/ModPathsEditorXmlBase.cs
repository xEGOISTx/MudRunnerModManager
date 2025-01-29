using MudRunnerModManager.Common.XmlWorker;
using MudRunnerModManager.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile
{
	public class ModPathsEditorXmlBase : IModPathsEditor
	{
		private readonly string _filePath;
		private readonly string _relativePathToRootModsDir;
		private readonly string _rootXmlElemName;
		private readonly string _modsRootDirName;

		public ModPathsEditorXmlBase(string filePath, 
			string relativePathToRootModsDir, 
			string modsRootDirName, 
			string rootXmlElemName)
		{
			_filePath = filePath.Trim(' ', '\\');
			_relativePathToRootModsDir = relativePathToRootModsDir.Trim(' ', '\\');
			_rootXmlElemName = rootXmlElemName.Trim();
			_modsRootDirName = modsRootDirName.Trim();
		}

		public void AddModPath(DirectoryInfo modDir)
		{
			XmlDoc xmlModPaths = new(_filePath);
			xmlModPaths.LoadOrCreate();

			if (xmlModPaths.IsEmpty || !xmlModPaths.IsPresentRootElem(new(_rootXmlElemName)))
			{
				XmlElem rootElem = new(_rootXmlElemName);
				xmlModPaths.AddRootXmlElem(rootElem);
			}

			var modPathElem = CreateXmlElem(modDir);

			if (!xmlModPaths.IsPresentElem(modPathElem))
				xmlModPaths.AddXmlElem(modPathElem, _rootXmlElemName);

			xmlModPaths.Save();
		}

		public void ChangeModChapter(DirectoryInfo modDir, DirectoryInfo newChapter)
		{
			XmlDoc xmlModPaths = new(_filePath);
			if (!xmlModPaths.Exists)
				return;

			if (modDir.Parent == null)
				throw new ArgumentException($"Invalid path: {modDir.FullName}");

			xmlModPaths.Load();

			if(xmlModPaths.IsEmpty)
				return;

			var targetPath = $@"{_relativePathToRootModsDir}\{modDir.Parent.Name}\{modDir.Name}";
			if (modDir.Parent.Name == _modsRootDirName)
				targetPath = $@"{_relativePathToRootModsDir}\{modDir.Name}";

			var newVal = $@"{_relativePathToRootModsDir}\{newChapter.Name}\{modDir.Name}";
			if (newChapter.Name == _modsRootDirName)
				newVal = $@"{_relativePathToRootModsDir}\{modDir.Name}";

			var replaced = ReplaceMediaPaths(
				xmlModPaths,
				targetPath,
				target => newVal,
				true);

			if (replaced)
				xmlModPaths.Save();
		}

		public void DeleteChapter(DirectoryInfo chapter)
		{
			XmlDoc xmlModPaths = new(_filePath);
			if (!xmlModPaths.Exists)
				return;

			xmlModPaths.Load();

			if (xmlModPaths.IsEmpty)
				return;

			var chapterXmlElems = xmlModPaths
				.GetXmlItems<XmlElem>(elem => elem.Name == XmlConsts.MEDIA_PATH
				&& elem.Attributes.Count > 0
				&& elem.Attributes.First().Name == XmlConsts.PATH
				&& elem.Attributes.First().Value.Contains($@"{_relativePathToRootModsDir}\{chapter.Name}"));

			if (chapterXmlElems.Count == 0)
				return;

			foreach (var xmlElem in chapterXmlElems)
				xmlModPaths.RemoveXmlElem(xmlElem);

			xmlModPaths.Save();
		}

		public void DeleteModPath(DirectoryInfo modDir)
		{
			XmlDoc xmlModPaths = new(_filePath);
			if (!xmlModPaths.Exists)
				return;

			xmlModPaths.Load();

			var modPathElem = CreateXmlElem(modDir);

			if (xmlModPaths.IsEmpty || !xmlModPaths.IsPresentElem(modPathElem))
				return;

			xmlModPaths.RemoveXmlElem(modPathElem);

			xmlModPaths.Save();
		}

		public void RenameChapter(DirectoryInfo chapter, string newName)
		{
			XmlDoc xmlModPaths = new(_filePath);
			if (!xmlModPaths.Exists)
				return;

			xmlModPaths.Load();

			if (xmlModPaths.IsEmpty)
				return;

			var replaced = ReplaceMediaPaths(
				xmlModPaths,
				$@"{_relativePathToRootModsDir}\{chapter.Name}",
				target => target.Replace(chapter.Name, newName),
				true);

			if (replaced)
				xmlModPaths.Save();
		}

		public void RenameMod(DirectoryInfo modDir, string newModName)
		{
			XmlDoc xmlModPaths = new(_filePath);
			if (!xmlModPaths.Exists)
				return;

			xmlModPaths.Load();

			if (xmlModPaths.IsEmpty)
				return;

			var replaced = ReplaceMediaPaths(xmlModPaths,
				$@"{modDir.Parent?.Name}\{modDir.Name}",
				target => target.Replace(modDir.Name, newModName),
				true);

			if (replaced)
				xmlModPaths.Save();
		}

		private XmlElem CreateXmlElem(DirectoryInfo modDir)
		{
			if (modDir.Parent == null)
				throw new ArgumentException($"Invalid path: {modDir.FullName}");

			string path = $@"{_relativePathToRootModsDir}\{modDir.Parent.Name}\{modDir.Name}";

			if (modDir.Parent.Name == _modsRootDirName)
				path = $@"{_relativePathToRootModsDir}\{modDir.Name}";

			var elem = new XmlElem(XmlConsts.MEDIA_PATH);
			elem.Attributes.Add(new XmlElemAttribute(XmlConsts.PATH, path));
			return elem;
		}

		private bool ReplaceMediaPaths(XmlDoc doc,
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
				.GetXmlItems<XmlElem>(elem => elem.Name == XmlConsts.MEDIA_PATH
				&& elem.Attributes.Count == 1
				&& elem.Attributes.First().Name == XmlConsts.PATH
				&& comparator(elem.Attributes.First().Value, target));

			if (xmlElems.Count == 0)
				return false;

			List<XmlElem> replacedXmlElems = [];

			foreach (var elem in xmlElems)
			{
				string newVal = repFunc(elem.Attributes.First().Value);

				var newElem = new XmlElem(XmlConsts.MEDIA_PATH);
				newElem.Attributes.Add(new XmlElemAttribute(XmlConsts.PATH, newVal));
				replacedXmlElems.Add(newElem);
				doc.RemoveXmlElem(elem);
			}

			doc.AddRangeXmlElems(replacedXmlElems, _rootXmlElemName);

			return true;
		}


		//private void SaveConfig(XmlDoc xmlConfig)
		//{
		//	string curPath = xmlConfig.Path;
		//	if (!AppPaths.AppTempDir.Exists)
		//	{
		//		AppPaths.AppTempDir.Create();
		//	}
		//	xmlConfig.Save(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
		//	xmlConfig.Copy(curPath, true);
		//	xmlConfig.Delete();
		//}
	}
}
