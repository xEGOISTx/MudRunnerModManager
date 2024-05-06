using MudRunnerModManager.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System.Collections.Generic;


namespace MudRunnerModManager.AdditionalWindows.SelectItemDialog
{
	public class SelectItemViewModel : ViewModelBase
	{
		private string? _selectedItem;
		private List<string> _items = [];
		private string _description = string.Empty;

		public List<string> Items
		{
			get => _items;
			set => this.RaiseAndSetIfChanged(ref _items, value, nameof(Items));
		}

		public string? SelectedItem
		{
			get => _selectedItem;
			set
			{
				if(value != _selectedItem)
				{
					_selectedItem = value;
					this.RaisePropertyChanged(nameof(SelectedItem));
					this.RaisePropertyChanged(nameof(IsValid));
				}
			}
		}

		public string Description
		{
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value, nameof(Description));
		}

		public bool IsValid => ValidationContext.IsValid;

		public void SetValidator(IEnumerable<DialogValidator> validators)
		{
			this.IsValid();
			foreach (var validator in validators)
			{
				this.ValidationRule(vm => vm.SelectedItem, validator.Rule, validator.Message);
			}
		}
	}
}
