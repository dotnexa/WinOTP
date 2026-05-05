using System.Windows;

namespace TotpViewerWpf;

public partial class AddMethodWindow : Window
{
    public string SelectedMethod { get; private set; } = "";

    public AddMethodWindow()
    {
        InitializeComponent();
    }

    private void Manual_Click(object sender, RoutedEventArgs e)
    {
        SelectedMethod = "manual";
        DialogResult = true;
    }

    private void Qr_Click(object sender, RoutedEventArgs e)
    {
        SelectedMethod = "qr";
        DialogResult = true;
    }
}
