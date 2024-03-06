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
				XmlElem addElem = CreateMediaPathElement(modDir);

				if (!action(xmlConfig, addElem))
					return;

				await xmlConfig.SaveAsync(@$"{AppPaths.AppTempDir}\{AppConsts.CONFIG_XML}");
				await xmlConfig.CopyAsync(config.FullName, true);
				xmlConfig.Delete();
			});
		}


		private XmlElem CreateMediaPathElement(DirectoryInfo modDir)
		{
			var elem = new XmlElem("MediaPath");
			elem.Attributes.Add(new XmlElemAttribute("Path", @$"{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{modDir.Name}"));
			return elem;
		}
	}
}
