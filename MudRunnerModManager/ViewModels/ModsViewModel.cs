using MudRunnerModManager.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;
using MudRunnerModManager.Common;
using MudRunnerModManager.AdditionalWindows.AddModDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Collections.ObjectModel;

namespace MudRunnerModManager.ViewModels
{
	public class ModsViewModel : BusyViewModel
	{
		private readonly ModsModel _model;
		private readonly CacheCleaner _cacheCleaner;
		private ObservableCollection<ModViewModel> _addedMods = [];
		private ModViewModel? _selectedMod;
		private List<ChapterBaseViewModel> _chapters = [];

		public ModsViewModel(ModsModel model, CacheCleaner cacheCleaner)
		{
			_model = model;
			_cacheCleaner = cacheCleaner;
			EventTube.EventPushed += EventTube_EventPushed;

			var canExec = this.WhenAnyValue(vm => vm.SelectedMod, vm => vm.IsCorrectMRRootDir, (sm, isCorrect) => sm as ModViewModel != null && isCorrect == true);
			var canExecRelocate = this.WhenAnyValue(vm => vm.SelectedMod, vm => vm.IsCorrectMRRootDir, (sm, isCorrect) => sm as ModViewModel != null && _chapters.Count > 0 && isCorrect == true);//&& () == true
			AddModCommand = ReactiveCommand.Create(AddMod, this.WhenAnyValue(vm => vm.IsCorrectMRRootDir, isCorrect => isCorrect == true));
			DeleteModCommand = ReactiveCommand.Create(DeleteSelectedMod, canExec);
			RenameModCommand = ReactiveCommand.Create(RenameMod, canExec);
			RelocateModCommand = ReactiveCommand.Create(RelocateMod, canExecRelocate);
			OpenModFolderCommand = ReactiveCommand.Create(OpenModFolder, canExec);

			RefreshAsync();
		}

		public bool IsCorrectMRRootDir => _model.IsCorrectMRRootDir;

		public ObservableCollection<ModViewModel> AddedMods
		{
			get => _addedMods;
			set => this.RaiseAndSetIfChanged(ref _addedMods, value, nameof(AddedMods));
		}

		public ModViewModel? SelectedMod
		{
			get => _selectedMod;
			set => this.RaiseAndSetIfChanged(ref _selectedMod, value, nameof(SelectedMod));
		}

		public ReactiveCommand<Unit, Task> AddModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> DeleteModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RenameModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RelocateModCommand { get; private set; }

		public ReactiveCommand<Unit, Unit> OpenModFolderCommand { get; private set; }

		private async Task AddMod()
		{
			var modPath = await DialogManager.OpenFileDialog();
			if (string.IsNullOrEmpty(modPath))
				return;

			var mod = new FileInfo(modPath);

			List<ChapterItem> chapters = await CreateChapterItems();
			var dialogRes = await DialogManager.ShowAddModDialog(Path.GetFileNameWithoutExtension(mod.Name), chapters);
			if (!dialogRes.OK || dialogRes.ModName == null || dialogRes.Chapter == null)
				return;

			ChapterBaseViewModel selectedChap = _chapters.First(ch => ch.Name == dialogRes.Chapter);

			var res = await BusyAction(() => _model.AddMod(mod, dialogRes.ModName, selectedChap.Model));
			if(res.Success)
			{
				AddedMods.Add(new ModViewModel(res.Value));
				PushEvent();
				await ClearCacheIfNeed();
				await DialogManager.ShowMessageDialog(string.Format(Res.ModAdded, dialogRes.ModName), DialogManager.OK, DialogImage.Success);
			}		
		}

		private async Task DeleteSelectedMod()
		{
			if (SelectedMod == null)
				return;

			if (!_model.Settings.DeleteModWithoutWarning)
			{
				string message = string.Format(Res.DeleteMod, SelectedMod.Name);
				var res = await DialogManager.ShowMessageDialog(message, DialogManager.YesNo, DialogImage.Question);
				if (res != DialogButtonResult.Yes)
					return;
			}

			await BusyAction
			(
				() => _model.DeleteMod(SelectedMod.Model),
				() =>
				{
					AddedMods.Remove(SelectedMod);
					SelectedMod = null;
					PushEvent();
				}
			);
		}

