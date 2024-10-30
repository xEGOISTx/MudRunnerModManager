using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog
{
    public class SelectItemBox<TItem> : ValidatingBox<SelectItemView, SelectItemViewModel, SelectItemBoxResult<TItem>>
    {
        private readonly IEnumerable<SelectItemUserValidationCondition<TItem>>? _validationConditions;
		private readonly SelectItemViewModel _viewModel;

		public SelectItemBox(IEnumerable<TItem> items,
            Func<TItem, string> getDisplayValue,
            string description,
            DialogButton[] buttons,
            IEnumerable<SelectItemUserValidationCondition<TItem>>? validationConditions = null,
			DialogWindowSettings? settings = null) :
            base(buttons, settings ?? new DialogWindowSettings())
        {
            var itms = items.Select(it => new SelectItemBoxItem<TItem>(it, getDisplayValue) as ISelectItem).ToList();
            _viewModel = new()
            {
                Description = description,
                Items = itms,
                SelectedItem = itms.FirstOrDefault(),
            };

			_validationConditions = validationConditions;
		}

		protected override (SelectItemView view, SelectItemViewModel dataContext) GetContent()
		{
            return (new SelectItemView(), _viewModel);
		}

		protected override SelectItemBoxResult<TItem> GetResult(string buttonResult, SelectItemViewModel viewModel)
		{
			TItem? item = viewModel.SelectedItem is SelectItemBoxItem<TItem> sItem ? sItem.Item : default;
			return new SelectItemBoxResult<TItem> { Result = buttonResult, SelectedItem = item };
		}

		protected override void SetValidation(SelectItemViewModel viewModel)
		{
			if (_validationConditions != null && _validationConditions.Any())
			{
				viewModel.IsValid();
				foreach (var validation in _validationConditions)
				{
				    bool Condition(ISelectItem? selectItem)
				    {
					    TItem? item = selectItem is SelectItemBoxItem<TItem> selItem ? selItem.Item : default;
					    return validation.Condition(item);
				    }

					viewModel.ValidationRule<SelectItemViewModel, ISelectItem>(vm => vm.SelectedItem, Condition, validation.Message);
				}
			}
		}
	}

    public class SelectItemUserValidationCondition<TItem>
    {
        public SelectItemUserValidationCondition(Func<TItem?, bool> condition, string message)
        {
			Condition = condition;
            Message = message;
        }

		public Func<TItem?, bool> Condition { get; }

		public string Message { get; }
    }

    public class SelectItemBoxItem<TItem> : ISelectItem
	{
        public SelectItemBoxItem(TItem item, Func<TItem, string> getVal)
        {
            Item = item;
			DisplayValue = getVal(item);
		}

		public TItem Item { get; }
		public object? DisplayValue { get; set; }
	}


	public class SelectItemBoxResult<TItem>
    {
        public string Result { get; init; } = string.Empty;

        public TItem? SelectedItem { get; init; }
    }
}
