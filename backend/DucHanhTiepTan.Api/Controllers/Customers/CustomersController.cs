using System.Text;
using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Customers;

/// <summary>
/// Khách hàng – port of CustomerUserControl / DeclareCustomerDialog / ViewCustomerDialog.
/// </summary>
[ApiController]
[Route("api/customers")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public CustomersController(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    /// <summary>Legacy Configuration "CustomerFormat" ({0:000000}).</summary>
    internal static async Task<string> GetCustomerFormatAsync(AppDbContext db)
    {
        var fmt = await db.Configurations.Where(c => c.Name == "CustomerFormat").Select(c => c.Value).FirstOrDefaultAsync();
        return string.IsNullOrWhiteSpace(fmt) ? "{0:000000}" : fmt;
    }

    internal static string FormatId(string format, long visiableId)
    {
        try { return string.Format(format, visiableId); }
        catch (FormatException) { return visiableId.ToString(); }
    }

    internal static string AddressRepresentation(string? address, string? ward, string? district, string? province)
    {
        var parts = new[] { address, ward, district, province }.Where(p => !string.IsNullOrWhiteSpace(p));
        return string.Join(", ", parts);
    }

    private static readonly string[] DefaultJobs = ["Giáo viên", "Bác sĩ", "Nhà báo", "Doanh nhân", "Công nhân", "Nội trợ", "Kế toán", "Thư kí"];

    // ------------------------------------------------------------------ list

    [HttpGet]
    public async Task<PagedResult<CustomerListDto>> List(
        [FromQuery] string? keyword, [FromQuery] string searchBy = "name", [FromQuery] int? branchId = null,
        [FromQuery] string dateMode = "all", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] string? province = null, [FromQuery] string? district = null, [FromQuery] string? ward = null,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var scope = BranchScope.Resolve(_user, branchId);
        var query = _db.Customers.AsNoTracking().Where(c => scope == 0 || c.BranchId == scope);

        if (dateMode == "today")
        {
            var today = DateTime.Today;
            query = query.Where(c => c.CreatedDate >= today && c.CreatedDate < today.AddDays(1));
        }
        else if (dateMode == "custom" && from.HasValue && to.HasValue)
        {
            var f = from.Value.Date; var t = to.Value.Date.AddDays(1);
            query = query.Where(c => c.CreatedDate >= f && c.CreatedDate < t);
        }
        if (!string.IsNullOrWhiteSpace(province)) query = query.Where(c => c.ProvinceName == province);
        if (!string.IsNullOrWhiteSpace(district)) query = query.Where(c => c.DistrictName == district);
        if (!string.IsNullOrWhiteSpace(ward)) query = query.Where(c => c.WardName == ward);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var kw = keyword.Trim();
            switch (searchBy)
            {
                case "id":
                    {
                        var digits = kw.TrimStart('0');
                        if (long.TryParse(kw, out var exact))
                            query = query.Where(c => c.VisiableId == exact || c.VisiableId.ToString().Contains(digits));
                        else
                            query = query.Where(c => c.VisiableId.ToString().Contains(kw));
                        break;
                    }
                case "phone":
                    query = query.Where(c => c.Phone != null && c.Phone.Contains(kw));
                    break;
                case "job":
                    query = query.WhereUnaccentLike(c => c.Job, kw);
                    break;
                default:
                    query = query.WhereUnaccentLike(c => c.Name, kw);
                    break;
            }
        }

        var format = await GetCustomerFormatAsync(_db);
        var paged = await query.OrderByDescending(c => c.CreatedDate).ThenByDescending(c => c.Id)
            .Select(c => new
            {
                c.Id, c.VisiableId, c.Name, c.Sex, c.BirthYear, c.Address, c.ProvinceName, c.DistrictName, c.WardName,
                c.CreatedDate, c.Phone, c.Email, c.Job, c.BranchId, BranchName = c.Branch!.Name,
                HasImage = c.Image != null && c.Image.Length > 0,
            })
            .ToPagedAsync(page, pageSize);

        var items = paged.Items.Select(c => new CustomerListDto(
            c.Id, c.VisiableId, FormatId(format, c.VisiableId), c.Name, c.Sex, c.BirthYear, c.Address,
            c.ProvinceName, c.DistrictName, c.WardName, AddressRepresentation(c.Address, c.WardName, c.DistrictName, c.ProvinceName),
            c.CreatedDate, c.Phone, c.Email, c.Job, c.BranchId, c.BranchName, c.HasImage)).ToList();
        return new PagedResult<CustomerListDto>(items, paged.Total, paged.Page, paged.PageSize);
    }

    // ------------------------------------------------------------------ lookups

    [HttpGet("questions")]
    public async Task<List<YesNoQuestionDto>> Questions() =>
        await _db.YesNoQuestions.OrderBy(q => q.Id).Select(q => new YesNoQuestionDto(q.Id, q.Header)).ToListAsync();

    [HttpGet("jobs")]
    public async Task<List<string>> Jobs()
    {
        var fromDb = await _db.Customers.Where(c => c.Job != null && c.Job != "").Select(c => c.Job!).Distinct().ToListAsync();
        return DefaultJobs.Concat(fromDb).Distinct().OrderBy(j => j).ToList();
    }

    /// <summary>Next VisiableId for the branch (legacy "Tiếp theo" link).</summary>
    [HttpGet("next-id")]
    public async Task<ActionResult<long>> NextId([FromQuery] int? branchId)
    {
        var branch = _user.IsAdministrator ? (branchId ?? _user.BranchId) : _user.BranchId;
        var max = await _db.Customers.Where(c => c.BranchId == branch).Select(c => (long?)c.VisiableId).MaxAsync();
        return (max ?? 0) + 1;
    }

    // ------------------------------------------------------------------ detail

    [HttpGet("{id:long}")]
    public async Task<ActionResult<CustomerDetailDto>> Get(long id)
    {
        var c = await _db.Customers.AsNoTracking().Include(x => x.Branch).FirstOrDefaultAsync(x => x.Id == id)
                ?? throw new NotFoundException("Không tìm thấy khách hàng");
        if (!_user.IsAdministrator && c.BranchId != _user.BranchId) throw new NotFoundException("Không tìm thấy khách hàng");
        var more = await _db.CustomerMoreInformations.AsNoTracking().FirstOrDefaultAsync(m => m.CustomerId == id);
        var times = await _db.Prescriptions.CountAsync(p => p.CustomerId == id && p.HasDone);
        var format = await GetCustomerFormatAsync(_db);
        return ToDetail(c, more, times, format);
    }

    private static CustomerDetailDto ToDetail(Customer c, CustomerMoreInformation? more, int times, string format) => new(
        c.Id, c.VisiableId, FormatId(format, c.VisiableId), c.Name, c.Sex, c.BirthYear, DateTime.Now.Year - c.BirthYear, c.Address,
        c.ProvinceName, c.DistrictName, c.WardName, AddressRepresentation(c.Address, c.WardName, c.DistrictName, c.ProvinceName),
        c.CreatedDate, c.Phone, c.Email, c.Job, c.BranchId, c.Branch?.Name ?? string.Empty, c.Image != null && c.Image.Length > 0,
        times,
        more is null ? null : new CustomerMoreInformationDto(more.WhyYouSeeMe, more.TheReasonYouSeeMe,
            more.SubcribeByPhone, more.PhoneToSubcribe, more.SubcribeByEmail, more.EmailToSubcribe,
            more.SubcribeByMail, more.MailToSubcribe, more.YesQuestions));

    // ------------------------------------------------------------------ photo

    [HttpGet("{id:long}/photo")]
    public async Task<IActionResult> Photo(long id)
    {
        var image = await _db.Customers.Where(c => c.Id == id).Select(c => c.Image).FirstOrDefaultAsync();
        if (image is null || image.Length == 0) return NotFound();
        return File(image, DetectContentType(image));
    }

    private static string DetectContentType(byte[] bytes)
    {
        if (bytes.Length >= 4 && bytes[0] == 0x89 && bytes[1] == 0x50) return "image/png";
        if (bytes.Length >= 3 && bytes[0] == 0x47 && bytes[1] == 0x49) return "image/gif";
        if (bytes.Length >= 2 && bytes[0] == 0x42 && bytes[1] == 0x4D) return "image/bmp";
        return "image/jpeg";
    }

    private static byte[]? DecodeImage(string? base64)
    {
        if (string.IsNullOrWhiteSpace(base64)) return null;
        var comma = base64.IndexOf(',');
        var data = comma >= 0 && base64.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ? base64[(comma + 1)..] : base64;
        try { return Convert.FromBase64String(data); }
        catch (FormatException) { throw new ApiException("Hình ảnh không hợp lệ"); }
    }

    // ------------------------------------------------------------------ create / update / delete

    private async Task ValidateAsync(CustomerInput input, int branchId, long? excludeId)
    {
        if (string.IsNullOrWhiteSpace(input.Name)) throw new ApiException("Vui lòng điền tên khách hàng.");
        if (input.VisiableId is null or < 0) throw new ApiException("Vui lòng điền mã khách hàng, hoặc click \"Tiếp theo\".");
        if (input.BirthYear < 1900 || input.BirthYear > DateTime.Today.Year) throw new ApiException("Năm sinh không hợp lệ.");
        var dup = await _db.Customers.AnyAsync(c => c.BranchId == branchId && c.VisiableId == input.VisiableId && (excludeId == null || c.Id != excludeId));
        if (dup) throw new ApiException("Mã khách hàng bị trùng.");
    }

    private int ResolveBranch(int? requested)
    {
        if (!_user.IsAdministrator) return _user.BranchId;
        var b = requested ?? _user.BranchId;
        if (b == 0) throw new ApiException("Vui lòng chọn chi nhánh cho khách hàng.");
        return b;
    }

    private static CustomerMoreInformation BuildMore(long customerId, CustomerMoreInformationDto? m) => new()
    {
        CustomerId = customerId,
        WhyYouSeeMe = m?.WhyYouSeeMe is >= 1 and <= 4 ? m.WhyYouSeeMe : 4,
        TheReasonYouSeeMe = m?.WhyYouSeeMe == 4 ? m.TheReasonYouSeeMe : null,
        SubcribeByPhone = m?.SubcribeByPhone ?? false,
        PhoneToSubcribe = m?.SubcribeByPhone == true ? m.PhoneToSubcribe : null,
        SubcribeByEmail = m?.SubcribeByEmail ?? false,
        EmailToSubcribe = m?.SubcribeByEmail == true ? m.EmailToSubcribe : null,
        SubcribeByMail = m?.SubcribeByMail ?? false,
        MailToSubcribe = m?.SubcribeByMail == true ? m.MailToSubcribe : null,
        YesQuestions = m?.YesQuestions ?? 0,
    };

    [HttpPost]
    public async Task<ActionResult<CustomerDetailDto>> Create(CustomerInput input)
    {
        var branchId = ResolveBranch(input.BranchId);
        await ValidateAsync(input, branchId, null);
        var c = new Customer
        {
            VisiableId = input.VisiableId!.Value,
            Name = input.Name.Trim(),
            Sex = input.Sex == 1 ? 1 : 0,
            BirthYear = input.BirthYear,
            Address = input.Address,
            Phone = input.Phone,
            Email = input.Email,
            Job = string.IsNullOrWhiteSpace(input.Job) ? null : input.Job,
            CreatedDate = (input.CreatedDate ?? DateTime.Now).Date,
            BranchId = branchId,
            ProvinceName = input.ProvinceName,
            DistrictName = input.DistrictName,
            WardName = input.WardName,
            Image = DecodeImage(input.ImageBase64) ?? Array.Empty<byte>(),
        };
        _db.Customers.Add(c);
        await _db.SaveChangesAsync();
        var more = BuildMore(c.Id, input.MoreInformation);
        _db.CustomerMoreInformations.Add(more);
        await _db.SaveChangesAsync();
        await _db.Entry(c).Reference(x => x.Branch).LoadAsync();
        var format = await GetCustomerFormatAsync(_db);
        return CreatedAtAction(nameof(Get), new { id = c.Id }, ToDetail(c, more, 0, format));
    }

    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, CustomerInput input)
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy khách hàng");
        if (!_user.IsAdministrator && c.BranchId != _user.BranchId) throw new NotFoundException("Không tìm thấy khách hàng");
        await ValidateAsync(input, c.BranchId, id);
        c.VisiableId = input.VisiableId!.Value;
        c.Name = input.Name.Trim();
        c.Sex = input.Sex == 1 ? 1 : 0;
        c.BirthYear = input.BirthYear;
        c.Address = input.Address;
        c.Phone = input.Phone;
        c.Email = input.Email;
        c.Job = string.IsNullOrWhiteSpace(input.Job) ? null : input.Job;
        c.ProvinceName = input.ProvinceName;
        c.DistrictName = input.DistrictName;
        c.WardName = input.WardName;
        if (_user.IsAdministrator && input.BranchId is > 0) c.BranchId = input.BranchId.Value;
        if (input.ImageChanged) c.Image = DecodeImage(input.ImageBase64);

        var more = await _db.CustomerMoreInformations.FirstOrDefaultAsync(m => m.CustomerId == id);
        var fresh = BuildMore(id, input.MoreInformation);
        if (more is null)
        {
            _db.CustomerMoreInformations.Add(fresh);
        }
        else
        {
            more.WhyYouSeeMe = fresh.WhyYouSeeMe; more.TheReasonYouSeeMe = fresh.TheReasonYouSeeMe;
            more.SubcribeByPhone = fresh.SubcribeByPhone; more.PhoneToSubcribe = fresh.PhoneToSubcribe;
            more.SubcribeByEmail = fresh.SubcribeByEmail; more.EmailToSubcribe = fresh.EmailToSubcribe;
            more.SubcribeByMail = fresh.SubcribeByMail; more.MailToSubcribe = fresh.MailToSubcribe;
            more.YesQuestions = fresh.YesQuestions;
        }
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var c = await _db.Customers.FirstOrDefaultAsync(x => x.Id == id) ?? throw new NotFoundException("Không tìm thấy khách hàng");
        if (!_user.IsAdministrator && c.BranchId != _user.BranchId) throw new NotFoundException("Không tìm thấy khách hàng");
        if (await _db.Prescriptions.AnyAsync(p => p.CustomerId == id))
            throw new ApiException("Không thể xóa khách hàng này được, chỉ có thể xóa khách hàng không chứa dữ liệu.");
        var more = await _db.CustomerMoreInformations.FirstOrDefaultAsync(m => m.CustomerId == id);
        if (more is not null) _db.CustomerMoreInformations.Remove(more);
        _db.Customers.Remove(c);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    // ------------------------------------------------------------------ export

    [HttpGet("export")]
    public async Task<IActionResult> Export([FromQuery] string? keyword, [FromQuery] string searchBy = "name", [FromQuery] int? branchId = null,
        [FromQuery] string dateMode = "all", [FromQuery] DateTime? from = null, [FromQuery] DateTime? to = null,
        [FromQuery] string? province = null, [FromQuery] string? district = null, [FromQuery] string? ward = null)
    {
        var rows = await List(keyword, searchBy, branchId, dateMode, from, to, province, district, ward, 1, 500);
        var bytes = ExcelExporter.Build("Khach hang", rows.Items, new List<ExcelExporter.Column<CustomerListDto>>
        {
            new("Chi nhánh", r => r.BranchName),
            new("Khách hàng", r => r.Name),
            new("Mã KH", r => r.FormattedId),
            new("Ngày tạo", r => r.CreatedDate),
            new("Giới tính", r => r.Sex == 1 ? "Nữ" : "Nam"),
            new("Năm sinh", r => r.BirthYear, "0"),
            new("Địa chỉ", r => r.AddressRepresentation),
            new("Điện thoại", r => r.Phone),
            new("Email", r => r.Email),
            new("Nghề nghiệp", r => r.Job),
        }, "Danh sách khách hàng");
        return File(bytes, ExcelExporter.ContentType, "khach-hang.xlsx");
    }
}
