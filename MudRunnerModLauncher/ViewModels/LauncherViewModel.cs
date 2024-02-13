﻿using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MudRunnerModLauncher.Models;
using MudRunnerModLauncher.Views;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace MudRunnerModLauncher.ViewModels;

public class LauncherViewModel : ViewModelBase
{
	private readonly Launcher _model = new();
	private List<DirectoryInfo> _addedMods = [];
	private DirectoryInfo? _selectedMod;
	private bool _isBusy;

	public LauncherViewModel() 
	{
		IObservable<bool> validateMRRootDir =
		this.WhenAnyValue(
			x => x.MRRootDirectory,
			x => x.IsCorrectMRRootDir,
			(dir, isCorrect) => string.IsNullOrWhiteSpace(dir) || isCorrect == true);

		this.ValidationRule(vm => vm.MRRootDirectory, validateMRRootDir, "Не верный путь!");


		BrowseMRRootDirCommand = ReactiveCommand.Create(BrowseMRRootDir);

		AddModCommand = ReactiveCommand.Create(AddMod, this.WhenAnyValue(vm => vm.IsCorrectMRRootDir, isCorrect => isCorrect == true));
		DeleteModCommand = ReactiveCommand.Create(DeleteSelectedMod, this.WhenAnyValue(vm => vm.SelectedMod, sm => sm as DirectoryInfo != null));
		OpenGitHubLinkCommand = ReactiveCommand.Create(OpenGitHubLink);

		this.WhenAnyValue(vm => vm.MRRootDirectory).Subscribe(x => this.RaisePropertyChanged(nameof(IsCorrectMRRootDir)));
		
		RefreshAddedMods();
	}


	public string MRRootDirectory
	{
		get => _model.MudRunnerRoorDir;
		set
		{
			if(value != _model.MudRunnerRoorDir)
			{
				_model.SaveMudRunnerRoorDir(value);
				this.RaisePropertyChanged(nameof(MRRootDirectory));
			}
		}
	}

	public bool IsCorrectMRRootDir => _model.IsCorrectMRRootDir;

	public List<DirectoryInfo> AddedMods
	{
		get => _addedMods;
		set => this.RaiseAndSetIfChanged(ref _addedMods, value, nameof(AddedMods));
	}

	public DirectoryInfo? SelectedMod
	{
		get => _selectedMod;
		set => this.RaiseAndSetIfChanged(ref _selectedMod, value, nameof(SelectedMod));
	}

	public bool IsBusy
	{
		get => _isBusy;
		set => this.RaiseAndSetIfChanged(ref _isBusy, value, nameof(IsBusy));
	}

	public ReactiveCommand<Unit, Task> BrowseMRRootDirCommand { get; private set; }

	public ReactiveCommand<Unit, Task> AddModCommand { get; private set; }

	public ReactiveCommand<Unit, Unit> DeleteModCommand { get; private set; }
	 
	public ReactiveCommand<Unit, Unit> OpenGitHubLinkCommand { get; private set; }


	private async Task BrowseMRRootDir()
	{
		string mRRootDir = await OpenFolderDialog();
		if (!string.IsNullOrWhiteSpace(mRRootDir))
		{
			MRRootDirectory = mRRootDir;
		}
	}

	private async Task AddMod()
	{
		var modPath = await OpenFileDialog();
		if (string.IsNullOrEmpty(modPath))
			return;

		await BusyAction(async () =>
		{		
			await _model.AddModAsync(new FileInfo(modPath));
			RefreshAddedMods();
		});
	}

	private async void DeleteSelectedMod()
	{
		await BusyAction(async () =>
		{
			if (SelectedMod != null)
			{
				await _model.DeleteModAsync(SelectedMod);
			}
			SelectedMod = null;
			RefreshAddedMods();
		});
	}

	private async Task<string> OpenFolderDialog()
	{
		var topLevel = TopLevel.GetTopLevel(MainWindow.Instsnce);

		if(topLevel != null)
		{
			var folder = (await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
			{
				Title = "Выбор папки",
				AllowMultiple = false
			})).FirstOrDefault();

			if(folder != null)
			{
				return folder.Path.LocalPath;
			}
		}

		return string.Empty;
	}

	private async Task<string> OpenFileDialog()
	{
		var topLevel = TopLevel.GetTopLevel(MainWindow.Instsnce);

		if (topLevel != null)
		{
			var file = (await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
			{
				Title = "Выбор файла",
				AllowMultiple = false,
				FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType(null) {Patterns = _model.AvailableExts } }
			})).FirstOrDefault();

			if (file != null)
			{
				return file.Path.LocalPath;
			}
		}

		return string.Empty;
	}

	//https://github.com/dotnet/runtime/issues/17938
	private void OpenGitHubLink()
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			Process.Start(new ProcessStartInfo("cmd", $"/c start https://github.com/xEGOISTx/MudRunnerModLauncher"));
		}
	}

	private async void RefreshAddedMods()
	{
		string selectedModName = SelectedMod != null ? SelectedMod.Name : string.Empty;

		AddedMods = await _model.GetAddedModsAsync();

		if (string.IsNullOrEmpty(selectedModName))
			return;

		DirectoryInfo? selectedMod = AddedMods.FirstOrDefault(dir => dir.Name == selectedModName);
		if (selectedMod != null)
		{
			SelectedMod = selectedMod;
		}
	}

	private async Task BusyAction(Func<Task> action)
	{
		IsBusy = true;
		await action();
		IsBusy = false;
	}
}

