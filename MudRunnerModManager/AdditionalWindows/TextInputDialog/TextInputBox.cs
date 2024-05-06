using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.AdditionalWindows.TextInputDialog
{
	public class TextInputBox
	{
		private readonly string _description;
		private readonly string _defaultText;
		private readonly IEnumerable<TextInputValidateCondition>? _validateConditions;

		public TextInputBox(string defaultText,
			string description,  
			IEnumerable<TextInputValidateCondition>? validateConditions = null)
		{
			_description = description;
			_defaultText = defaultText;
			_validateConditions = validateConditions;
		}

		public async Task<TexInputBoxResult> ShowDialog(Window owner)
		{
			if (owner == null)
			{
				throw new ArgumentNullException(nameof(owner));
			}

			TextInputWindow view = new();
			TextInputViewModel viewModel = new()
			{
				Description = _description,
				Text = _defaultText,
			};

			if(_validateConditions != null)
			{
				viewModel.SetValidator(_validateConditions.Select(cond => cond.ToValidator(viewModel)));
			}

			view.WindowStartupLocation = WindowStartupLocation.CenterOwner;
			view.DataContext = viewModel;

			TexInputBoxResult res = new();
			void Closing(object? sender, WindowClosingEventArgs e)
			{
				res = new TexInputBoxResult { OK = view.OK, Text = viewModel.IsValid && view.OK ? viewModel.Text : string.Empty };
			}

			view.Closing += Closing;
			await view.ShowDialog(owner);
			view.Closing -= Closing;

			return res;
		}

	}

	public class TextInputValidateCondition : ValidateConditionBase<TextInputViewModel>
	{
		public TextInputValidateCondition(Func<TextInputViewModel, IObservable<bool>> getCondition, string message)
			: base(getCondition, message)
		{

		}
	}

	public class TexInputBoxResult
	{
		public bool OK { get; init; } = false;

		public string Text { get; init; } = string.Empty;
	}
}
