﻿using MudRunnerModManager.Common.XmlWorker;
using System.Linq;
using SConsts = MudRunnerModManager.Common.SettingsConsts;

namespace MudRunnerModManager.Common.AppRepo
{
	public class XmlSettingsRepo : ISettingsRepo
	{
		private readonly string _filePath;

		public XmlSettingsRepo(string filePath) 
		{ 
			_filePath = filePath;
		}

		public Settings Load()
		{
			Settings settings = new();
			XmlDoc xmlSettings = new(_filePath);
			if (!xmlSettings.Exists)
				return settings;

			xmlSettings.Load();

			XmlElem? alwaysClearCache = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.ALWAYS_CLEAR_CACHE);
			if (alwaysClearCache is not null)
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

			return settings;
		}

		public void Save(Settings settings)
		{
			XmlDoc xmlSettings = new(_filePath);

			xmlSettings.LoadOrCreate();

			xmlSettings.Clear();

			XmlElem settElem = new(SConsts.SETTINGS);
			xmlSettings.AddRootXmlElem(settElem);

			XmlElem alwaysClearCache = new(SConsts.ALWAYS_CLEAR_CACHE);
			alwaysClearCache.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.AlwaysClearCache.ToString()));
			xmlSettings.AddXmlElem(alwaysClearCache, SConsts.SETTINGS);

			XmlElem delWithoutWarning = new(SConsts.DELETE_MOD_WITHOUT_WARNING);
			delWithoutWarning.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.DeleteModWithoutWarning.ToString()));
			xmlSettings.AddXmlElem(delWithoutWarning, SConsts.SETTINGS);

			xmlSettings.Save();
		}
	}
}
