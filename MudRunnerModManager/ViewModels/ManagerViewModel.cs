using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Models;
using ReactiveUI;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ViewModels;

public class ManagerViewModel : ViewModelBase
{
	private bool _isSelectedSettings = false;
	private bool _isBusy;
	private int _busyCount = 0;
	private bool _isUpdateAvailable = false;
	private bool _isShowPathInput = true;
	private readonly XmlGamesRootPathsRepo _grpRepo = new(AppPaths.XmlGameRootPathsFilePath);

	//todo: пока будет здесь
	private readonly PathValidationCondition _condition = new PathValidationCondition
	(
		path => !string.IsNullOrEmpty(path)
			&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}")
			&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}"),
		Res.WrongPath
	);
	private readonly string _gameName = GameName.MUDRUNNER;

	public ManagerViewModel() 
	{
		ShowRootPathInputCommand = ReactiveCommand.Create(ShowPathInput);
		Init();
	}


	public ModsViewModel? ModsVM { get; set; }
	public SettingsViewModel? SettingsVM { get; set; }
	public ChaptersViewModel? ChaptersVM { get; set; }
	public GameRootPathViewModel? GameRootPathVM { get; set; }

	//public bool IsSelectedSettings
	//{
	//	get => _isSelectedSettings;
	//	set
	//	{
	//		if(_isSelectedSettings != value)
	//		{
	//			if (_isSelectedSettings && !value)
	//			{
	//				SaveSettingsIfNeed();
	//			}
	//			_isSelectedSettings = value;
	//			this.RaisePropertyChanged(nameof(IsSelectedSettings));
	//		}
	//	}
	//}

	public bool IsBusy
	{
		get => _isBusy;
		set => this.RaiseAndSetIfChanged(ref _isBusy, value, nameof(IsBusy));
	}

	public bool IsUpdateAvailable
	{
		get => _isUpdateAvailable;
		set => this.RaiseAndSetIfChanged(ref _isUpdateAvailable, value, nameof(IsUpdateAvailable));
	}

	public bool IsShowPathInput
	{
		get => _isShowPathInput;
		set => this.RaiseAndSetIfChanged(ref _isShowPathInput, value, nameof(IsShowPathInput));
	}

	public string AppVersion { get; private set; } = string.Empty;

	private async void Init()
	{
		IsBusy = true;
		var appVer = Common.AppVersion.GetVersion();
		AppVersion = $"v{appVer}";

		await Task.Run(() => { new VersionSapport().Execute(); });
		GameRootPath? gameRootPath = await Task.Run(() => _grpRepo.Get(_gameName));
		IsBusy = false;

		if (gameRootPath == null
			|| (gameRootPath != null && !_condition.Condition(gameRootPath.Path)))
		{
			ShowPathInput();		
		}
		else
		{
			ShowManagerView(true);
		}

#if !DEBUG
		var latestAppVer = await GitHubRepo.GetLatestVersionAsync();
		if (latestAppVer != null)
			IsUpdateAvailable = latestAppVer > appVer;
#endif
	}


	public ReactiveCommand<Unit, Unit> OpenGitHubLinkCommand { get; private set; } = ReactiveCommand.Create(GitHubRepo.OpenRepo);
	public ReactiveCommand<Unit, Unit> OpenReleasesCommand { get; private set; } = ReactiveCommand.Create(GitHubRepo.OpenReleases);
	public ReactiveCommand<Unit, Unit> ShowRootPathInputCommand { get; private set; }

	private void BusyVM_BusyChanged(object? sender, bool e)
	{
		if(sender is BusyViewModel)
		{
			_busyCount += e == true ? 1 : -1;
			if (_busyCount < 0)
				_busyCount = 0;

			if(_busyCount > 0)
				IsBusy = true;
			else 
				IsBusy = false;
		}
	}

	//private async void SaveSettingsIfNeed()
	//{
	//	if(SettingsVM != null && SettingsVM.CanSave)
	//	{
	//		var res = await DialogManager.ShowMessageDialog(Res.SettingsHaveBeenChangedSaveIt, DialogManager.YesNo, AdditionalWindows.Dialogs.DialogImage.Question);
	//		if (res == DialogButtonResult.Yes)
	//			await SettingsVM.Save();
	//		if (res == DialogButtonResult.No)
	//			SettingsVM.Refresh();
	//	}
	//}

	private async void ShowManagerView(bool refresh)
	{
		IsShowPathInput = false;

		IsBusy = true;

		GameRootPath? gameRootPath = await Task.Run(() => _grpRepo.Get(_gameName));
		if (gameRootPath == null)
			throw new System.Exception("Root path to game not exist");
			
		//Settings settings = await Settings.GetInstance();

		XmlChapterInfosRepo chaptersRepo = new(AppPaths.XmlChaptersFilePath);
		XmlSettingsRepo settingsRepo = new(AppPaths.XmlSettingsFilePath);

		if (ModsVM == null)
		{
			ModsVM = new ModsViewModel(new ModsModel(chaptersRepo, settingsRepo, gameRootPath.Path), new CacheCleaner(AppPaths.MudRunnerCacheDir));
			ModsVM.BusyChanged += BusyVM_BusyChanged;
			this.RaisePropertyChanged(nameof(ModsVM));
		}

		if(ChaptersVM == null)
		{
			var chaptersModel = new ChaptersModel(chaptersRepo, gameRootPath.Path);
			ChaptersVM = new ChaptersViewModel(chaptersModel);
			ChaptersVM.BusyChanged += BusyVM_BusyChanged;
			this.RaisePropertyChanged(nameof(ChaptersVM));
		}

		if(SettingsVM == null)
		{
			SettingsVM = new SettingsViewModel(settingsRepo);
			SettingsVM.BusyChanged += BusyVM_BusyChanged;
			this.RaisePropertyChanged(nameof(SettingsVM));
		}

		IsBusy = false;

		if(refresh)
		{
			ModsVM.Refresh();
			ChaptersVM.Refresh();
			SettingsVM.Refresh();
		}		
	}

	private void ShowPathInput()
	{
		if(GameRootPathVM == null)
		{
			GameRootPathVM = new GameRootPathViewModel(new GameRootPathModel(_grpRepo, _gameName), [_condition]);
			GameRootPathVM.BusyChanged += BusyVM_BusyChanged;
			GameRootPathVM.PathChanged += GameRootPathVM_PathChanged;
			GameRootPathVM.Canceled += GameRootPathVM_Canceled;
			this.RaisePropertyChanged(nameof(GameRootPathVM));
		}
		GameRootPathVM.Refresh();
		IsShowPathInput = true;
	}

	private void GameRootPathVM_Canceled(object? sender, System.EventArgs e)
	{
		ShowManagerView(false);
	}

	private void GameRootPathVM_PathChanged(object? sender, System.EventArgs e)
	{
		ShowManagerView(true);
	}
}
