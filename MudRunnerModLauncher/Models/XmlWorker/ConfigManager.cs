using System;
using System.IO;
using System.Threading.Tasks;

namespace MudRunnerModLauncher.Models.XmlWorker
{
	internal class ConfigManager
	{
		public async Task AddModPath(DirectoryInfo modDir, FileInfo config)
		{
			await Execute(modDir, config, Add);
		}

		public async Task DeleteModPath(DirectoryInfo modDir, FileInfo config)
		{
			await Execute(modDir, config, Delete);
		}

		public async Task ReplaceModName(DirectoryInfo modDir, string newModName, FileInfo config)
		{
			await Task.Run(async () =>
			{
				XmlDoc xmlConfig = new(config.FullName);
				if (!xmlConfig.Exists)
					return;

				await xmlConfig.LoadAsync();

				XmlElem oldElem = CreateMediaPathElement(modDir.Name);
				XmlElem newElem = CreateMediaPathElement(newModName);

				if (!xmlConfig.IsPresentElem(oldElem))
					return;

				xmlConfig.RepalceXmlElem(oldElem, newElem, AppConsts.CONFIG);

				await SaveConfig(xmlConfig);
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
				XmlElem addElem = CreateMediaPathElement(modDir.Name);

				if (!action(xmlConfig, addElem))
					return;

				await SaveConfig(xmlConfig);
			});
		}

		private async Task SaveConfig(XmlDoc xmlConfig)
		{
			string curPath = xmlConfig.Path;
			await xmlConfig.SaveAsync(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
			await xmlConfig.CopyAsync(curPath, true);
			xmlConfig.Delete();
		}


		private XmlElem CreateMediaPathElement(string modName)
		{
			var elem = new XmlElem("MediaPath");
			elem.Attributes.Add(new XmlElemAttribute("Path", @$"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modName}"));
			return elem;
		}
	}
}
