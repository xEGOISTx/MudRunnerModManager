namespace MudRunnerModManager.Common.AppRepo
{
	public interface IGamesRootPathsRepo
	{
		GameRootPath? Get(string gameName);

		void Save(GameRootPath gameRootPath);

		bool IsPresent(GameRootPath gameRootPath);
	}
}
