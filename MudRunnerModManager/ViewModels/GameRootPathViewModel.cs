using MudRunnerModManager.Common;
using MudRunnerModManager.Common.AppRepo;
using MudRunnerModManager.Lang;
using MudRunnerModManager.Models;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reactive;
using System.Threading.Tasks;

namespace MudRunnerModManager.ViewModels
{
	public class PathValidationCondition
	{
		public PathValidationCondition(Func<string, bool> condition, string message)
		{
			Condition = condition;
			Message = message;
		}

		public Func<string, bool> Condition { get; }

		public string Message { get; }
	}

	//todo: может стоит сделать контрол
	public class GameRootPathViewModel : BusyViewModel
	{
		private readonly GameRootPathModel _model;
		private string _path = string.Empty;
		private bool _isCancelAvailable;
		private GameRootPath? _rootPath;

		public GameRootPathViewModel(GameRootPathModel model, IEnumerable<PathValidationCondition> validationConditions)
		{
			_model = model;
			InitCommands();
			foreach (var condition in validationConditions)
			{
				this.ValidationRule(vm => vm.Path, path => path == null || path == string.Empty || condition.Condition(path ?? string.Empty), condition.Message);
			}
		}

		public string SelectRootDirectory => string.Format(Resource.SelecRootDirectoryFor, _model.GameName);

		public string Path
		{
			get => _path;
			set => this.RaiseAndSetIfChanged(ref _path, value, nameof(Path));
		}

		public bool IsCancelAvailable
		{
			get => _isCancelAvailable;
			set => this.RaiseAndSetIfChanged(ref _isCancelAvailable, value, nameof(IsCancelAvailable));
		}

		public ReactiveCommand<Unit, Task> SaveCommand { get; private set; }

		public ReactiveCommand<Unit, Unit> CancelCommand { get; private set; }

		public ReactiveCommand<Unit, Task> BrowseCommand { get; private set; }

		public event EventHandler? PathChanged;

		public event EventHandler? Canceled;

		[MemberNotNull(nameof(SaveCommand), nameof(CancelCommand), nameof(BrowseCommand))]
		public void InitCommands()
		{
			var canSave = this.WhenAnyValue(vm => vm.ValidationContext.IsValid, vm => vm.Path,
				(isValid, path) => isValid == true
								&& !string.IsNullOrWhiteSpace(path)
								&& (_rootPath == null
									|| (_rootPath != null && path.Trim() != _rootPath.Path)));

			SaveCommand = ReactiveCommand.Create(Save, canSave);
			CancelCommand = ReactiveCommand.Create(OnCanceled);
			BrowseCommand = ReactiveCommand.Create(Browse);
		}

		public async void Refresh()
		{
			await BusyAction
			(
				_model.Get,
				SetPath
			);
		}

		private async Task Save()
		{
			await BusyAction
			(
				() => _model.Save(_path.Trim([' ', '\\'])),
				rootPath => 
				{
					SetPath(rootPath);
					OnPathChanged();
				}
			);
		}

		private void SetPath(GameRootPath? rootPath)
		{
			_rootPath = rootPath;
			if (_rootPath != null) 
				Path = _rootPath.Path;

			IsCancelAvailable = _rootPath != null;
		}

		private async Task Browse()
		{
			string path = await DialogManager.OpenFolderDialog();
			if (!string.IsNullOrWhiteSpace(path))
			{
				Path = path;
			}
		}

		private void OnPathChanged()
		{
			PathChanged?.Invoke(this, EventArgs.Empty);
		}

		private void OnCanceled()
		{
			Canceled?.Invoke(this, EventArgs.Empty);
		}
	}
}
