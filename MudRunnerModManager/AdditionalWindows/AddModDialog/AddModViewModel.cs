using MudRunnerModManager.AdditionalWindows.SelectItemDialog;
using MudRunnerModManager.AdditionalWindows.TextInputDialog;
using MudRunnerModManager.ViewModels;
using ReactiveUI;

namespace MudRunnerModManager.AdditionalWindows.AddModDialog
{
	public class AddModViewModel : ViewModelBase
	{
		public AddModViewModel(TextInputViewModel textInputVM, SelectItemViewModel selectItemVM) 
		{
			TextInputVM = textInputVM;
			SelectItemVM = selectItemVM;

			TextInputVM.ValidationContext.PropertyChanged += ValidationContext_PropertyChanged;
		}


		public TextInputViewModel TextInputVM { get; set; }

		public SelectItemViewModel SelectItemVM { get; set; }

		public bool IsValid =>  TextInputVM.IsValid;


		private void ValidationContext_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == nameof(TextInputVM.ValidationContext.IsValid))
				this.RaisePropertyChanged(nameof(IsValid));
		}
	}
}
