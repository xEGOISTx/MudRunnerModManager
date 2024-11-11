using MudRunnerModManager.Common.AppSettings;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using System.Reactive;
using MudRunnerModManager.Common;
using Res = MudRunnerModManager.Lang.Resource;
using System.IO;

namespace MudRunnerModManager.ViewModels
{
	public class ChaptersViewModel : ViewModelBase
	{
		private readonly ISettings _settings;
		private ChapterViewModel? _selectedChapter;

		public ChaptersViewModel(ISettings settings)
		{ 
			_settings = settings;

			AddCommand = ReactiveCommand.Create(Add);
			var canExec = this.WhenAnyValue(vm => vm.SelectedChapter, sch => sch as ChapterViewModel != null);
			RemoveCommand = ReactiveCommand.Create(Remove, canExec);
			RenameCommand = ReactiveCommand.Create(Rename, canExec);
			OpenChapterFolderCommand = ReactiveCommand.Create(OpenChapterFolder, canExec);

			//Refresh();
		}

		public ObservableCollection<ChapterViewModel> Chapters { get; set; } = [];

		public ChapterViewModel? SelectedChapter
		{
			get => _selectedChapter;
			set => this.RaiseAndSetIfChanged(ref _selectedChapter, value, nameof(SelectedChapter)); 
		}

		public List<ChapterViewModel> DeletedChapters { get; } = [];

		public ReactiveCommand<Unit, Task> AddCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RemoveCommand {  get; private set; }

		public ReactiveCommand<Unit, Task> RenameCommand { get; private set; }

		public ReactiveCommand<Unit, Unit> OpenChapterFolderCommand {  get; private set; }

		public event EventHandler? ChaptersChanged;

		public void Refresh()
		{
			DeletedChapters.Clear();
			Chapters.Clear();
			foreach (var item in _settings.Chapters)
			{
				Chapters.Add(new ChapterViewModel(item.Name, false));
			}

			OnChanged();
		}

		public bool HasChanges()
		{
			return DeletedChapters.Count > 0 || Chapters.FirstOrDefault(ch => ch.IsNew || ch.IsRenamed) != null;
		}

		private async Task Add()
		{
			var res = await ShowRenameDialog("");
			if (res.Result != DialogButtonResult.OK)
				return;

			var delCh = DeletedChapters.FirstOrDefault(ch => ch.Name == res.Name);
			if (delCh != null)
			{
				DeletedChapters.Remove(delCh);
				Chapters.Add(delCh);
			}
			else
			{
				Chapters.Add(new ChapterViewModel(res.Name));
			}

			OnChanged();
		}

		private async Task Remove()
		{
			if (SelectedChapter == null)
				return;

			if(!SelectedChapter.IsNew)
			{
				var message = string.Format(Res.DeleteChapterWithContent, SelectedChapter.Name);
				var res = await DialogManager.ShowMessageDialog(message, DialogManager.YesNo, AdditionalWindows.Dialogs.DialogImage.Question);
				if (res != DialogButtonResult.Yes)
					return;

				DeletedChapters.Add(SelectedChapter);
			}
				
			if (SelectedChapter.IsRenamed)
			{
				SelectedChapter.Name = SelectedChapter.OldName;
				SelectedChapter.OldName = string.Empty;
			}

			Chapters.Remove(SelectedChapter);
			SelectedChapter = null;

			OnChanged();
		}

		private async Task Rename()
		{
			if (SelectedChapter == null)
				return;

			var res = await ShowRenameDialog(SelectedChapter.Name);
			if (res.Result != DialogButtonResult.OK)
				return;

			SelectedChapter.Name = res.Name;

			OnChanged();
		}

		private void OpenChapterFolder()
		{
			if (SelectedChapter != null)
			{
				var dir = @$"{_settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}\{SelectedChapter.Name}";
				if(Directory.Exists(dir))
				{
					System.Diagnostics.Process.Start("explorer.exe", dir);
				}
			}				
		}

		private async Task<RenameDialogResult> ShowRenameDialog(string defaultChapterName)
		{
			return await DialogManager.ShowRenameFolderDialog(
				defaultChapterName,
				Res.EnterChapterName,
				new HashSet<string>(Chapters.Where(ch => ch.Name != defaultChapterName).Select(ch => ch.Name)));
		}

		private void OnChanged()
		{
			ChaptersChanged?.Invoke(this, new EventArgs());
		}

	}

	public class ChapterViewModel : ViewModelBase
	{
		private string _name;

		public ChapterViewModel(string name, bool isNew = true) 
		{
			_name = name;
			IsNew = isNew;
		}

		public string Name
		{
			get => _name;
			set
			{
				if(value != _name)
				{
					if(!IsNew && string.IsNullOrEmpty(OldName))
					{
						OldName = _name;
					}
					_name = value;
					this.RaisePropertyChanged(nameof(Name));
				}
			}
		}

		public string OldName { get; set; } = string.Empty;

		public bool IsNew { get; private set; }

		public bool IsRenamed => !IsNew && !string.IsNullOrEmpty(OldName) && OldName != Name;
	}
}
