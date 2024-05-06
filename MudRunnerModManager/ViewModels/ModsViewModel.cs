﻿using MudRunnerModManager.Models;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading.Tasks;
using MsBox.Avalonia.Enums;
using Res = MudRunnerModManager.Lang.Resource;
using MudRunnerModManager.AdditionalWindows.TextInputDialog;
using MudRunnerModManager.Common;
using MudRunnerModManager.AdditionalWindows.AddModDialog;
using MudRunnerModManager.AdditionalWindows.SelectItemDialog;

namespace MudRunnerModManager.ViewModels
{
	public class ModsViewModel : BusyViewModel
	{
		private readonly ModsModel _model;
		private List<Mod> _addedMods = [];
		private Mod? _selectedMod;		

		public ModsViewModel(ModsModel model)
		{
			_model = model;
			EventTube.EventPushed += EventTube_EventPushed;

			//IObservable<bool> validateMRRootDir =
			//this.WhenAnyValue(
			//	x => x.MRRootDirectory,
			//	x => x.IsCorrectMRRootDir,
			//	(dir, isCorrect) => string.IsNullOrWhiteSpace(dir) || isCorrect == true);

			//this.ValidationRule(vm => vm.MRRootDirectory, validateMRRootDir, Res.WrongPath);
			

			var canExec = this.WhenAnyValue(vm => vm.SelectedMod, sm => sm as Mod != null);
			var canExecRelocate = this.WhenAnyValue(vm => vm.SelectedMod, sm => sm as Mod != null && _model.Settings.Chapters.Count > 0);//&& () == true
			AddModCommand = ReactiveCommand.Create(AddMod, this.WhenAnyValue(vm => vm.IsCorrectMRRootDir, isCorrect => isCorrect == true));
			DeleteModCommand = ReactiveCommand.Create(DeleteSelectedMod, canExec);
			RenameModCommand = ReactiveCommand.Create(RenameMod, canExec);
			RelocateModCommand = ReactiveCommand.Create(RelocateMod, canExecRelocate);

			//this.WhenAnyValue(vm => vm.MRRootDirectory).Subscribe(x => this.RaisePropertyChanged(nameof(IsCorrectMRRootDir)));

			Task.Run(async () => await BusyAction(async () => await RefreshAsync()));
		}

		public bool IsCorrectMRRootDir => _model.IsCorrectMRRootDir;

		public List<Mod> AddedMods
		{
			get => _addedMods;
			set => this.RaiseAndSetIfChanged(ref _addedMods, value, nameof(AddedMods));
		}

		public Mod? SelectedMod
		{
			get => _selectedMod;
			set => this.RaiseAndSetIfChanged(ref _selectedMod, value, nameof(SelectedMod));
		}

		public ReactiveCommand<Unit, Task> AddModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> DeleteModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RenameModCommand { get; private set; }

		public ReactiveCommand<Unit, Task> RelocateModCommand {  get; private set; }


		private async Task AddMod()
		{
			var modPath = await DialogManager.OpenFileDialog();
			if (string.IsNullOrEmpty(modPath))
				return;

			var mod = new FileInfo(modPath);

			List<ChapterItem> chapters = await CreateChapterItems();
			var res = await DialogManager.ShowAddModDialog(Path.GetFileNameWithoutExtension(mod.Name), chapters);
			if (!res.OK || res.ModName == null || res.Chapter == null)
				return;

			await BusyAction(async () =>
			{
				await _model.AddModAsync(mod, res.Chapter, res.ModName);

				if (await _model.IsPresentCache())
				{
					if(!_model.Settings.AlwaysClearCache)
					{
						string message = string.Format(Res.DeleteCacheFrom, AppPaths.MudRunnerCacheDir);

						var res = await DialogManager.ShowMessageDialog(message, Icon.Question, MsgDialogButtons.YesNo);
						if (res == MsgDialogResult.Yes)
						{
							await _model.ClearCache();
						}
					}
					else
					{
						await _model.ClearCache();
					}
				}

				await RefreshAsync();

				await DialogManager.ShowMessageDialog(string.Format(Res.ModAdded, res.ModName), Icon.Success, MsgDialogButtons.OK);
			});
		}

		private async Task DeleteSelectedMod()
		{
			if (SelectedMod == null)
				return;

			if(!_model.Settings.DeleteModWithoutWarning)
			{
				string message = string.Format(Res.DeleteMod, SelectedMod.Name);
				var res = await DialogManager.ShowMessageDialog(message, Icon.Question, MsgDialogButtons.YesNo);
				if (res != MsgDialogResult.Yes)
					return;
			}

			await BusyAction(async () =>
			{
				await _model.DeleteModAsync(SelectedMod);
				SelectedMod = null;
				await RefreshAsync();
			});
		}

