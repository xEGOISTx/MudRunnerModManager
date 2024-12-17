using MudRunnerModManager.Common;
using MudRunnerModManager.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ViewModels
{
	public class SettingsViewModel : BusyViewModel
	{
		private readonly SettingsModel _model;
		private string _mRRootDirectory = string.Empty;
		private bool _alwaysClearCache = false;
		private bool _deleteModWithoutWarning = false;

		public SettingsViewModel(SettingsModel model) 
		{
			_model = model;
			//ChaptersVM = new ChaptersViewModel(_model.Settings);
			Refresh();
			//ChaptersVM.ChaptersChanged += ChaptersVM_ChaptersChanged;

			IObservable<bool> validateMRRootDir =
			this.WhenAnyValue(
				x => x.MRRootDirectory,
				x => x.IsCorrectMRRootDir,
				(dir, isCorrect) => string.IsNullOrWhiteSpace(dir) || isCorrect == true);

			this.ValidationRule(vm => vm.MRRootDirectory, validateMRRootDir, Res.WrongPath);

			this.WhenAnyValue(vm => vm.MRRootDirectory)
				.Subscribe(x => this.RaisePropertyChanged(nameof(IsCorrectMRRootDir)));


			BrowseMRRootDirCommand = ReactiveCommand.Create(BrowseMRRootDir);

			var canSave = this.WhenAnyValue(
				vm => vm.CanSave, can => can == true);

			SaveCommand = ReactiveCommand.Create(Save, canSave);

			PropertyChanged += SettingsViewModel_PropertyChanged;
		}


		//public ChaptersViewModel ChaptersVM { get; }

		public string MRRootDirectory
		{
			get => _mRRootDirectory;
			set => this.RaiseAndSetIfChanged(ref _mRRootDirectory, value, nameof(MRRootDirectory));
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

		public bool IsCorrectMRRootDir => _model.IsCorrectMRRootDir(_mRRootDirectory);

		public bool CanSave => IsCorrectMRRootDir && NeedSave;

		private bool NeedSave => _mRRootDirectory != _model.Settings.MudRunnerRootDir 
			|| _alwaysClearCache != _model.Settings.AlwaysClearCache
			|| _deleteModWithoutWarning != _model.Settings.DeleteModWithoutWarning;


		public ReactiveCommand<Unit, Task> BrowseMRRootDirCommand { get; private set; }
		public ReactiveCommand<Unit, Task> SaveCommand { get; private set; }


		public void Refresh()
		{
			MRRootDirectory = _model.Settings.MudRunnerRootDir;
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

		private async Task BrowseMRRootDir()
		{
			string mRRootDir = await DialogManager.OpenFolderDialog();
			if (!string.IsNullOrWhiteSpace(mRRootDir))
			{
				MRRootDirectory = mRRootDir;
			}
		}

		public async Task Save()
		{
			await BusyAction(async () =>
			{
				await ApplySettings();

				await _model.SynhronizeWithOldVersion();
				await _model.Save();
				EventTube.PushEvent(this, new EventArgs(), EventKey.SettingsChanged);
			});
		}

		private async Task ApplySettings()
		{
			await Task.Run( async () =>
			{
				_model.Settings.MudRunnerRootDir = MRRootDirectory;
				_model.Settings.AlwaysClearCache = AlwaysClearCache;
				_model.Settings.DeleteModWithoutWarning = DeleteModWithoutWarning;
				//List<DirectoryInfo> chapters = [];

				//var modsRootDir = $@"{MRRootDirectory}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}";
				//foreach (var chapter in ChaptersVM.Chapters)
				//{
				//	DirectoryInfo chapterInfo;
				//	if (chapter.IsRenamed)
				//	{
				//		chapterInfo = new DirectoryInfo($@"{modsRootDir}\{chapter.OldName}");
				//		await _model.RenameChapter(chapterInfo, chapter.Name);
				//	}

				//	chapterInfo = new DirectoryInfo($@"{modsRootDir}\{chapter.Name}");
				//	chapters.Add(chapterInfo);
				//}

				//foreach (var delChapter in ChaptersVM.DeletedChapters)
				//{
				//	await _model.DeleteChapter(new DirectoryInfo($@"{modsRootDir}\{delChapter.Name}"));
				//}

				//_model.Settings.Chapters = chapters;
			});
		}
	}
}
