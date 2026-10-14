using System.Globalization;
using System.Text;

namespace DucHanhTiepTan.Api.Common;

/// <summary>
/// Port of Tmt.Toolkit.VietnameseTones: accent-insensitive search support.
/// </summary>
public static class VietnameseText
{
    /// <summary>Lower-case, remove Vietnamese diacritics ("Nguyễn Văn Đức" -> "nguyen van duc").</summary>
    public static string Normalize(string? input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var formD = input.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(formD.Length);
        foreach (var ch in formD)
        {
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (cat == UnicodeCategory.NonSpacingMark) continue;
            sb.Append(ch switch { 'đ' => 'd', 'Đ' => 'D', _ => ch });
        }
        return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant().Trim();
    }

    /// <summary>True when <paramref name="text"/> contains <paramref name="keyword"/> ignoring case and accents.</summary>
    public static bool Contains(string? text, string? keyword)
    {
        if (string.IsNullOrWhiteSpace(keyword)) return true;
        return Normalize(text).Contains(Normalize(keyword), StringComparison.Ordinal);
    }
}
