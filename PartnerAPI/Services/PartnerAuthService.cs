namespace PartnerAPI.Services;

public  class PartnerAuthService
{
    // Allowed partners from spec
    private static readonly Dictionary<string, (string PartnerNo, string PlainPassword)> Allowed = new()
    {
        ["FAKEGOOGLE"] = ("FG-00001", "FAKEPASSWORD1234"),
        ["FAKEPEOPLE"] = ("FG-00002", "FAKEPASSWORD4578")
    };

    public bool IsAllowed(string partnerkey) => Allowed.ContainsKey(partnerkey);

    public bool VerifyPasswordBase64(string partnerkey, string base64)
    {
        if (!Allowed.TryGetValue(partnerkey, out var info)) return false;
        var expected = System.Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(info.PlainPassword));
        Console.WriteLine($"[Check] Expected plain password = {expected}");
        Console.WriteLine($"[Check] Expected partner password = {base64}");
        return string.Equals(base64?.Trim(), expected, StringComparison.Ordinal);
    }

    public string? GetPartnerNo(string partnerkey)
    {


        return Allowed.TryGetValue(partnerkey, out var info) ? info.PartnerNo : null;

    }
    }
  