		private async Task RenameMod()
		{
			if (SelectedMod == null)
				return;

			var renameRes = await ShowRenameDialog(SelectedMod);
			if (renameRes.Result != DialogButtonResult.OK)
				return;

			await BusyAction
			(
				() => _model.RenameMod(SelectedMod.Model, renameRes.Name),
				SelectedMod.Refresh
			);
		}

		private async Task RelocateMod()
		{
			if (SelectedMod == null)
				return;

			var result = await BusyAction(() => GetRelocModConditions(SelectedMod));
			if(result.Success)
			{
				var res = await DialogManager.ShowSelectItemDialog(_chapters.Where(ch => ch.Name != SelectedMod.ChapterName),
					chapter => chapter.Name,
					Res.SelectChapter,
					result.Value);

				if (res.Result != DialogButtonResult.OK)
					return;

				ChapterBase? selectedChapter = res.SelectedItem?.Model;

				//не должно такого случиться, но на всякий
				if (selectedChapter == null)
				{
					await DialogManager.ShowMessageDialog("Chapter not selected", DialogManager.OK, AdditionalWindows.Dialogs.DialogImage.Error);
					return;
				}

				await BusyAction
				(
					() => _model.RelocateMod(SelectedMod.Model, selectedChapter), 
					mod =>
					{
						SelectedMod.Refresh(mod);
						PushEvent();
					}
				);			
			}
		}

		private List<SelectItemUserValidationCondition<ChapterBaseViewModel>> GetRelocModConditions(ModViewModel relocMod)
		{
			var chaptersWithModsDic = GetChaptersWithMods(ch => ch.Name != relocMod.ChapterName);
			List<SelectItemUserValidationCondition<ChapterBaseViewModel>> conditions = [];
			var conditionNameAlreadyExists = new SelectItemUserValidationCondition<ChapterBaseViewModel>(
				selChap => selChap != null && !chaptersWithModsDic[selChap]
				.Any(mod => mod.Name.ToLower() == relocMod.Name.ToLower()),
				Res.NameAlreadyExists);
			conditions.Add(conditionNameAlreadyExists);

			if (!relocMod.Model.Chapter.IsRoot)
			{
				var chapterLowerNames = new HashSet<string>(chaptersWithModsDic.Keys.Select(ch => ch.Name.ToLower()));

				var conditionNameIsOccupiedByChapter = new SelectItemUserValidationCondition<ChapterBaseViewModel>(
					selChap => selChap != null && !selChap.Model.IsRoot || !chapterLowerNames.Contains(relocMod.Name.ToLower()),
					Res.NameIsOccupiedByChapter);
				conditions.Add(conditionNameIsOccupiedByChapter);
			}

			return conditions;
		}

		private void OpenModFolder()
		{
			if (SelectedMod != null && Directory.Exists(SelectedMod.Model.DirInfo.FullName))
				System.Diagnostics.Process.Start("explorer.exe", SelectedMod.Model.DirInfo.FullName);
		}

		private async Task<RenameDialogResult> ShowRenameDialog(ModViewModel mod)
		{
			List<TextInputValidationCondition>? validateConditions = null;

			if (mod.Model.Chapter.IsRoot)
			{
				var chapterNames = _chapters.Select(ch => ch.Name.ToLower()).ToHashSet();
				var conditionForRoorDir = new TextInputValidationCondition(
					 modName => modName == null || !chapterNames.Contains(modName.ToLower().Trim()), Res.NameIsOccupiedByChapter);

				validateConditions = [conditionForRoorDir];
			}

			return await DialogManager.ShowRenameFolderDialog(
				mod.Name,
				Res.EnterModName,
				new HashSet<string>(AddedMods.Where(m => m.Name != mod.Name && m.ChapterName == mod.ChapterName).Select(m => m.Name)),
				validateConditions);
		}

		private async void RefreshAsync()
		{
			await BusyAction
			(
				() =>
				{
					var chapters = _model.GetChapters();
					_chapters = new(chapters.Select(ch => new ChapterBaseViewModel(ch)));
					return _model.GetMods(chapters);
				},
				mods => AddedMods = new ObservableCollection<ModViewModel>(mods
				.Select(mod => new ModViewModel(mod))
				.OrderBy(mod => mod.Name)
				.ThenBy(mod => mod.ChapterName))
			);
		}

