using System;
using System.Collections.Generic;
using System.Linq;
using Res = MudRunnerModManager.Lang.Resource;
using ReactiveUI;
using MudRunnerModManager.AdditionalWindows.Dialogs;
using MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog;
using ReactiveUI.Validation.Extensions;
using MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog;

namespace MudRunnerModManager.AdditionalWindows.AddModDialog
{
	//todo: придумать что то типа комбинированного дилога
    public class AddModBox : ValidatingBox<AddModView, AddModViewModel, AddModBoxResult> 
	{
		private readonly IEnumerable<ChapterItem> _chapters;
		private readonly string _defaultModName;
		private readonly IEnumerable<TextInputValidationCondition> _validateConditions;
		private readonly AddModViewModel _viewModel;

		public AddModBox(string defaultModName, 
			IEnumerable<ChapterItem> chapters, 
			IEnumerable<TextInputValidationCondition> nameValidateConditions,
			DialogWindowSettings? settings = null) :
			base(new DialogButton[1] { new() { Name = Res.OK, Width = 80 } }, settings ?? new DialogWindowSettings())
		{
			_defaultModName = defaultModName;
			_validateConditions = nameValidateConditions;
			_chapters = chapters;
			foreach (var chapter in chapters)
			{
				chapter.Mods = chapter.Mods.Select(m => m.ToLower()).ToHashSet();
			}

			var itms = _chapters.Select(chap => new SelectItemBoxItem<ChapterItem>(chap, chap => chap.Name) as ISelectItem).ToList();
			SelectItemViewModel selectItemVM = new()
			{
				Description = Res.SelectChapter,
				Items = itms,
				SelectedItem = itms.FirstOrDefault()
			};

			TextInputViewModel textInputVM = new()
			{
				Description = Res.EnterModName,
				Text = _defaultModName,
			};

			_viewModel = new(textInputVM, selectItemVM);
		}


		protected override (AddModView view, AddModViewModel dataContext) GetContent()
		{
			return (new AddModView(), _viewModel);
		}

		protected override AddModBoxResult GetResult(string buttonResult, AddModViewModel viewModel)
		{
			string? selChapter = null;
			if(viewModel.SelectItemVM.SelectedItem is SelectItemBoxItem<ChapterItem> selItem)
				selChapter = selItem.Item.Name;

			return new AddModBoxResult
			{
				OK = buttonResult == Res.OK,
				Chapter = selChapter,
				ModName = viewModel.TextInputVM.ValidationContext.IsValid ? viewModel.TextInputVM.Text : null
			};
		}

		protected override void SetValidation(AddModViewModel viewModel)
		{
			var chaptersDic = _chapters.ToDictionary(ch => ch.Name, ch => ch);
			var chapterLowerNames = new HashSet<string>(chaptersDic.Keys.Select(chName => chName.ToLower()));

			IObservable<bool> validate1 =
				viewModel.WhenAnyValue(
					x => x.TextInputVM.Text,
					x => x.SelectItemVM.SelectedItem,
					(modName, selChapter) => selChapter != null && !chaptersDic[(selChapter as SelectItemBoxItem<ChapterItem>).Item.Name].Mods.Contains(modName.ToLower()));

			IObservable<bool> validateForRootDir =
				viewModel.WhenAnyValue(
					x => x.TextInputVM.Text,
					x => x.SelectItemVM.SelectedItem,
					(modName, selChapter) => (selChapter as SelectItemBoxItem<ChapterItem>).Item.Name != Res.RootChapter || !chapterLowerNames.Contains(modName.ToLower()));


			if (_validateConditions != null && _validateConditions.Any())
			{
				foreach (var validate in _validateConditions)
				{
					viewModel.TextInputVM.ValidationRule(vm => vm.Text, validate.Condition, validate.Message);
				}
			}

			viewModel.TextInputVM.ValidationRule(vm => vm.Text, validate1, Res.NameAlreadyExists);
			viewModel.TextInputVM.ValidationRule(vm => vm.Text, validateForRootDir, Res.NameIsOccupiedByChapter);
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