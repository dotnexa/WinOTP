using Microsoft.Win32;
using System.Drawing;
using System.Windows;
using ZXing.Windows.Compatibility;

namespace TotpViewerWpf;

public partial class QrScanWindow : Window
{
    public string QrText { get; private set; } = "";

    public QrScanWindow()
    {
        InitializeComponent();
    }

    private void SelectImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Image Files|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (dialog.ShowDialog() != true)
            return;

        try
        {
            var reader = new BarcodeReader();
            using var bitmap = (Bitmap)Image.FromFile(dialog.FileName);
            var result = reader.Decode(bitmap);

            if (result == null)
            {
                MessageBox.Show("QR okunamadı.");
                return;
            }

            QrText = result.Text;
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"QR okunamadı: {ex.Message}");
        }
    }
}
