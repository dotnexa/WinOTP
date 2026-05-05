using OtpNet;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace TotpViewerWpf;

public partial class MainWindow : Window
{
    private readonly List<TotpAccount> _accounts;
    public ObservableCollection<TotpRow> Rows { get; } = [];

    public MainWindow()
    {
        InitializeComponent();
        DataContext = this;
        _accounts = SecureStore.Load();
        RefreshRows();

        var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1) };
        timer.Tick += (_, _) => RefreshRows();
        timer.Start();

        Loaded += MainWindow_Loaded;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await Task.Delay(3000);

        var fade = new DoubleAnimation
        {
            From = 1.0,
            To = 0.0,
            Duration = TimeSpan.FromMilliseconds(450),
            EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut }
        };
        fade.Completed += (_, _) => SplashOverlay.Visibility = Visibility.Collapsed;
        SplashOverlay.BeginAnimation(OpacityProperty, fade);
    }

    private void AddButton_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new AddMethodWindow { Owner = this };
        if (dialog.ShowDialog() != true) return;

        if (dialog.SelectedMethod == "manual")
        {
            var manual = new ManualAddWindow { Owner = this };
            if (manual.ShowDialog() == true)
                AddAccount(manual.Issuer, manual.Username, manual.Secret);
            return;
        }

        if (dialog.SelectedMethod == "qr")
        {
            var qr = new QrScanWindow { Owner = this };
            if (qr.ShowDialog() == true)
                HandleQrText(qr.QrText);
        }
    }

    private void RowMenu_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { ContextMenu: { } menu } btn)
        {
            menu.PlacementTarget = btn;
            menu.IsOpen = true;
        }
    }

    private void DeleteRow_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem { CommandParameter: TotpRow row } || row.Account is null)
            return;

        var label = string.IsNullOrWhiteSpace(row.Username) ? row.Issuer : $"{row.Issuer} ({row.Username})";
        var result = MessageBox.Show($"\"{label}\" silinsin mi?", "Hesabı sil",
            MessageBoxButton.YesNo, MessageBoxImage.Question);
        if (result != MessageBoxResult.Yes) return;

        _accounts.Remove(row.Account);
        SecureStore.Save(_accounts);
        RefreshRows();
    }

    private async void Code_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is not TextBlock tb || tb.DataContext is not TotpRow row || string.IsNullOrWhiteSpace(row.Code))
            return;

        Clipboard.SetText(row.Code.Replace(" ", ""));
        var oldText = tb.Text;
        var oldBrush = tb.Foreground;
        tb.Foreground = Brushes.Green;
        tb.Text = "Kopyalandı ✓";
        await Task.Delay(800);
        tb.Foreground = oldBrush;
        tb.Text = oldText;
    }

    private void HandleQrText(string qrText)
    {
        try
        {
            if (qrText.StartsWith("otpauth://", StringComparison.OrdinalIgnoreCase))
            {
                var account = OtpAuthParser.Parse(qrText);
                AddAccount(account.Issuer, account.Username, account.Secret);
                return;
            }

            if (qrText.StartsWith("otpauth-migration://", StringComparison.OrdinalIgnoreCase))
            {
                var imported = GoogleAuthenticatorMigrationImporter.Import(qrText);
                foreach (var account in imported)
                    AddAccount(account.Issuer, account.Username, account.Secret, saveImmediately: false);
                SecureStore.Save(_accounts);
                RefreshRows();
                MessageBox.Show($"{imported.Count} hesap içe aktarıldı.");
                return;
            }

            MessageBox.Show("Desteklenmeyen QR formatı.");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"QR içe aktarılamadı: {ex.Message}");
        }
    }

    private void AddAccount(string issuer, string username, string secret, bool saveImmediately = true)
    {
        issuer = string.IsNullOrWhiteSpace(issuer) ? "Hesap" : issuer.Trim();
        username = username.Trim();
        secret = secret.Replace(" ", "").Trim();

        _ = Base32Encoding.ToBytes(secret);

        _accounts.Add(new TotpAccount
        {
            Issuer = issuer,
            Username = username,
            Secret = secret
        });

        if (saveImmediately)
            SecureStore.Save(_accounts);

        RefreshRows();
    }

    private void RefreshRows()
    {
        Rows.Clear();
        foreach (var account in _accounts)
        {
            try
            {
                var secretBytes = Base32Encoding.ToBytes(account.Secret);
                var totp = new Totp(secretBytes);
                var rawCode = totp.ComputeTotp();
                Rows.Add(new TotpRow
                {
                    Issuer = account.Issuer,
                    Username = account.Username,
                    Code = rawCode.Length == 6 ? $"{rawCode[..3]} {rawCode[3..]}" : rawCode,
                    Remaining = totp.RemainingSeconds().ToString(),
                    RemainingSeconds = totp.RemainingSeconds(),
                    Account = account
                });
            }
            catch
            {
                Rows.Add(new TotpRow { Issuer = account.Issuer, Username = account.Username, Code = "Hatalı", Remaining = "-", Account = account });
            }
        }
    }
}
