using Avalonia.Controls;

namespace MudRunnerModManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Instsnce = this;
    }

    public static MainWindow? Instsnce;
}
