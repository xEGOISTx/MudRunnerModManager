using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MudRunnerModManager.AdditionalWindows.AddModDialog
{
	public partial class AddModWindow : Window
	{
		//todo: ����� ���������� ��� ����� ���� ��� �������� � ��������
		public AddModWindow()
		{
			InitializeComponent();
		}

		public bool OK { get; private set; } = false;

		public void OK_Click(object sender, RoutedEventArgs e)
		{
			OK = true;
			Close();
		}
	}
}