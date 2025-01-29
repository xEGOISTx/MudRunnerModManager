using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Models;
using MudRunnerModManager.ModManager.GamesWithModPathsFile.CommonModels;
using MudRunnerModManager.ModManager.GamesWithModPathsFile.MudRunner;
using MudRunnerModManager.ModManager.GamesWithModPathsFile.SpinTires;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
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
	private GameInfo? _selectedGameInfo;

	[MemberNotNullWhen(true, nameof(SelectedGameInfo))]
	private bool _isSelectedGame => SelectedGameInfo != null;
	private bool _gameChanged = false;

	//todo: пока будет здесь
	//private readonly PathValidationCondition _condition = new
	//(
	//	path => !string.IsNullOrEmpty(path)
	//		&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.MUD_RUNNER_EXE}")
	//		&& File.Exists(@$"{path.Trim([' ', '\\'])}\{AppConsts.CONFIG_XML}"),
	//	Res.WrongPath
	//);
	//private readonly string _gameName = GameName.MUD_RUNNER;

	public ManagerViewModel() 
	{
		ShowRootPathInputCommand = ReactiveCommand.Create(ShowPathInput);
		Init();
	}


	public ModsViewModel? ModsVM { get; set; }
	public SettingsViewModel? SettingsVM { get; set; }
	public ChaptersViewModel? ChaptersVM { get; set; }
	public GameRootPathViewModel? GameRootPathVM { get; set; }
	public List<GameInfo> GameInfos { get; set; } = [];
	public GameInfo? SelectedGameInfo
	{
		get => _selectedGameInfo;
		set
		{
			if ( _selectedGameInfo != value )
			{
				_selectedGameInfo = value;
				_gameChanged = true;
				InitGameModManager();
				this.RaisePropertyChanged(nameof(SelectedGameInfo));
			}
		}
	}

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

		GameInfos =
		[
			new (GameName.MUD_RUNNER, new Bitmap(AssetLoader.Open(new Uri("avares://MudRunnerModManager/Assets/MudRunner.jpg"))) , new MRGameComponents()),
			new (GameName.SPIN_TIRES, new Bitmap(AssetLoader.Open(new Uri("avares://MudRunnerModManager/Assets/Spintires.png"))) , new STGameComponents())
		];
		this.RaisePropertyChanged(nameof(GameInfos));

		SelectedGameInfo = GameInfos.First();

		IsBusy = false;

		//if (gameRootPath == null
		//	|| (gameRootPath != null && !_condition.Condition(gameRootPath.Path)))
		//{
		//	ShowPathInput();		
		//}
		//else
		//{
		//	ShowManagerView(true);
		//}

#if !DEBUG
		var latestAppVer = await GitHubRepo.GetLatestVersionAsync();
		if (latestAppVer != null)
			IsUpdateAvailable = latestAppVer > appVer;
#endif
	}

	private async void InitGameModManager()
	{
		if (!_isSelectedGame)
			return;

		GameRootPath? gameRootPath = await Task.Run(() => _grpRepo.Get(SelectedGameInfo.GameName));
		if(gameRootPath != null && SelectedGameInfo.GameComponents.RootPathValidation.Condition(gameRootPath.Path))//_condition.Condition(gameRootPath.Path)
			ShowManagerView(false);
		else
			ShowPathInput();
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

	private async void ShowManagerView(bool gameRootPathChanged)
	{
		if (!_isSelectedGame)
			return;

		IsShowPathInput = false;

		if(_gameChanged || gameRootPathChanged)
		{
			IsBusy = true;

			GameRootPath? gameRootPath = await Task.Run(() => _grpRepo.Get(SelectedGameInfo.GameName));
			if (gameRootPath == null)
				throw new Exception("Root path to game not exist");
			
			XmlChapterInfosRepo chaptersRepo = new(AppPaths.XmlChaptersFilePath);
			XmlSettingsRepo settingsRepo = new(AppPaths.XmlSettingsFilePath);

			if(ModsVM != null)
				ModsVM.BusyChanged -= BusyVM_BusyChanged;

			IModsModel modsModel = new ModsModel(chaptersRepo, settingsRepo, gameRootPath.Path, SelectedGameInfo.GameComponents.RelativePathToRootModsDir);
			modsModel = SelectedGameInfo.GameComponents.GetModsModel(modsModel, gameRootPath.Path);

			ModsVM = new ModsViewModel(modsModel, new CacheCleaner(SelectedGameInfo.GameComponents.CacheDirectory));
			ModsVM.BusyChanged += BusyVM_BusyChanged;
			this.RaisePropertyChanged(nameof(ModsVM));

			if(ChaptersVM != null)
				ChaptersVM.BusyChanged -= BusyVM_BusyChanged;

			IChaptersModel chaptersModel = new ChaptersModel(chaptersRepo, gameRootPath.Path, SelectedGameInfo.GameComponents.RelativePathToRootModsDir);
			chaptersModel = SelectedGameInfo.GameComponents.GetChaptersModel(chaptersModel, gameRootPath.Path);

			ChaptersVM = new ChaptersViewModel(chaptersModel);
			ChaptersVM.BusyChanged += BusyVM_BusyChanged;
			this.RaisePropertyChanged(nameof(ChaptersVM));

			if(SettingsVM == null)
			{
				SettingsVM = new SettingsViewModel(settingsRepo);
				SettingsVM.BusyChanged += BusyVM_BusyChanged;
				this.RaisePropertyChanged(nameof(SettingsVM));
			}

			IsBusy = false;
		}

		if(_gameChanged || gameRootPathChanged)
		{
			ModsVM?.Refresh();
			ChaptersVM?.Refresh();
			SettingsVM?.Refresh();
		}

		_gameChanged = false;
	}

	private void ShowPathInput()
	{
		if (!_isSelectedGame)
			return;

		void CreateGameRootPathVM()
		{
			GameRootPathVM = new GameRootPathViewModel(new GameRootPathModel(_grpRepo, SelectedGameInfo.GameName), [SelectedGameInfo.GameComponents.RootPathValidation]);
			GameRootPathVM.BusyChanged += BusyVM_BusyChanged;
			GameRootPathVM.PathChanged += GameRootPathVM_PathChanged;
			GameRootPathVM.Canceled += GameRootPathVM_Canceled;
			this.RaisePropertyChanged(nameof(GameRootPathVM));
		}

		if (GameRootPathVM == null)
		{
			CreateGameRootPathVM();
		}
		else if(GameRootPathVM.GameName != SelectedGameInfo.GameName)
		{
			GameRootPathVM.BusyChanged -= BusyVM_BusyChanged;
			GameRootPathVM.PathChanged -= GameRootPathVM_PathChanged;
			GameRootPathVM.Canceled -= GameRootPathVM_Canceled;

			CreateGameRootPathVM();
		}

		GameRootPathVM?.Refresh();
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

public class GameInfo(string gameName, IImage image, IGameComponents gameComponents)
{
	public string GameName { get; } = gameName;

	public IGameComponents GameComponents { get; } = gameComponents;

	public IImage Image { get; } = image;
}

