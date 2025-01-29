using MudRunnerModManager.Common;
using MudRunnerModManager.Models;
using MudRunnerModManager.ModManager.GamesWithModPathsFile.CommonModels;
using MudRunnerModManager.ViewModels;
using System.IO;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ModManager.GamesWithModPathsFile.MudRunner
{
	public class MRGameComponents : IGameComponents
	{
		public string GameName => Common.GameName.MUD_RUNNER;

		public PathValidationCondition RootPathValidation { get; } = new
		(
			path => !string.IsNullOrEmpty(path)
				&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}"),
			Res.WrongPath
		);
		//&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}")
		public DirectoryInfo CacheDirectory => AppPaths.MudRunnerCacheDir;

		public string RelativePathToRootModsDir => $@"{AppConsts.MEDIA}\{AppConsts.MR_MODS_ROOT_DIR_NAME}";

		public IChaptersModel GetChaptersModel(IChaptersModel baseModel, string gameRootPath)
		{
			return new ChaptersModelModPathsEditor(baseModel, new MRModPathsEditor(gameRootPath));
		}

		public IModsModel GetModsModel(IModsModel baseModel, string gameRootPath)
		{
			return new ModsModelModPathsEditor(baseModel, new MRModPathsEditor(gameRootPath));
		}
	}
}
