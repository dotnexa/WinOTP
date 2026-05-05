namespace TotpViewerWpf;

public class TotpRow
{
    public string Issuer { get; set; } = "";
    public string Username { get; set; } = "";
    public string Code { get; set; } = "";
    public string Remaining { get; set; } = "";
    public int RemainingSeconds { get; set; }
    public TotpAccount? Account { get; set; }
}
