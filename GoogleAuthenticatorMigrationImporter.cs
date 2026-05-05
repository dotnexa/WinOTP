using Google.Protobuf;
using OtpNet;
using System.Web;

namespace TotpViewerWpf;

public static class GoogleAuthenticatorMigrationImporter
{
    public static List<TotpAccount> Import(string migrationUri)
    {
        var uri = new Uri(migrationUri);
        var query = HttpUtility.ParseQueryString(uri.Query);
        var data = query["data"];

        if (string.IsNullOrWhiteSpace(data))
            throw new InvalidOperationException("Migration QR içinde data alanı yok.");

        var bytes = Convert.FromBase64String(data);
        var payload = MigrationPayload.Parser.ParseFrom(bytes);
        var result = new List<TotpAccount>();

        foreach (var otp in payload.OtpParameters)
        {
            if (otp.Type != OtpType.Totp)
                continue;

            var secretBase32 = Base32Encoding.ToString(otp.Secret.ToByteArray());
            var username = otp.Name;
            var issuer = otp.Issuer;

            if (string.IsNullOrWhiteSpace(issuer) && username.Contains(':'))
            {
                var parts = username.Split(':', 2);
                issuer = parts[0];
                username = parts[1];
            }

            result.Add(new TotpAccount
            {
                Issuer = issuer,
                Username = username,
                Secret = secretBase32
            });
        }

        return result;
    }
}
