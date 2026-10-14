using DucHanhTiepTan.Api.Common;

namespace DucHanhTiepTan.Api.Controllers.Reports;

/// <summary>
/// Date range filter shared by every statistics screen (legacy radio buttons: Tất cả / Hôm nay / Hôm qua / Ngày / Tháng / Từ ngày – đến ngày).
/// Both bounds null = "Tất cả" (DateTime.MinValue in the legacy code).
/// </summary>
public record ReportRange(DateOnly? From, DateOnly? To)
{
    public bool IsAll => From is null && To is null;
    /// <summary>Inclusive lower bound as DateTime (00:00:00).</summary>
    public DateTime FromDateTime => (From ?? DateOnly.MinValue).ToDateTime(TimeOnly.MinValue);
    /// <summary>Exclusive upper bound as DateTime (next day 00:00:00) so that "CreatedDate.Date &lt;= To" can be expressed without date_trunc.</summary>
    public DateTime ToExclusive => (To ?? From ?? DateOnly.MaxValue).AddDays(To is null && From is null ? 0 : 1).ToDateTime(TimeOnly.MinValue);

    public static ReportRange Parse(DateOnly? from, DateOnly? to)
    {
        if (from is null && to is null) return new ReportRange(null, null);
        var f = from ?? to!.Value;
        var t = to ?? from!.Value;
        if (t < f) (f, t) = (t, f);
        return new ReportRange(f, t);
    }

    public string Label => IsAll ? "Tất cả" : From == To ? $"Ngày {From:dd/MM/yyyy}" : $"Từ {From:dd/MM/yyyy} đến {To:dd/MM/yyyy}";
}

/// <summary>Pivot table result (rows = dates or groups, columns = doctors/branches), with totals like the legacy DataTable grids.</summary>
public record PivotColumn(string Key, string Label);
public record PivotRow(string Key, string Label, Dictionary<string, long> Values, long Total);
public record PivotResult(List<PivotColumn> Columns, List<PivotRow> Rows, Dictionary<string, long> ColumnTotals, long GrandTotal);

public static class Pivot
{
    public const string EmptyDoctorKey = "NoName";
    public const string EmptyDoctorLabel = "Không tên";

    /// <summary>Builds a pivot from flat aggregated cells (rowKey, rowLabel, colKey, colLabel, value).</summary>
    public static PivotResult Build(IEnumerable<(string RowKey, string RowLabel, string ColKey, string ColLabel, long Value)> cells, IEnumerable<PivotColumn>? extraColumns = null, Func<string, string, int>? rowSort = null)
    {
        var columns = new Dictionary<string, PivotColumn>();
        if (extraColumns is not null)
            foreach (var c in extraColumns) columns[c.Key] = c;
        var rows = new Dictionary<string, (string Label, Dictionary<string, long> Values)>();
        foreach (var c in cells)
        {
            if (!columns.ContainsKey(c.ColKey)) columns[c.ColKey] = new PivotColumn(c.ColKey, c.ColLabel);
            if (!rows.TryGetValue(c.RowKey, out var row))
            {
                row = (c.RowLabel, new Dictionary<string, long>());
                rows[c.RowKey] = row;
            }
            row.Values[c.ColKey] = row.Values.GetValueOrDefault(c.ColKey) + c.Value;
        }
        var colTotals = columns.Keys.ToDictionary(k => k, _ => 0L);
        long grand = 0;
        var resultRows = new List<PivotRow>();
        foreach (var (key, row) in rows)
        {
            long total = 0;
            foreach (var (ck, v) in row.Values)
            {
                total += v;
                colTotals[ck] += v;
            }
            grand += total;
            resultRows.Add(new PivotRow(key, row.Label, row.Values, total));
        }
        if (rowSort is not null) resultRows.Sort((a, b) => rowSort(a.Key, b.Key));
        else resultRows.Sort((a, b) => string.CompareOrdinal(a.Key, b.Key));
        return new PivotResult(columns.Values.ToList(), resultRows, colTotals, grand);
    }

    /// <summary>Excel export of a pivot: first column = row label, then one column per pivot column, then "Tổng"; last row = totals.</summary>
    public static byte[] ToExcel(PivotResult pivot, string sheetName, string firstColumnHeader, string title)
    {
        var rows = pivot.Rows.Select(r => (Row: r, IsTotal: false)).ToList();
        rows.Add((new PivotRow("__total", "Tổng cộng", pivot.ColumnTotals, pivot.GrandTotal), true));
        var columns = new List<ExcelExporter.Column<(PivotRow Row, bool IsTotal)>>
        {
            new(firstColumnHeader, x => x.Row.Label),
        };
        foreach (var c in pivot.Columns)
            columns.Add(new ExcelExporter.Column<(PivotRow Row, bool IsTotal)>(c.Label, x => x.Row.Values.TryGetValue(c.Key, out var v) ? v : null));
        columns.Add(new ExcelExporter.Column<(PivotRow Row, bool IsTotal)>("Tổng", x => x.Row.Total));
        return ExcelExporter.Build(sheetName, rows, columns, title);
    }
}
