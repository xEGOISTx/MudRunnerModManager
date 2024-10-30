using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using ReactiveUI;
using System;
using System.Reactive;

namespace MudRunnerModManager.AdditionalWindows.Dialogs
{
	public sealed partial class DialogWindow : Window
	{
		public DialogWindow()
		{
			InitializeComponent();
		}

		public static readonly StyledProperty<DialogWindowButton[]> DialogButtonsProperty =
			AvaloniaProperty.Register<DialogWindow, DialogWindowButton[]>(nameof(DialogButtons), [], coerce: OnDialogButtonsChanged);

		public static readonly StyledProperty<object?> DialogContentProperty =
			AvaloniaProperty.Register<DialogWindow, object?>(nameof(DialogContent), null, coerce: OnDialogContentChanged);

		public static readonly StyledProperty<IImage?> DialogImageProperty =
			AvaloniaProperty.Register<DialogWindow, IImage?>(nameof(DialogImage), null, coerce: OnDialogImageChanged);


		public object? DialogContent
		{
			get { return GetValue(DialogContentProperty); }
			set { SetValue(DialogContentProperty, value); }
		}


		public DialogWindowButton[] DialogButtons
		{
			get { return GetValue(DialogButtonsProperty); }
			set { SetValue(DialogButtonsProperty, value); }
		}

		public IImage? DialogImage
		{
			get { return GetValue(DialogImageProperty); }
			set { SetValue(DialogImageProperty, value); }
		}


		public string Result { get; private set; } = string.Empty;


		private static DialogWindowButton[] OnDialogButtonsChanged(AvaloniaObject obj, DialogWindowButton[] buttons)
		{
			if (obj is DialogWindow dWindow)
			{

				dWindow.buttonsPanel.Children.Clear();
				foreach (DialogWindowButton dWButton in buttons)
				{
					Button button = new()
					{
						Margin = new Thickness(3, 0, 0, 0),
						HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Center,
						VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Center,
						Content = dWButton.Name,
						Command = ReactiveCommand.Create<Button, Unit>(dWindow.CloseWindow, dWButton.CanExecute),
						Width = dWButton.Width,
					};
					button.CommandParameter = button;

					dWindow.buttonsPanel.Children.Add(button);
				}
			}

			return buttons;
		}

		private static object? OnDialogContentChanged(AvaloniaObject obj, object? content)
		{
			if (obj is DialogWindow dWindow)
			{
				if (content is string strCont)
				{
					TextBlock tb = new()
					{
						TextWrapping = TextWrapping.Wrap,
						Text = strCont
					};
					dWindow.contPres.Content = tb;
				}
				else
					dWindow.contPres.Content = content;
			}

			return content;
		}

		private static IImage? OnDialogImageChanged(AvaloniaObject obj, IImage? image)
		{
			if (obj is DialogWindow dWindow)
			{
				dWindow.imgPres.Source = image;
				if(image != null)
				{
					dWindow.imgPres.Margin = new Thickness(20);
				}
			}

			return image;
		}

		private Unit CloseWindow(Button button)
		{
			Result = button.Content != null && button.Content is string cont ? cont : string.Empty;
			Close();
			return Unit.Default;
		}
	}


	public class DialogWindowButton
	{ 
		public string? Name { get; set; }

		public IObservable<bool>? CanExecute { get; set; }

		public double Width { get; set; } = double.NaN;
	}

}
