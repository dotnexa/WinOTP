using System.Web;

namespace TotpViewerWpf;

public static class OtpAuthParser
{
    public static TotpAccount Parse(string otpAuthUri)
    {
        var parsed = new Uri(otpAuthUri);
        var label = Uri.UnescapeDataString(parsed.AbsolutePath.TrimStart('/'));

        var issuer = "";
        var username = label;

        if (label.Contains(':'))
        {
            var parts = label.Split(':', 2);
            issuer = parts[0];
            username = parts[1];
        }

        var query = HttpUtility.ParseQueryString(parsed.Query);
        var secret = query["secret"] ?? "";
        var issuerFromQuery = query["issuer"];

        if (!string.IsNullOrWhiteSpace(issuerFromQuery))
            issuer = issuerFromQuery;

        return new TotpAccount
        {
            Issuer = issuer,
            Username = username,
            Secret = secret.Replace(" ", "").Trim()
        };
    }
}
