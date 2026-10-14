using System.Security.Cryptography;
using System.Text;

namespace DucHanhTiepTan.Api.Security;

/// <summary>
/// Password hashing compatible with the legacy WinForms app (Tmt.Toolkit.MD5Encryptor):
/// lowercase hex MD5 of the UTF-8 password. Kept so that existing accounts keep working.
/// </summary>
public static class Md5Hasher
{
    public static string Hash(string input)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(input));
        var sb = new StringBuilder(bytes.Length * 2);
        foreach (var b in bytes) sb.Append(b.ToString("x2"));
        return sb.ToString();
    }

    public static bool Verify(string input, string hash) =>
        string.Equals(Hash(input), hash, StringComparison.OrdinalIgnoreCase);
}
