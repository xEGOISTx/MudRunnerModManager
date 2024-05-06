using Avalonia.Controls;
using System.Diagnostics.CodeAnalysis;

namespace MudRunnerModManager.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Instsnce = this;
    }

    [AllowNull]
    public static MainWindow Instsnce;
}
