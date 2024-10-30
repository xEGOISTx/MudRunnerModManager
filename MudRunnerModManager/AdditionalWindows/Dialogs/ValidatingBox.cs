using Avalonia.Controls;
using ReactiveUI;
using ReactiveUI.Validation.Abstractions;

namespace MudRunnerModManager.AdditionalWindows.Dialogs
{
	public abstract class ValidatingBox<TView, TViewModel, TResult> : BaseBox<TResult>
		where TView : ContentControl where  TViewModel : IValidatableViewModel
	{
		protected ValidatingBox(DialogButton[] buttons, DialogWindowSettings dialogWindowSettings) 
			: base(buttons, dialogWindowSettings)
		{

		}

		protected override BoxContent GetBoxContent()
		{
			var (view, viewModel) = GetContent();
			SetValidation(viewModel);

			return new BoxContent(view)
			{
				CanExecute = viewModel.WhenAnyValue(vm => vm.ValidationContext.IsValid, isValid => isValid == true),
				DataContext = viewModel,
			};
		}

		protected override TResult GetResult(string buttonResult, BoxContent content)
		{
			return GetResult(buttonResult, (TViewModel)content.DataContext);
		}

		protected abstract (TView view, TViewModel dataContext) GetContent();

		protected abstract void SetValidation(TViewModel viewModel);

		protected abstract TResult GetResult(string buttonResult, TViewModel viewModel);
	}
}
