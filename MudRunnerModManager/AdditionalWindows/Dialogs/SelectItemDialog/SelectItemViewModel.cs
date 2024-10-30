using MudRunnerModManager.ViewModels;
using ReactiveUI;
using System.Collections.Generic;

namespace MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog
{
    public class SelectItemViewModel : ViewModelBase
    {

        private string _description = string.Empty;
        private ISelectItem? _selectedItem;
        private List<ISelectItem> _items = [];

		public ISelectItem? SelectedItem
		{
			get => _selectedItem;
			set
			{
				if (value != _selectedItem)
				{
					_selectedItem = value;
					this.RaisePropertyChanged(nameof(SelectedItem));
				}
			}
		}

		public List<ISelectItem> Items
		{
			get => _items;
			set => this.RaiseAndSetIfChanged(ref _items, value, nameof(Items));
		}

        public string Description
        {
            get => _description;
            set => this.RaiseAndSetIfChanged(ref _description, value, nameof(Description));
        }
    }

    public interface ISelectItem
    {
        public object? DisplayValue { get; set; }
    }

}
