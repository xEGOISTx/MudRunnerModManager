using MudRunnerModManager.ViewModels;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;
using System;

namespace MudRunnerModManager.AdditionalWindows
{
	public class TextInputWindowViewModel : ViewModelBase
	{
		private string _description = string.Empty;
		private string _text = string.Empty;

		public string Description
		{ 
			get => _description;
			set => this.RaiseAndSetIfChanged(ref _description, value, nameof(Description));
		}

		public string Text
		{
			get => _text;
			set
			{
				this.RaiseAndSetIfChanged(ref _text, value, nameof(Text));
				this.RaisePropertyChanged(nameof(IsValid));
			}
		}

		public bool IsValid => ValidationContext.IsValid;

		public void SetValidator(IObservable<bool>validator, string validatorMessage)
		{
			this.IsValid();
			this.ValidationRule(vm => vm.Text, validator, validatorMessage);
		}
	}
}
