using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MudRunnerModManager.AdditionalWindows.TextInputDialog
{
	//todo: потом переделать как общее окно для диалогов с кнопками
	public partial class TextInputWindow : Window
	{
		public TextInputWindow()
		{
			InitializeComponent();
		}

		public bool OK {  get; private set; } = false;

		public void OK_Click(object sender, RoutedEventArgs e)
		{
			OK = true;
			Close();
		}
	}
}
