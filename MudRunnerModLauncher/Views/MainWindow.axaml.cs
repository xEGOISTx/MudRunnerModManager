using Avalonia.Controls;

namespace MudRunnerModLauncher.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Instsnce = this;
    }

    public static MainWindow? Instsnce;
}
