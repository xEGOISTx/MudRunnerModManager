using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MudRunnerModManager.Models.ArchiveWorker;
using MudRunnerModManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;
using Avalonia.Platform;
using MudRunnerModManager.AdditionalWindows.AddModDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs;
using MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs.MsgDialog;

namespace MudRunnerModManager.Common
{
    public enum MsgDialogButtons
	{
		OK,
		YesNo
	}

	public enum DialogButtonResult
	{
		OK,
		Yes,
		No,
		Cancel
	}

	public class DialogResult
	{
		public DialogButtonResult Result { get; set; }
	}


	public class SelectItemDialogResult<TItem> : DialogResult
	{
		public TItem? SelectedItem { get; set; }
	}


	public class RenameDialogResult : DialogResult
	{
		public string Name { get; init; } = string.Empty;
	}

	public class DialogManager
	{
		private const double BUTTON_WIDTH = 80;

		public static DialogButton[] YesNo => 
		[
			new() { Name = Res.Yes, Width = BUTTON_WIDTH },
			new() { Name = Res.No, Width = BUTTON_WIDTH}
		];

		public static DialogButton[] OKCancel =>
		[
			new() { Name = Res.OK, Width = BUTTON_WIDTH },
			new() { Name = Res.Cancel, Width = BUTTON_WIDTH, IsCancel = true }
		];

		public static DialogButton[] OK =>
		[
			new() { Name = Res.OK, Width = BUTTON_WIDTH }
		];


		public static async Task<TexInputBoxResult> ShowTextInputDialog(
			string defaultText, 
			string description,
			DialogButton[] buttons,
			IEnumerable<TextInputValidationCondition> conditions)
		{
			TextInputBox textInputBox = new(
				defaultText,
				description,
				buttons,
				conditions,
				GetDialogWindowSettings());

			return await textInputBox.ShowDialogAsync(MainWindow.Instsnce);
		}


		public static async Task<RenameDialogResult> ShowRenameFolderDialog(
			string defaultText,
			string description,
			HashSet<string> existFolderNames,
			IEnumerable<TextInputValidationCondition>? validateConditions = null)
		{

			existFolderNames = existFolderNames.Select(n => n.ToLower()).ToHashSet();

			static bool IsValidText(string text)
			{
				var val = text.Trim();
				string pattern = @"[^\w\.\s@-]";
				var res = Regex.IsMatch(val, pattern);
				return !res;
			}

			List<TextInputValidationCondition> conditions =
			[
				new(text => !string.IsNullOrWhiteSpace(text) && IsValidText(text), Res.InvalidFolderName),
				new(text => string.IsNullOrWhiteSpace(text) || !existFolderNames.Contains(text.ToLower().Trim()), Res.NameAlreadyExists),
				new(text => string.IsNullOrWhiteSpace(defaultText) || (text != null && text.ToLower().Trim() != defaultText.ToLower().Trim()), Res.NameMustBeDifferent)
			];

			if (validateConditions != null)
				conditions.AddRange(validateConditions);

			var res = await ShowTextInputDialog(defaultText, description, OKCancel, conditions);

			return new RenameDialogResult { Name = res.Text, Result = res.Result == Res.OK ? DialogButtonResult.OK : DialogButtonResult.Cancel};
		}


		public static async Task<string> OpenFolderDialog()
		{
			var topLevel = TopLevel.GetTopLevel(MainWindow.Instsnce);

			if (topLevel != null)
			{
				var folder = (await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
				{
					Title = Res.SelectFolder,
					AllowMultiple = false
				})).FirstOrDefault();

				if (folder != null)
				{
					return folder.Path.LocalPath;
				}
			}

			return string.Empty;
		}

		public static async Task<string> OpenFileDialog()
		{
			var topLevel = TopLevel.GetTopLevel(MainWindow.Instsnce);

			if (topLevel != null)
			{
				var file = (await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
				{
					Title = Res.SelectFile,
					AllowMultiple = false,
					FileTypeFilter = new List<FilePickerFileType> { new FilePickerFileType(null) { Patterns = ArchiveExtractor.AvailableExts } }
				})).FirstOrDefault();

				if (file != null)
				{
					return file.Path.LocalPath;
				}
			}

			return string.Empty;
		}

		public static async Task<DialogButtonResult> ShowMessageDialog(string message, DialogButton[] buttons, DialogImage dialogImage)
		{
			MsgBox msgBox = new(message, buttons, dialogImage, GetMsgDialogWindowSettings());
			var result = await msgBox.ShowDialogAsync(MainWindow.Instsnce);

			return GetResult(result);
		}

		public static async Task<AddModBoxResult> ShowAddModDialog(string defaultModName, IEnumerable<ChapterItem> chapters)
		{
			static bool IsValidText(string text)
			{
				var val = text.Trim();
				string pattern = @"[^\w\.\s@-]";
				var res = Regex.IsMatch(val, pattern);
				return !res;
			}

			List<TextInputValidationCondition> conditions =
			[
				new(text => !string.IsNullOrWhiteSpace(text) && IsValidText(text), Res.InvalidFolderName),
			];

			AddModBox addModBox = new(defaultModName, chapters, conditions, GetDialogWindowSettings());

			return await addModBox.ShowDialogAsync(MainWindow.Instsnce);
		}

		public static async Task<SelectItemDialogResult<TItem>> ShowSelectItemDialog<TItem>(IEnumerable<TItem> items,
			Func<TItem, string> displayValue,
			string description,
			IEnumerable<SelectItemUserValidationCondition<TItem>>? validateConditions = null)
		{
			SelectItemBox<TItem> selectItemBox = new(items, displayValue, 
				description,
				OKCancel, 
				validateConditions, 
				GetDialogWindowSettings());

			var res = await selectItemBox.ShowDialogAsync(MainWindow.Instsnce);

			return new SelectItemDialogResult<TItem>
			{
				//todo: _
				Result = res.Result == Res.OK ? DialogButtonResult.OK : DialogButtonResult.Cancel,
				SelectedItem = res.SelectedItem,
			};
		}

		private static DialogButtonResult GetResult(string result)
		{
			if (result == Res.OK)
				return DialogButtonResult.OK;
			else if(result ==  Res.Yes)
				return DialogButtonResult.Yes;
			else if(result == Res.No)
				return DialogButtonResult.No;
			else if (result == string.Empty || result == Res.Cancel)
				return DialogButtonResult.Cancel;

			throw new NotImplementedException($"{nameof(DialogManager)}. Not implemented {nameof(DialogButtonResult)} value for result \"{result}\"");
		}

		private static DialogWindowSettings GetDialogWindowSettings() 
		{
			return new DialogWindowSettings()
			{
				SizeToContent = SizeToContent.Height,
				CanResize = false,
				WindowIcon = GetLogo(),
				WindowTitle = Res.AppName,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				MaxWidth = 350
			};
		}

		private static DialogWindowSettings GetMsgDialogWindowSettings()
		{
			return new DialogWindowSettings()
			{
				CanResize = false,
				WindowIcon = GetLogo(),
				WindowTitle = Res.AppName,
				WindowStartupLocation = WindowStartupLocation.CenterOwner,
				MaxWidth = 700
			};
		}


		//пока так
		private static WindowIcon GetLogo()
		{
			return new WindowIcon(AssetLoader.Open(new Uri("avares://MudRunnerModManager/Assets/logo.ico")));
		}
	}
}
