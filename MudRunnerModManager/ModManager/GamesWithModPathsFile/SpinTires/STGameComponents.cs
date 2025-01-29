using MudRunnerModManager.Common;
using MudRunnerModManager.Models;
using MudRunnerModManager.ModManager.GamesWithModPathsFile.CommonModels;
using MudRunnerModManager.ViewModels;
using System.IO;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.SpinTires
{
	public class STGameComponents : IGameComponents
	{
		public string GameName => Common.GameName.SPIN_TIRES;

		public PathValidationCondition RootPathValidation { get; } = new
		(
			path => !string.IsNullOrEmpty(path)
				&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.SPIN_TIRES_EXE}"),
			Res.WrongPath
		);

		public DirectoryInfo CacheDirectory => AppPaths.SpinTiresCacheDir;

		public string RelativePathToRootModsDir => $@"{AppConsts.MEDIA}\{AppConsts.ST_MODS_ROOT_DIR_NAME}";

		public IChaptersModel GetChaptersModel(IChaptersModel baseModel, string gameRootPath)
		{
			return new ChaptersModelModPathsEditor(baseModel, new STModPathsEditor(gameRootPath));
		}

		public IModsModel GetModsModel(IModsModel baseModel, string gameRootPath)
		{
			return new ModsModelModPathsEditor(baseModel, new STModPathsEditor(gameRootPath));
		}
	}
}
