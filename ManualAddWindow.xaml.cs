using OtpNet;
using System.Windows;

namespace TotpViewerWpf;

public partial class ManualAddWindow : Window
{
    public string Issuer { get; private set; } = "";
    public string Username { get; private set; } = "";
    public string Secret { get; private set; } = "";

    public ManualAddWindow()
    {
        InitializeComponent();
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        Issuer = IssuerBox.Text.Trim();
        Username = UsernameBox.Text.Trim();
        Secret = SecretBox.Text.Trim().Replace(" ", "");

        if (string.IsNullOrWhiteSpace(Issuer) || string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Secret))
        {
            MessageBox.Show("Tüm alanları doldur.");
            return;
        }

        try
        {
            _ = Base32Encoding.ToBytes(Secret);
        }
        catch
        {
            MessageBox.Show("Secret Key geçersiz. Base32 formatında olmalı.");
            return;
        }

        DialogResult = true;
    }
}
