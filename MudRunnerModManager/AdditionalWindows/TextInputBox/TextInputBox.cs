using Avalonia.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MudRunnerModManager.AdditionalWindows
{
	public class TextInputBox
	{
		private readonly string _description;
		private readonly string _defaultText;
		private readonly Func<TextInputWindowViewModel, IObservable<bool>> _getTextValidator;
		private readonly string _validatorMessage;

		public TextInputBox(string description, string defaultText, Func<TextInputWindowViewModel, IObservable<bool>> getTextValidator, string validatorMessage)
		{
			_description = description;
			_defaultText = defaultText;
			_getTextValidator = getTextValidator;
			_validatorMessage = validatorMessage;
		}

		public async Task<TexInputBoxResult> ShowDialog(Window owner)
		{
			TextInputWindow view = new();
			TextInputWindowViewModel viewModel = new()
			{
				Description = _description,
				Text = _defaultText,
			};
			viewModel.SetValidator(_getTextValidator(viewModel), _validatorMessage);
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
}
