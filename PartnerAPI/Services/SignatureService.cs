using PartnerAPI.Models;
using System.Security.Cryptography;
using System.Text;

public class SignatureService
{
    public bool Verify(PartnerSubmittedRequest r)
    {
        // Parse timestamp (ISO 8601 UTC) and format to yyyyMMddHHmmss
        if (!DateTimeOffset.TryParse(r.timestamp, out var ts)) return false;
        var sigTimestamp = ts.ToUniversalTime().ToString("yyyyMMddHHmmss");

        // the signature format
        var concat = $"{sigTimestamp}{r.partnerkey}{r.partnerrefno}{r.totalamount}{r.partnerpassword}";

        // convert to the lowercase hex
        var hexLower = Sha256HexUtf8(concat);                   
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(hexLower));

        return string.Equals(base64, r.sig?.Trim(), StringComparison.Ordinal);
    }

    private static string Sha256HexUtf8(string s)
    {
        using var sha = SHA256.Create();
        var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
        var sb = new StringBuilder(hash.Length * 2);
        //convert to the lower hex
        foreach (var b in hash) sb.Append(b.ToString("x2"));   
        return sb.ToString();
    }
}
