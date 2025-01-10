using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.ViewModels
{
	public class SettingsViewModel : BusyViewModel
	{
		private readonly ISettingsRepo _settingsRepo;
		private bool _alwaysClearCache = false;
		private bool _deleteModWithoutWarning = false;
		private Settings _settings;

		public SettingsViewModel(ISettingsRepo settingsRepo) 
		{
			_settingsRepo = settingsRepo;
			_settings = new();

			this.WhenAnyValue(vm => vm.AlwaysClearCache, vm => vm.DeleteModWithoutWarning)
				.Subscribe(async (x) => await Save());
		}

		public bool AlwaysClearCache
		{
			get => _alwaysClearCache;
			set => this.RaiseAndSetIfChanged(ref _alwaysClearCache, value, nameof(AlwaysClearCache));
		}
		public bool DeleteModWithoutWarning
		{
			get => _deleteModWithoutWarning;
			set => this.RaiseAndSetIfChanged(ref _deleteModWithoutWarning, value, nameof(DeleteModWithoutWarning));
		}


		public bool CanSave => _settings != null && NeedSave;

		private bool NeedSave => _alwaysClearCache != _settings.AlwaysClearCache
			|| _deleteModWithoutWarning != _settings.DeleteModWithoutWarning;


		public async void Refresh()
		{
			await BusyAction
			(
				_settingsRepo.Load,
				SetSettings
			);
		}

		public async Task Save()
		{
			if (!CanSave)
				return;

			await BusyAction
			(
				() =>
				{
					ApplySettings();
					_settingsRepo.Save(_settings);
				}, 
				() => 
				{
					this.RaisePropertyChanged(nameof(CanSave));
					EventTube.PushEvent(this, new EventArgs(), EventKey.SettingsChanged);
				}
			);
		}

		private void ApplySettings()
		{
			_settings.AlwaysClearCache = _alwaysClearCache;
			_settings.DeleteModWithoutWarning = _deleteModWithoutWarning;
		}

		private void SetSettings(Settings settings)
		{
			_settings = settings;
			AlwaysClearCache = settings.AlwaysClearCache;
			DeleteModWithoutWarning = settings.DeleteModWithoutWarning;
		}
	}
}
