using Avalonia.Controls;
using Avalonia.Platform.Storage;
using MsBox.Avalonia.Dto;
using MsBox.Avalonia.Enums;
using MsBox.Avalonia.Models;
using MsBox.Avalonia;
using MudRunnerModManager.AdditionalWindows.TextInputDialog;
using MudRunnerModManager.Models.ArchiveWorker;
using MudRunnerModManager.Views;
using ReactiveUI;
using Splat.ModeDetection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Res = MudRunnerModManager.Lang.Resource;
using Avalonia.Platform;
using MudRunnerModManager.AdditionalWindows.AddModDialog;
using MudRunnerModManager.AdditionalWindows.SelectItemDialog;

namespace MudRunnerModManager.Common
{
	public enum MsgDialogButtons
	{
		OK,
		YesNo
	}

	public enum MsgDialogResult
	{
		OK,
		Yes,
		No
	}

	public class DialogManager
	{
		public static async Task<TexInputBoxResult> ShowTextInputDialog(
			string defaultText, 
			string description,
			IEnumerable<TextInputValidateCondition> conditions)
		{
			TextInputBox textInputBox = new(
				defaultText,
				description,
				conditions);

			return await textInputBox.ShowDialog(MainWindow.Instsnce);
		}


		public static async Task<TexInputBoxResult> ShowRenameFolderDialog(
			string defaultText,
			string description,
			HashSet<string> existFolderNames,
			IEnumerable<TextInputValidateCondition>? validateConditions = null)
		{

			existFolderNames = existFolderNames.Select(n => n.ToLower()).ToHashSet();

			static bool IsValidText(string text)
			{
				var val = text.Trim();
				string pattern = @"[^\w\.\s@-]";
				var res = Regex.IsMatch(val, pattern);
				return !res;
			}

			List<TextInputValidateCondition> conditions =
			[
				new(vm => vm.WhenAnyValue(x => x.Text, text => !string.IsNullOrWhiteSpace(text) && IsValidText(text)), Res.InvalidFolderName),
				new(vm => vm.WhenAnyValue(x => x.Text, text => !existFolderNames.Contains(text.ToLower())), Res.NameAlreadyExists)
			];

			if (validateConditions != null)
				conditions.AddRange(validateConditions);

			return await ShowTextInputDialog(defaultText, description, conditions);
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

		public static async Task<MsgDialogResult> ShowMessageDialog(string message, Icon icon, MsgDialogButtons buttons)
		{
			var msbCache = MessageBoxManager.GetMessageBoxCustom(
				new MessageBoxCustomParams
				{
					ButtonDefinitions = GetButtons(buttons),
					WindowIcon = GetLogo(),
					ContentTitle = Res.AppName,
					ContentMessage = message,
					Icon = icon,
					WindowStartupLocation = WindowStartupLocation.CenterOwner,
					CanResize = false,
					MinWidth = 350,
					MaxWidth = 500,
					MaxHeight = 800,
					SizeToContent = SizeToContent.WidthAndHeight,
					ShowInCenter = true,
					Topmost = false,
				});

			var result = await msbCache.ShowWindowDialogAsync(MainWindow.Instsnce);

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

			List<TextInputValidateCondition> conditions =
			[
				new(vm => vm.WhenAnyValue(x => x.Text, text => !string.IsNullOrWhiteSpace(text) && IsValidText(text)), Res.InvalidFolderName),
			];

			AddModBox addModBox = new(defaultModName, chapters, conditions);

			return await addModBox.ShowDialog(MainWindow.Instsnce);
		}

		public static async Task<SelectItemBoxResult> ShowSelectItemDialog(IEnumerable<string> items,
			string description,
			IEnumerable<SelectItemValidateCondition>? validateConditions = null)
		{
			SelectItemBox selectItemBox = new SelectItemBox(items, description, validateConditions);

			return await selectItemBox.ShowDialog(MainWindow.Instsnce);
		}

		private static List<ButtonDefinition> GetButtons(MsgDialogButtons buttons)
		{
			return buttons switch
			{
				MsgDialogButtons.OK => [new ButtonDefinition { Name = "OK", }],
				MsgDialogButtons.YesNo => 
				[
					new ButtonDefinition { Name = Res.Yes, },
					new ButtonDefinition { Name = Res.No, },
				],
				_ => [],
			};
		}

		private static MsgDialogResult GetResult(string result)
		{
			if (result == "OK")
				return MsgDialogResult.OK;
			else if(result ==  Res.Yes)
				return MsgDialogResult.Yes;
			else if(result == Res.No)
				return MsgDialogResult.No;

			throw new NotImplementedException();
		}

		//пока так
		private static WindowIcon GetLogo()
		{
			return new WindowIcon(AssetLoader.Open(new Uri("avares://MudRunnerModManager/Assets/logo.ico")));
		}
	}
}
