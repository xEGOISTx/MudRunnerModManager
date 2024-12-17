using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace MudRunnerModManager.Common.AppSettings
{
    public class Settings : SettingsBase, ISettings
    {
        private static Settings? _instance;

        private Settings() { }


        public static ISettingsProvider SettingsProvider { get; set; } = new XmlSettingsProvider(AppPaths.XmlSettingsFilePath);

        public new string MudRunnerRootDir
        {
            get => base.MudRunnerRootDir;
            set => base.MudRunnerRootDir = value;
        }

		public new bool AlwaysClearCache
        { 
            get => base.AlwaysClearCache; 
            set => base.AlwaysClearCache = value; 
        }
		public new bool DeleteModWithoutWarning
        { 
            get => base.DeleteModWithoutWarning; 
            set => base.DeleteModWithoutWarning = value; 
        }

		public static async Task<Settings> GetInstance()
		{
			if (_instance == null)
			{
				_instance = new Settings();
				await _instance.LoadAsync();
			}

			return _instance;
		}

		public async Task LoadAsync()
        {
            if (SettingsProvider != null)
                await SettingsProvider.LoadAsync(this);
        }

        public async Task SaveAsync()
        {
            if (SettingsProvider != null)
                await SettingsProvider.SaveAsync(this);
        }

    }
}
