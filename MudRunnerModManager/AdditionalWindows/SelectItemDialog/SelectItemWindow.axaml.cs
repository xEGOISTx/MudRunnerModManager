using Avalonia.Controls;
using Avalonia.Interactivity;

namespace MudRunnerModManager.AdditionalWindows.SelectItemDialog
{
	//todo: ����� ���������� ��� ����� ���� ��� �������� � ��������
	public partial class SelectItemWindow : Window
	{
		public SelectItemWindow()
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
