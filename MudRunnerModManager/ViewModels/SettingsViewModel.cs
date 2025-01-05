using MudRunnerModManager.Common;
using MudRunnerModManager.Models;
using ReactiveUI;
using System;
using System.Reactive;
using System.Threading.Tasks;

namespace MudRunnerModManager.ViewModels
{
	public class SettingsViewModel : BusyViewModel
	{
		private readonly SettingsModel _model;
		private bool _alwaysClearCache = false;
		private bool _deleteModWithoutWarning = false;

		public SettingsViewModel(SettingsModel model) 
		{
			_model = model;

			//IObservable<bool> validateMRRootDir =
			//this.WhenAnyValue(
			//	x => x.MRRootDirectory,
			//	x => x.IsCorrectMRRootDir,
			//	(dir, isCorrect) => string.IsNullOrWhiteSpace(dir) || isCorrect == true);

			//this.ValidationRule(vm => vm.MRRootDirectory, validateMRRootDir, Res.WrongPath);

			//this.WhenAnyValue(vm => vm.MRRootDirectory)
			//	.Subscribe(x => this.RaisePropertyChanged(nameof(IsCorrectMRRootDir)));


			//BrowseMRRootDirCommand = ReactiveCommand.Create(BrowseMRRootDir);

			var canSave = this.WhenAnyValue(
				vm => vm.CanSave, can => can == true);

			SaveCommand = ReactiveCommand.Create(Save, canSave);

			PropertyChanged += SettingsViewModel_PropertyChanged;
		}


		//public ChaptersViewModel ChaptersVM { get; }

		//public string MRRootDirectory
		//{
		//	get => _mRRootDirectory;
		//	set => this.RaiseAndSetIfChanged(ref _mRRootDirectory, value, nameof(MRRootDirectory));
		//}

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


		public bool CanSave => NeedSave;

		private bool NeedSave => _alwaysClearCache != _model.Settings.AlwaysClearCache
			|| _deleteModWithoutWarning != _model.Settings.DeleteModWithoutWarning;


		public ReactiveCommand<Unit, Task> SaveCommand { get; private set; }


		public void Refresh()
		{
			AlwaysClearCache = _model.Settings.AlwaysClearCache;
			DeleteModWithoutWarning = _model.Settings.DeleteModWithoutWarning;
		}

		private void SettingsViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName != nameof(CanSave))
				this.RaisePropertyChanged(nameof(CanSave));
		}

		private void ChaptersVM_ChaptersChanged(object? sender, EventArgs e)
		{
			this.RaisePropertyChanged(nameof(CanSave));
		}

		//private async Task BrowseMRRootDir()
		//{
		//	string mRRootDir = await DialogManager.OpenFolderDialog();
		//	if (!string.IsNullOrWhiteSpace(mRRootDir))
		//	{
		//		MRRootDirectory = mRRootDir;
		//	}
		//}

		public async Task Save()
		{
			await BusyAction(async () =>
			{
				ApplySettings();
				await _model.Save();
				EventTube.PushEvent(this, new EventArgs(), EventKey.SettingsChanged);
			});
		}

		private void ApplySettings()
		{
			_model.Settings.AlwaysClearCache = AlwaysClearCache;
			_model.Settings.DeleteModWithoutWarning = DeleteModWithoutWarning;
		}
	}
}
