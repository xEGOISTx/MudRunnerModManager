namespace MudRunnerModManager.Common.AppRepo
{
	public interface ISettingsRepo
	{
		Settings Load();
		void Save(Settings settings);
	}

	public class Settings
	{
		public bool AlwaysClearCache { get; set; } = false;
		public bool DeleteModWithoutWarning { get; set; } = false;
	}

}
