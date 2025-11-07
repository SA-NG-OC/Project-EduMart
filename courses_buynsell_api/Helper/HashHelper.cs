using System.Security.Cryptography;
using System.Text;

namespace courses_buynsell_api.Helper;

public static class HashHelper
{
    public static string ComputeHmacSha256(string data, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);
        using var hmac = new HMACSHA256(keyBytes);
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
        var sb = new StringBuilder();
        foreach (var b in hash) sb.AppendFormat("{0:x2}", b);
        return sb.ToString();
    }
}
