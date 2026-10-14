using DucHanhTiepTan.Api.Common;

namespace DucHanhTiepTan.Api.Controllers.Examination;

/// <summary>
/// Port of the legacy PositionsAdapter: parses "Số răng" strings such as "11,12,21-23"
/// (quadrant 1-4, tooth 1-8; "a-b" is a bridge) and computes the implied quantity.
/// </summary>
public static class ToothPositions
{
    private record Teeth(int Cau, int Rang)
    {
        public static Teeth Parse(string value)
        {
            if (value.Length != 2 || !int.TryParse(value[..1], out var cau) || !int.TryParse(value[1..], out var rang))
                throw new ApiException(value + " không phải là số răng");
            if (cau > 4 || cau <= 0) throw new ApiException("Số hàm phải nhỏ hơn hoặc bằng 4 và lớn hơn 0");
            if (rang > 8 || rang <= 0) throw new ApiException("Số răng phải nhỏ hơn hoặc bằng 8 và lớn hơn 0");
            return new Teeth(cau, rang);
        }
        public bool Upper => Cau <= 2;
    }

    private class Tooth
    {
        public Teeth? R; public Teeth? F; public Teeth? T; public int Quantity;

        public static Tooth Parse(string value)
        {
            if (value.Contains('-'))
            {
                var parts = value.Split('-');
                var f = Teeth.Parse(parts[0].Trim());
                if (parts.Length < 2 || parts[1].Trim().Length == 0) throw new ApiException("Vui lòng chỉ vị trí răng thứ 2 của cầu răng");
                var t = Teeth.Parse(parts[1].Trim());
                if (f == t) throw new ApiException("Hai răng của một cầu răng phải khác nhau");
                if (f.Upper != t.Upper) throw new ApiException("Hai răng của một cầu răng không thể nằm ở 2 hàm khác nhau.");
                var qty = f.Cau == t.Cau ? Math.Abs(f.Rang - t.Rang) + 1 : f.Rang + t.Rang;
                return new Tooth { F = f, T = t, Quantity = qty };
            }
            return new Tooth { R = Teeth.Parse(value), Quantity = 1 };
        }

        public bool Contains(Teeth teeth)
        {
            if (R is not null) return R == teeth;
            if (F!.Upper != teeth.Upper) return false;
            if (F.Cau == T!.Cau)
            {
                if (teeth.Cau != F.Cau) return false;
                var lo = Math.Min(F.Rang, T.Rang); var hi = Math.Max(F.Rang, T.Rang);
                return lo <= teeth.Rang && teeth.Rang <= hi;
            }
            return teeth.Cau == F.Cau ? teeth.Rang <= F.Rang : teeth.Rang <= T.Rang;
        }

        public bool Collision(Tooth other)
        {
            if (other.R is null && R is null)
            {
                if (F!.Upper != other.F!.Upper) return false;
                return Contains(other.F) || Contains(other.T!) || other.Contains(F);
            }
            if (other.R is null) return other.Contains(R!);
            if (R is null) return Contains(other.R);
            return other.R == R;
        }

        public override string ToString() => R is not null ? $"{R.Cau}{R.Rang}" : $"{F!.Cau}{F.Rang}-{T!.Cau}{T.Rang}";
    }

    /// <summary>Validates and normalizes; returns (normalized string or null, quantity). Quantity 0 when empty.</summary>
    public static (string? Normalized, int Quantity) Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return (null, 0);
        var parts = value.Split(',');
        var teeth = new List<Tooth>();
        foreach (var raw in parts)
        {
            var p = raw.Trim();
            if (p.Length == 0) continue;
            var tooth = Tooth.Parse(p);
            if (teeth.Any(t => t.Collision(tooth))) throw new ApiException("Răng " + p + " bị trùng");
            teeth.Add(tooth);
        }
        if (teeth.Count == 0) return (null, 0);
        return (string.Join(",", teeth.Select(t => t.ToString())), teeth.Sum(t => t.Quantity));
    }
}
