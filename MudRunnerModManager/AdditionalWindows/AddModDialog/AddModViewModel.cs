using MudRunnerModManager.AdditionalWindows.Dialogs.SelectItemDialog;
using MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog;
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
			ValidationContext.Add(TextInputVM.ValidationContext);
			ValidationContext.Add(SelectItemVM.ValidationContext);
		}


		public TextInputViewModel TextInputVM { get; private set; }

		public SelectItemViewModel SelectItemVM { get; private set; }
	}
}
