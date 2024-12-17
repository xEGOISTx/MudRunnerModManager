using ReactiveUI.Validation.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MudRunnerModManager.AdditionalWindows.Dialogs.TextInputDialog
{
    public class TextInputBox : ValidatingBox<TextInputView, TextInputViewModel, TexInputBoxResult>
    {
		private readonly TextInputViewModel _viewModel;
        IEnumerable<TextInputValidationCondition>? _validationConditions;

		public TextInputBox(string defaultText,
            string description,
            DialogButton[] buttons,
            IEnumerable<TextInputValidationCondition>? validationConditions = null,
			DialogWindowSettings? settings = null) :
			base (buttons, settings ?? new DialogWindowSettings())
        {
			_viewModel = new()
			{
				Description = description,
				Text = defaultText,
			};

            _validationConditions = validationConditions;
        }

		protected override (TextInputView view, TextInputViewModel dataContext) GetContent()
		{
			return (new TextInputView(), _viewModel);
		}

		protected override TexInputBoxResult GetResult(string buttonResult, TextInputViewModel viewModel)
		{
			return new TexInputBoxResult { Result = buttonResult, Text = viewModel.ValidationContext.IsValid ? viewModel.Text.Trim() : string.Empty };
		}

		protected override void SetValidation(TextInputViewModel viewModel)
		{
            if(_validationConditions != null && _validationConditions.Any())
            {
                viewModel.IsValid();
                foreach(var validation in _validationConditions)
                {
                    viewModel.ValidationRule<TextInputViewModel, string>(vm => vm.Text, validation.Condition, validation.Message);
                }
            }
		}

	}

    public class TextInputValidationCondition
    {
        public TextInputValidationCondition(Func<string?, bool> condition, string message)
        {
            Condition = condition;
            Message = message;
        }

        public Func<string?, bool> Condition { get; }

        public string Message { get; }
    }

    public class TexInputBoxResult
    {
        public string Result { get; init; } = string.Empty;

        public string Text { get; init; } = string.Empty;
    }
}
