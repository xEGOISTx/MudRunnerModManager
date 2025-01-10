//using MudRunnerModManager.Common.XmlWorker;
//using System.Linq;
//using System.Threading.Tasks;
//using SConsts = MudRunnerModManager.Common.SettingsConsts;

//namespace MudRunnerModManager.Common.AppSettings
//{
//    public class XmlSettingsProvider : ISettingsProvider
//    {
//        private readonly string _filePath;

//        public XmlSettingsProvider(string filePath)
//        {
//            _filePath = filePath;
//        }

//        public async Task LoadAsync(ISettings settings)
//        {
//            XmlDoc xmlSettings = new(_filePath);
//            if (!xmlSettings.Exists)
//                return;

//            await xmlSettings.LoadAsync();

//            await Task.Run(() =>
//            {
//				XmlElem? alwaysClearCache = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.ALWAYS_CLEAR_CACHE);
//                if(alwaysClearCache is not null)
//                {
//					string? value = alwaysClearCache.Attributes.FirstOrDefault(atr => atr.Name == SConsts.VALUE)?.Value;
//                    if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool val))
//                        settings.AlwaysClearCache = val;
//				}

//				XmlElem? delWithoutWarning = xmlSettings.GetXmlItem<XmlElem>(elem => elem.Name == SConsts.DELETE_MOD_WITHOUT_WARNING);
//				if (delWithoutWarning is not null)
//				{
//					string? value = delWithoutWarning.Attributes.FirstOrDefault(atr => atr.Name == SConsts.VALUE)?.Value;
//					if (!string.IsNullOrWhiteSpace(value) && bool.TryParse(value, out bool val))
//						settings.DeleteModWithoutWarning = val;
//				}

//			});
//        }

//        public async Task SaveAsync(ISettings settings)
//        {
//            XmlDoc xmlSettings = new(_filePath);

//            await xmlSettings.LoadOrCreateAsync();

//            await Task.Run(() =>
//            {
//                xmlSettings.Clear();

//                XmlElem settElem = new(SConsts.SETTINGS);
//                xmlSettings.AddRootXmlElem(settElem);

//                XmlElem alwaysClearCache = new(SConsts.ALWAYS_CLEAR_CACHE);
//				alwaysClearCache.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.AlwaysClearCache.ToString()));
//				xmlSettings.AddXmlElem(alwaysClearCache, SConsts.SETTINGS);

//				XmlElem delWithoutWarning = new(SConsts.DELETE_MOD_WITHOUT_WARNING);
//				delWithoutWarning.Attributes.Add(new XmlElemAttribute(SConsts.VALUE, settings.DeleteModWithoutWarning.ToString()));
//				xmlSettings.AddXmlElem(delWithoutWarning, SConsts.SETTINGS);
//			});

//            await xmlSettings.SaveAsync();
//        }
//    }
//}
