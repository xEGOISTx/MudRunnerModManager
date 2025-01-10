namespace MudRunnerModManager.Common.AppRepo
{
	public interface IGamesRootPathsRepo
	{
		GameRootPath? Get(string gameName);

		void Save(GameRootPath gameRootPath);

		bool IsPresent(GameRootPath gameRootPath);
	}

	public class GameRootPath
	{
		public GameRootPath(string path, string gameName)
		{
			Path = path;
			GameName = gameName;
		}

		public string Path { get; }
		public string GameName { get; }
	}
}