		private void EventTube_EventPushed(object sender, EventArgs e, EventKey key)
		{
			if (key == EventKey.SettingsChanged)
			{
				this.RaisePropertyChanged(nameof(IsCorrectMRRootDir));
				RefreshAsync();
			}
			if(key == EventKey.ChaptersChanged)
			{
				RefreshAsync();
			}
		}

		private Dictionary<ChapterBaseViewModel, List<ModViewModel>>GetChaptersWithMods(
			Predicate<ChapterBaseViewModel>? chaptersFilter = null,
			Predicate<ModViewModel>? modsFilter = null)
		{
			Dictionary<string, List<ModViewModel>> modsByChapters = [];

			var filteredMods = modsFilter != null ? _addedMods.Where(mod => modsFilter(mod)) : _addedMods;

			foreach (var mod in filteredMods)
			{
				if(modsFilter != null && !modsFilter(mod))
					continue;

				if (modsByChapters.TryGetValue(mod.Model.Chapter.Path, out List<ModViewModel>? mods))
					mods.Add(mod);
				else
					modsByChapters.Add(mod.Model.Chapter.Path, [mod]);
			}

			Dictionary<ChapterBaseViewModel, List<ModViewModel>> chaptersWithMods = [];
			var filteredChapters = chaptersFilter != null ? _chapters.Where(ch => chaptersFilter(ch)) : _chapters;
			foreach (var chapter in filteredChapters)
			{
				if (modsByChapters.TryGetValue(chapter.Model.Path, out List<ModViewModel>? mods))
					chaptersWithMods.Add(chapter, mods);
				else
					chaptersWithMods.Add(chapter, []);
			}

			return chaptersWithMods;
		}

		private async Task<List<ChapterItem>> CreateChapterItems()
		{
			return await Task.Run(() =>
			{
				var chaptersWithMods = GetChaptersWithMods();
				List<ChapterItem> chapters = [];
				foreach (var chapter in chaptersWithMods)
				{
					chapters.Add(new ChapterItem
					{
						Name = chapter.Key.Name,
						Mods = chapter.Value.Select(mod => mod.Name).ToHashSet()
					});
				}

				return chapters.OrderBy(ch => ch.Name).ToList();
			});
		}

		private async Task ClearCacheIfNeed()
		{
			var res = await BusyAction(_cacheCleaner.IsPresentCache);
			if (res.Success && res.Value == true)
			{
				bool clearCache = false;
				if (!_model.Settings.AlwaysClearCache)
				{
					string message = string.Format(Res.DeleteCacheFrom, AppPaths.MudRunnerCacheDir);

					var dialogRes = await DialogManager.ShowMessageDialog(message, DialogManager.YesNo, DialogImage.Question);
					if (dialogRes == DialogButtonResult.Yes)
						clearCache = true;
				}
				else
				{
					clearCache = true;
				}

				if (clearCache)
					await BusyAction(_cacheCleaner.ClearCache);
			}
		}

		private void PushEvent()
		{
			EventTube.PushEvent(this, new System.EventArgs(), EventKey.ModsChanged);
		}
	}

	public class ChapterBaseViewModel
	{
		public ChapterBaseViewModel(ChapterBase chapter)
		{
			Model = chapter;
		}

		public ChapterBase Model { get; }

		public string Name => !Model.IsRoot ? Model.Name : Res.RootChapter;

		public override bool Equals(object? obj)
		{
			return obj is ChapterBaseViewModel chapter && chapter.GetHashCode() == GetHashCode();
		}

		public override int GetHashCode()
		{
			return Model.Path.GetHashCode();
		}
	}


	public class ModViewModel : INotifyPropertyChanged
	{
		public ModViewModel(Mod mod)
		{
			Refresh(mod);
		}

		public Mod Model { get; private set; }

		public string ChapterName => !Model.Chapter.IsRoot ? Model.Chapter.Name : Res.RootChapter;

		public string Name => Model.Name;

		public long Size => Model.Size;


		public event PropertyChangedEventHandler? PropertyChanged;

		[MemberNotNull(nameof(Model))]
		public void Refresh(Mod mod)
		{
			Model = mod;
			OnPropertyChanged(nameof(ChapterName));
			OnPropertyChanged(nameof(Name));
			OnPropertyChanged(nameof(Size));
		}

		private void OnPropertyChanged(string propName)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propName));
		}
	}
}
