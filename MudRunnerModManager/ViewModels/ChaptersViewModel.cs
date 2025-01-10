using MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog;
using MudRunnerModManager.Common;
using MudRunnerModManager.Models;
using ReactiveUI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;

namespace MudRunnerModManager.ViewModels
{
	public class ChaptersViewModel : BusyViewModel
	{
		private readonly ChaptersModel _model;
		private ObservableCollection<ChapterViewModel> _chapters = [];
		private ChapterViewModel? _selectedChapter;

		public ChaptersViewModel(ChaptersModel model)
		{
			_model = model;
			EventTube.EventPushed += EventTube_EventPushed;
			InitCommands();			
		}

		public ObservableCollection<ChapterViewModel> Chapters
		{
			get => _chapters;
			private set => this.RaiseAndSetIfChanged(ref _chapters, value, nameof(Chapters));
		}

		public ChapterViewModel? SelectedChapter
		{
			get => _selectedChapter;
			set => this.RaiseAndSetIfChanged(ref _selectedChapter, value, nameof(SelectedChapter));
		}

		public ReactiveCommand<Unit, Task> AddCommand { get; private set; }

		public ReactiveCommand<Unit, Task> DeleteCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RenameCommand { get; private set; }

		public ReactiveCommand<Unit, Unit> OpenFolderCommand { get; private set; }


		public async void Refresh()
		{
			await BusyAction
			(
				_model.GetChapters,
				chapters => 
				{
					Chapters = new ObservableCollection<ChapterViewModel>(chapters.OrderBy(ch => ch.Name).Select(ch => new ChapterViewModel(ch)));
				}
			);

		}

		[MemberNotNull(nameof(AddCommand), nameof(DeleteCommand),
			nameof(RenameCommand), nameof(OpenFolderCommand))]
		private void InitCommands()
		{
			AddCommand = ReactiveCommand.Create(Add);
			var canExec = this.WhenAnyValue(vm => vm.SelectedChapter, sch => sch as ChapterViewModel != null);
			DeleteCommand = ReactiveCommand.Create(Delete, canExec);
			RenameCommand = ReactiveCommand.Create(Rename, canExec);
			OpenFolderCommand = ReactiveCommand.Create(OpenFolder, canExec);
		}

		private async Task Add()
		{
			var res = await ShowRenameDialog("");
			if (res.Result != DialogButtonResult.OK)
				return;

			await BusyAction
			(
				() => _model.AddChapter(res.Name), 
				newChapter =>
				{
					Chapters.Add(new ChapterViewModel(newChapter));
					PushEvent();
				}
			);
		}

		private async Task Delete()
		{
			if (SelectedChapter == null)
				return;

			var message = string.Format(Res.DeleteChapterWithContent, SelectedChapter.Name);
			var res = await DialogManager.ShowMessageDialog(message, DialogManager.YesNo, AdditionalWindows.Dialogs.DialogImage.Question);
			if (res != DialogButtonResult.Yes)
				return;

			await BusyAction
			(
				() => _model.DeleteChapter(SelectedChapter.Model),
				() =>
				{
					Chapters.Remove(SelectedChapter);
					PushEvent();
				}
			);
		}

		private async Task Rename()
		{
			if (SelectedChapter == null)
				return;

			var res = await ShowRenameDialog(SelectedChapter.Name);
			if (res.Result != DialogButtonResult.OK)
				return;

			await BusyAction
			(
				() => _model.RenameChapter(SelectedChapter.Model, res.Name),
				chapter =>
				{
					SelectedChapter.Refresh(chapter);
					PushEvent();
				}
			);
		}

		private void OpenFolder()
		{
			if (SelectedChapter != null && SelectedChapter.Model.Exists)
				System.Diagnostics.Process.Start("explorer.exe", SelectedChapter.Model.Path);

		}

		private async Task<RenameDialogResult> ShowRenameDialog(string defaultChapterName)
		{
			var rootChapterModNames = await BusyAction(_model.GetRootChapterModNames);
			if (rootChapterModNames.Success)
			{
				TextInputValidationCondition condition = new(chName => chName == null || !rootChapterModNames.Value.Contains(chName), Res.NameIsOccupiedByMod);

				return await DialogManager.ShowRenameFolderDialog(
					defaultChapterName,
					Res.EnterChapterName,
					new HashSet<string>(Chapters.Where(ch => ch.Name != defaultChapterName).Select(ch => ch.Name).ToHashSet()),
					[condition]);
			}
			
			return new RenameDialogResult() { Result = DialogButtonResult.Cancel};
		}

		private void EventTube_EventPushed(object sender, System.EventArgs e, EventKey key)
		{
			if (key == EventKey.ModsChanged)
				Refresh();
		}

		private void PushEvent()
		{
			EventTube.PushEvent(this, new System.EventArgs(), EventKey.ChaptersChanged);
		}
	}


	public class ChapterViewModel : INotifyPropertyChanged
	{
		public ChapterViewModel(Chapter chapter)
		{
			Refresh(chapter);
		}

		public Chapter Model { get; private set; }

		public string Name => Model.IsRoot ? Res.RootChapter : Model.Name;

		public long Size => Model.Size;


		public event PropertyChangedEventHandler? PropertyChanged;

		[MemberNotNull(nameof(Model))]
		public void Refresh(Chapter chapter)
		{
			Model = chapter;
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(Size));
		}

		private void OnPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
	}
}
