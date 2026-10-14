using ClosedXML.Excel;

namespace DucHanhTiepTan.Api.Common;

/// <summary>Replacement for the legacy "Lưu Excel" ribbon button: builds an .xlsx from a list of rows.</summary>
public static class ExcelExporter
{
    public record Column<T>(string Header, Func<T, object?> Value, string? NumberFormat = null);

    public static byte[] Build<T>(string sheetName, IEnumerable<T> rows, IReadOnlyList<Column<T>> columns, string? title = null)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add(string.IsNullOrWhiteSpace(sheetName) ? "Sheet1" : sheetName[..Math.Min(31, sheetName.Length)]);
        var row = 1;
        if (!string.IsNullOrWhiteSpace(title))
        {
            ws.Cell(row, 1).Value = title;
            ws.Cell(row, 1).Style.Font.Bold = true;
            ws.Cell(row, 1).Style.Font.FontSize = 14;
            ws.Range(row, 1, row, columns.Count).Merge();
            row += 2;
        }
        for (var c = 0; c < columns.Count; c++)
        {
            var cell = ws.Cell(row, c + 1);
            cell.Value = columns[c].Header;
            cell.Style.Font.Bold = true;
            cell.Style.Fill.BackgroundColor = XLColor.LightGray;
            cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
        }
        row++;
        foreach (var item in rows)
        {
            for (var c = 0; c < columns.Count; c++)
            {
                var cell = ws.Cell(row, c + 1);
                var value = columns[c].Value(item);
                cell.Value = value switch
                {
                    null => Blank.Value,
                    DateTime dt => dt,
                    DateOnly d => d.ToDateTime(TimeOnly.MinValue),
                    bool b => b ? "Có" : "Không",
                    string s => s,
                    _ => XLCellValue.FromObject(value),
                };
                if (columns[c].NumberFormat is not null) cell.Style.NumberFormat.Format = columns[c].NumberFormat;
                else if (value is DateTime) cell.Style.DateFormat.Format = "dd/MM/yyyy HH:mm";
                else if (value is DateOnly) cell.Style.DateFormat.Format = "dd/MM/yyyy";
                else if (value is long or int or double or decimal) cell.Style.NumberFormat.Format = "#,##0";
                cell.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            }
            row++;
        }
        ws.Columns().AdjustToContents();
        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    public const string ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
}
