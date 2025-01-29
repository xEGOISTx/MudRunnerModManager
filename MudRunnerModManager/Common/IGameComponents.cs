using MudRunnerModManager.Models;
using MudRunnerModManager.ViewModels;
using System.IO;

namespace MudRunnerModManager.Common
{
	public interface IGameComponents
	{
		string GameName { get; }
		PathValidationCondition RootPathValidation {  get; }
		DirectoryInfo CacheDirectory { get; }
		string RelativePathToRootModsDir { get; }

		IModsModel GetModsModel(IModsModel baseModel, string gameRootPath);
		IChaptersModel GetChaptersModel(IChaptersModel baseModel, string gameRootPath);


	}
}
