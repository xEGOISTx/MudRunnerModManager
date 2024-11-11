using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppSettings;
using ReactiveUI;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ViewModels;

public class ManagerViewModel : ViewModelBase
{
	private bool _isSelectedSettings = false;
	private bool _isBusy;
	private int _busyCount = 0;
	private bool _isUpdateAvailable = false;

	public ManagerViewModel() 
	{
		Init();
	}


	public ModsViewModel ModsVM { get; set; }
	public SettingsViewModel SettingsVM { get; set; }

	public bool IsSelectedSettings
	{
		get => _isSelectedSettings;
		set
		{
			if(_isSelectedSettings != value)
			{
				if (_isSelectedSettings && !value)
				{
					SaveSettingsIfNeed();
				}
				_isSelectedSettings = value;
				this.RaisePropertyChanged(nameof(IsSelectedSettings));
			}
		}
	}

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

	public string AppVersion { get; private set; } = string.Empty;

	[MemberNotNull(nameof(ModsVM), nameof(SettingsVM))]
	private async void Init()
	{
		var appVer = Common.AppVersion.GetVersion();
		AppVersion = $"v{appVer}";

#pragma warning disable CS8774 // Member must have a non-null value when exiting.
		var settings = await Settings.GetInstance();
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

		ModsVM = new ModsViewModel(new Models.ModsModel(settings));
		ModsVM.BusyChanged += BusyVM_BusyChanged;
		SettingsVM = new SettingsViewModel(new Models.SettingsModel(settings));
		SettingsVM.BusyChanged += BusyVM_BusyChanged;

		this.RaisePropertyChanged(nameof(ModsVM));
		this.RaisePropertyChanged(nameof(SettingsVM));

		var latestAppVer = await GitHubRepo.GetLatestVersionAsync();
		if (latestAppVer != null)
			IsUpdateAvailable = latestAppVer > appVer;
	}


	public ReactiveCommand<Unit, Unit> OpenGitHubLinkCommand { get; private set; } = ReactiveCommand.Create(GitHubRepo.OpenRepo);
	public ReactiveCommand<Unit, Unit> OpenReleasesCommand { get; private set; } = ReactiveCommand.Create(GitHubRepo.OpenReleases);

	private void BusyVM_BusyChanged(object? sender, bool e)
	{
		if(sender is BusyViewModel busyVM)
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

	private async void SaveSettingsIfNeed()
	{
		if(SettingsVM != null && SettingsVM.CanSave)
		{
			var res = await DialogManager.ShowMessageDialog(Res.SettingsHaveBeenChangedSaveIt, DialogManager.YesNo, AdditionalWindows.Dialogs.DialogImage.Question);
			if (res == DialogButtonResult.Yes)
				await SettingsVM.Save();
			if (res == DialogButtonResult.No)
				SettingsVM.Refresh();
		}
	}

}
