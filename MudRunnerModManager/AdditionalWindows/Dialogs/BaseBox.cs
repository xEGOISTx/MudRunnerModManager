using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MudRunnerModManager.AdditionalWindows.Dialogs
{
	public abstract class BaseBox<TResult>
	{
		private readonly DialogWindowSettings _dialogWindowSettings;
		private readonly IEnumerable<DialogButton> _buttons;

		protected BaseBox(DialogButton[] buttons, DialogWindowSettings dialogWindowSettings) 
		{ 
			_dialogWindowSettings = dialogWindowSettings;
			_buttons = buttons;
		}

		public async Task<TResult> ShowDialogAsync(Window owner)
		{
			if (owner == null)
				throw new ArgumentNullException(nameof(owner));

			DialogWindow dialogWindow = new();
			SetSettings(dialogWindow);
			var content = GetBoxContent();
			SetContent(dialogWindow, content);

			TResult? res = default;
			void Closing(object? sender, WindowClosingEventArgs e)
			{
				res = GetResult(dialogWindow.Result, content);
			}

			dialogWindow.Closing += Closing;
			await dialogWindow.ShowDialog(owner);
			dialogWindow.Closing -= Closing;

			return res ?? throw new Exception("Result not set");
		}

		protected abstract BoxContent GetBoxContent();

		protected abstract TResult GetResult(string buttonResult, BoxContent content);

		private void SetSettings(DialogWindow dialogWindow)
		{
			dialogWindow.Height = _dialogWindowSettings.Height;
			dialogWindow.Width = _dialogWindowSettings.Width;
			dialogWindow.MaxHeight = _dialogWindowSettings.MaxHeight;
			dialogWindow.MinHeight = _dialogWindowSettings.MinHeight;
			dialogWindow.MaxWidth = _dialogWindowSettings.MaxWidth;
			dialogWindow.MinWidth = _dialogWindowSettings.MinWidth;
			dialogWindow.WindowStartupLocation = _dialogWindowSettings.WindowStartupLocation;
			dialogWindow.CanResize = _dialogWindowSettings.CanResize;
			dialogWindow.Title = _dialogWindowSettings.WindowTitle;
			dialogWindow.Icon = _dialogWindowSettings.WindowIcon;
			dialogWindow.SizeToContent = _dialogWindowSettings.SizeToContent;
		}

		private void SetContent(DialogWindow dialogWindow, BoxContent content) 
		{
			dialogWindow.DialogButtons = dialogWindow.DialogButtons = _buttons.Select(b => new DialogWindowButton
			{
				Name = b.Name,
				Width = b.Width,
				CanExecute = !b.IsCancel ? content.CanExecute : null,
			}).ToArray();

			if(content.UserDialogImage != null)
			{
				dialogWindow.DialogImage = content.UserDialogImage;
			}
			else if(content.DialogImage != DialogImage.None)
			{
				var imgKey = GetImageKey(content.DialogImage);
				//todo: возможно стоит убрать из общих ресурсов
				if (Application.Current.Resources.TryGetResource(imgKey, null, out object? image))
				{
					dialogWindow.DialogImage = image as IImage;
				}
			}

			dialogWindow.DialogContent = content.Content;
			dialogWindow.DataContext = content.DataContext;
		}

		private string GetImageKey(DialogImage msgImage)
		{
			return msgImage switch
			{
				DialogImage.Warning => "warningImage",
				DialogImage.Error => "errorImage",
				DialogImage.Question => "questionImage",
				DialogImage.Info => "infoImage",
				DialogImage.Success => "successImage",
				_ => ""
			};

		}

	}

	public class BoxContent
	{
		public BoxContent(object content) 
		{ 
			Content = content;
		}

		public IObservable<bool>? CanExecute { get; init; }

		public object Content { get; }

		public object? DataContext { get; init; }

		public DialogImage DialogImage { get; init; }

		public IImage? UserDialogImage { get; init; }
	}
}