		private async Task RenameMod()
		{
			if (SelectedMod == null)
				return;

			var renameRes = await ShowRenameDialog(SelectedMod);
			if (!renameRes.OK)
				return;

			await BusyAction(async () =>
			{
				await _model.RenameModAsync(SelectedMod, renameRes.Text);
				await RefreshAsync();
			});
		}

		private async Task RelocateMod()
		{
			if (SelectedMod == null)
				return;

			var chaptersDic = (await CreateChapterItems())
				.Where(ch => ch.Name != SelectedMod.Chapter).ToDictionary(ch => ch.Name);

			var chapterLowerNames = new HashSet<string>(chaptersDic.Keys.Select(chName => chName.ToLower()));

			var conditionNameAlreadyExists = new SelectItemValidateCondition(
				vm => vm.WhenAnyValue(vm => vm.SelectedItem, si => si != null && !chaptersDic[si].Mods.Contains(SelectedMod.Name)), 
				Res.NameAlreadyExists);

			var conditionNameIsOccupiedByChapter = new SelectItemValidateCondition(
				vm => vm.WhenAnyValue(vm => vm.SelectedItem, si => si != Res.RootChapter || !chapterLowerNames.Contains(SelectedMod.Name.ToLower())),
				Res.NameIsOccupiedByChapter);

			var res = await DialogManager.ShowSelectItemDialog(chaptersDic.Keys,
				Res.SelectChapter,
				[conditionNameAlreadyExists, conditionNameIsOccupiedByChapter]);

			if (!res.OK)
				return;

			DirectoryInfo selectedChapter = res.SelectedItem == Res.RootChapter 
				? new(@$"{_model.Settings.MudRunnerRootDir}\{AppConsts.MEDIA}\{AppConsts.MODS_ROOT_DIR}") 
				: _model.Settings.Chapters.First(ch => ch.Name == res.SelectedItem);

			await BusyAction(async () =>
			{
				await _model.RelocateModAsync(SelectedMod, selectedChapter);
				await RefreshAsync();
			});
		}

		private async Task<TexInputBoxResult> ShowRenameDialog(Mod mod)
		{
			List<TextInputValidateCondition>? validateConditions = null;

			if(mod.Chapter == Res.RootChapter)
			{
				var chapterNames = _model.Settings.Chapters.Select(ch => ch.Name.ToLower()).ToHashSet();
				var conditionForRoorDir = new TextInputValidateCondition(
					vm => vm.WhenAnyValue(vm => vm.Text, modName => !chapterNames.Contains(modName.ToLower())), Res.NameIsOccupiedByChapter);

				validateConditions = [conditionForRoorDir];
			}

			return await DialogManager.ShowRenameFolderDialog(
				mod.Name, 
				Res.EnterModName, 
				new HashSet<string>(AddedMods.Where(m => m.Name != mod.Name && m.Chapter == mod.Chapter).Select(m => m.Name)),
				validateConditions);
		}

		private void Refresh()
		{
			Mod? selectedMod = SelectedMod;

			AddedMods = _model.GetAddedMods()
				.OrderBy(mod => mod.Name)
				.ThenBy(mod => mod.Chapter).ToList();

			if (selectedMod == null)
				return;

			SelectedMod = AddedMods.FirstOrDefault(dir => dir.DirInfo.FullName.TrimEnd('\\') == selectedMod.DirInfo.FullName.TrimEnd('\\'));
		}

		private async Task RefreshAsync()
		{
			await Task.Run(Refresh);
		}

		private void EventTube_EventPushed(object sender, EventArgs e, EventKey key)
		{
			if (key == EventKey.SettingsChanged)
			{
				this.RaisePropertyChanged(nameof(IsCorrectMRRootDir));
				Task.Run(async () => await BusyAction(async () => await RefreshAsync()));
			}
		}

		private async Task<List<ChapterItem>> CreateChapterItems()
		{
			return await Task.Run(() =>
			{
				List<ChapterItem> chapters = [];
				foreach (var chapterGroup in AddedMods.GroupBy(m => m.Chapter))
				{
					chapters.Add
						(
							new ChapterItem
							{
								Name = chapterGroup.Key,
								Mods = chapterGroup.Select(g => g.Name).ToHashSet(),
							}
						);
				}

				foreach (var chapter in _model.Settings.Chapters)
				{
					if (chapter.GetDirectories().Length == 0)
						chapters.Add
							(
								new ChapterItem
								{
									Name = chapter.Name,
								}
							);
				}

				if(!chapters.Any(ch => ch.Name == Res.RootChapter))
				{
					chapters.Add
					(
						new ChapterItem
						{
							Name = Res.RootChapter,
						}
					);
				}

				return chapters.OrderBy(ch => ch.Name).ToList();
			});
		}
	}
}
