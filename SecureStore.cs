using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace TotpViewerWpf;

public static class SecureStore
{
    private static readonly string Folder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
        "TotpViewerWpf");

    private static readonly string FilePath = Path.Combine(Folder, "accounts.secure");

    public static List<TotpAccount> Load()
    {
        try
        {
            if (!File.Exists(FilePath))
                return [];

            var encrypted = File.ReadAllBytes(FilePath);
            var decrypted = ProtectedData.Unprotect(encrypted, null, DataProtectionScope.CurrentUser);
            var json = Encoding.UTF8.GetString(decrypted);
            return JsonSerializer.Deserialize<List<TotpAccount>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }

    public static void Save(List<TotpAccount> accounts)
    {
        Directory.CreateDirectory(Folder);
        var json = JsonSerializer.Serialize(accounts, new JsonSerializerOptions { WriteIndented = true });
        var data = Encoding.UTF8.GetBytes(json);
        var encrypted = ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser);
        File.WriteAllBytes(FilePath, encrypted);
    }
}
