using Avalonia.Controls;
using MudRunnerModManager.AdditionalWindows.SelectItemDialog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MudRunnerModManager.AdditionalWindows.TextInputDialog;
using Res = MudRunnerModManager.Lang.Resource;
using ReactiveUI;

namespace MudRunnerModManager.AdditionalWindows.AddModDialog
{
	public class AddModBox
	{
		private readonly IEnumerable<ChapterItem> _chapters;
		private readonly string _defaultModName;
		private readonly IEnumerable<TextInputValidateCondition> _validateConditions;

		public AddModBox(string defaultModName,IEnumerable<ChapterItem> chapters, IEnumerable<TextInputValidateCondition> nameValidateConditions)
		{
			_defaultModName = defaultModName;
			_validateConditions = nameValidateConditions;
			_chapters = chapters;
			foreach (var chapter in chapters)
			{
				chapter.Mods = chapter.Mods.Select(m => m.ToLower()).ToHashSet();
			}
		}

		public async Task<AddModBoxResult> ShowDialog(Window owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			AddModWindow view = new();
			var chapterNames = _chapters.Select(ch => ch.Name).ToList();
			var chaptersDic = _chapters.ToDictionary(ch => ch.Name, ch => ch);
			var chapterLowerNames = new HashSet<string>(chaptersDic.Keys.Select(chName => chName.ToLower()));
			SelectItemViewModel selectItemVM = new()
			{
				Description = Res.SelectChapter,
				Items = new List<string>(chapterNames),
				SelectedItem = chapterNames.FirstOrDefault()
			};

			TextInputViewModel textInputVM = new()
			{
				Description = Res.EnterModName,
				Text = _defaultModName,
			};

			AddModViewModel addModVM = new(textInputVM, selectItemVM);

			IObservable<bool> validate =
					addModVM.WhenAnyValue(
						x => x.TextInputVM.Text,
						x => x.SelectItemVM.SelectedItem,
						(modName, selChapter) => selChapter != null && !chaptersDic[selChapter].Mods.Contains(modName.ToLower()));

			IObservable<bool> validateForRootDir =
				addModVM.WhenAnyValue(
					x => x.TextInputVM.Text,
					x => x.SelectItemVM.SelectedItem,
					(modName, selChapter) => selChapter != Res.RootChapter || !chapterLowerNames.Contains(modName.ToLower()));

			List<DialogValidator> validators = new(_validateConditions.Select(cond => cond.ToValidator(textInputVM)))
			{
				new(validate, Res.NameAlreadyExists),
				new(validateForRootDir, Res.NameIsOccupiedByChapter)
			};

			textInputVM.SetValidator(validators);

			view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			view.DataContext = addModVM;

			AddModBoxResult res = new();
			void Closing(object? sender, WindowClosingEventArgs e)
			{
				res = new AddModBoxResult { OK = view.OK, 
					Chapter = selectItemVM.SelectedItem, 
					ModName = textInputVM.IsValid && view.OK ? textInputVM.Text : null };
			}

			view.Closing += Closing;
			await view.ShowDialog(owner);
			view.Closing -= Closing;

			return res;
		}
	}

	public class AddModBoxResult
	{
		public bool OK { get; init; }
		public string? Chapter { get; init; }
		public string? ModName { get; init; }
	}

	public class ChapterItem
	{
		public string Name { get; init; } = string.Empty;

		public HashSet<string> Mods { get; set; } = [];
	}

}
