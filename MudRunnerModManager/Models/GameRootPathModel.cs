using MudRunnerModManager.Common.AppRepo;

namespace MudRunnerModManager.Models
{
	//пока тут
	public static class GameName
	{
		public const string MUDRUNNER = "MudRunner";
	}

	public class GameRootPathModel
	{
		private readonly IGamesRootPathsRepo _gamesRootPathsRepo;

		public GameRootPathModel(IGamesRootPathsRepo gamesRootPathsRepo, string gameName)
		{
			_gamesRootPathsRepo = gamesRootPathsRepo;
			GameName = gameName.ToString();
		}

		public string GameName { get; }

		public GameRootPath? Get()
		{
			return _gamesRootPathsRepo.Get(GameName);
		}

		public GameRootPath Save(string path)
		{
			var grp = new GameRootPath(path, GameName);

			if (!_gamesRootPathsRepo.IsPresent(grp))
				_gamesRootPathsRepo.Save(grp);

			return grp;
		}
	}
}
