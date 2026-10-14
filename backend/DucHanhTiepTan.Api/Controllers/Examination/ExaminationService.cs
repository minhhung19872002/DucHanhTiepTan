using DucHanhTiepTan.Api.Common;
using DucHanhTiepTan.Api.Data;
using DucHanhTiepTan.Api.Entities;
using DucHanhTiepTan.Api.Security;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Controllers.Examination;

/// <summary>
/// Business logic of the legacy PrescriptionControl / TreatmentControl / ConsultingUserControl /
/// KeThuocPanel / DiscountControl / ImageManager / DoctorNavigatorUserControl.
/// </summary>
public class ExaminationService
{
    private readonly AppDbContext _db;
    private readonly ICurrentUser _user;

    public ExaminationService(AppDbContext db, ICurrentUser user)
    {
        _db = db; _user = user;
    }

    // ------------------------------------------------------------------ helpers
    private async Task<string> CustomerFormatAsync() =>
        await _db.Configurations.Where(c => c.Name == "CustomerFormat").Select(c => c.Value).FirstOrDefaultAsync() ?? "{0:000000}";

    private static string FormatId(string format, long visiableId)
    {
        try { return string.Format(format, visiableId); } catch { return visiableId.ToString(); }
    }

    private async Task<bool> IsSuperUserAsync()
    {
        if (!_user.IsAdministrator) return false;
        var su = await _db.Configurations.Where(c => c.Name == "SuperUser").Select(c => c.Value).FirstOrDefaultAsync();
        return string.IsNullOrEmpty(su) || string.Equals(su, _user.UserName, StringComparison.OrdinalIgnoreCase);
    }

    private static long RemainOf(long previousDebt, int qty, long price, long paid, bool cancelled) =>
        cancelled ? previousDebt - paid : previousDebt + (long)qty * price - paid;

    /// <summary>PrescriptionAdapter.Remain: remain of the treatment with the greatest SubId among finished appointments.</summary>
    private async Task<long> PrescriptionRemainAsync(long prescriptionId, int? excludeAppointmentNo = null)
    {
        var q = _db.ContainedTreatments.Where(t => t.PrescriptionId == prescriptionId && t.Appointment!.HasDone);
        if (excludeAppointmentNo.HasValue) q = q.Where(t => t.AppointmentNo != excludeAppointmentNo.Value);
        var last = await q.OrderByDescending(t => t.SubId).FirstOrDefaultAsync();
        return last is null ? 0 : RemainOf(last.PreviousDebt, last.zQuantity, last.Price, last.Paid, last.IsCancelled);
    }

    private static LaboDto? ToLabo(ContainedLabo? l) => l is null ? null :
        new LaboDto(l.LaboVendorId, l.LaboTypeId, l.Color, l.ReceiverDate, l.DeliverDate, l.Delivered, l.Price);
    private static LaboDto? ToLabo(ContainedConsultingLabo? l) => l is null ? null :
        new LaboDto(l.LaboVendorId, l.LaboTypeId, l.Color, l.ReceiverDate, l.DeliverDate, l.Delivered, l.Price);

    private static TreatmentDto ToDto(ContainedTreatment t) => new(t.SubId, t.AppointmentNo, t.ServiceId, t.OriginalPrice, t.Price, t.CreatedDate, t.Paid,
        t.DoctorName, t.AssistantName, t.Comment, t.zQuantity, t.ToothPositions, t.PreviousDebt, t.IsCancelled,
        ToLabo(t.ContainedLabos.FirstOrDefault()), t.LaboId);

    // ------------------------------------------------------------------ lookups
    public async Task<LookupsDto> GetLookupsAsync(int branchId)
    {
        var services = await _db.Services.OrderBy(s => s.Name).Select(s => new ServiceLookup(s.Id, s.Name, s.Price, s.GroupId, s.ServiceGroup!.Name)).ToListAsync();
        var drugs = await _db.Drugs.OrderBy(d => d.Name).Select(d => new DrugLookup(d.Id, d.Name, d.Unit, d.Price, d.OutPrice)).ToListAsync();
        var equipment = await _db.Equipments.Where(e => e.IsVarious).OrderBy(e => e.Name)
            .Select(e => new EquipmentLookup(e.Id, e.Name, e.Serial, e.Unit, e.Vendor, e.Position, e.Usage, e.Note,
                e.EquimentInBranches.Where(x => branchId == 0 || x.BranchId == branchId).Sum(x => (int?)x.Quantity) ?? 0)).ToListAsync();
        var laboTypes = await _db.LaboTypes.OrderBy(l => l.Name).Select(l => new IdName(l.Id, l.Name)).ToListAsync();
        var laboVendors = await _db.LaboVendors.OrderBy(l => l.Name).Select(l => new IdName(l.Id, l.Name)).ToListAsync();
        var doctors = await _db.Doctors.Where(d => d.Employee!.IsValid).OrderBy(d => d.Employee!.FullName).Select(d => new EmployeeLookup(d.UserName, d.Employee!.FullName)).ToListAsync();
        var assistants = await _db.Assistants.Where(d => d.Employee!.IsValid).OrderBy(d => d.Employee!.FullName).Select(d => new EmployeeLookup(d.UserName, d.Employee!.FullName)).ToListAsync();
        return new LookupsDto(services, drugs, equipment, laboTypes, laboVendors, doctors, assistants);
    }

    // ------------------------------------------------------------------ in-progress list
    public async Task<List<InProgressItem>> GetInProgressAsync(int branchId)
    {
        var format = await CustomerFormatAsync();
        var rows = await _db.Appointments
            .Where(a => !a.HasDone && (branchId == 0 || a.Prescription!.Customer!.BranchId == branchId))
            .OrderByDescending(a => a.CreatedDate)
            .Select(a => new
            {
                a.PrescriptionId, a.No, a.CreatedDate, a.DoctorName,
                DoctorFullName = a.Doctor != null ? a.Doctor.Employee!.FullName : null,
                Customer = a.Prescription!.Customer!,
            }).ToListAsync();
        return rows.Select(r => new InProgressItem(r.PrescriptionId, r.No, r.CreatedDate, r.Customer.Id, r.Customer.Name,
            FormatId(format, r.Customer.VisiableId), r.Customer.BirthYear, r.Customer.Sex == 0 ? "Nam" : "Nữ", r.DoctorName, r.DoctorFullName, r.Customer.BranchId)).ToList();
    }

    // ------------------------------------------------------------------ start / next appointment / delete
    public async Task<StartResult> StartAsync(long customerId)
    {
        var customer = await _db.Customers.FindAsync(customerId) ?? throw new NotFoundException("Không tìm thấy khách hàng");
        if (!_user.IsAdministrator && customer.BranchId != _user.BranchId)
            throw new ApiException("Khách hàng thuộc chi nhánh khác");
        var now = DateTime.Now;
        var prescription = new Prescription { CustomerId = customer.Id, CreatedDate = now, HasDone = false, DiscountValue = 0 };
        _db.Prescriptions.Add(prescription);
        await _db.SaveChangesAsync();
        var appointment = new Appointment { PrescriptionId = prescription.Id, No = 1, HasDone = false, CreatedDate = now };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return new StartResult(prescription.Id, 1);
    }

    public async Task<StartResult> NextAppointmentAsync(long prescriptionId)
    {
        var prescription = await _db.Prescriptions.FindAsync(prescriptionId) ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        var open = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId && !a.HasDone).Select(a => (int?)a.No).FirstOrDefaultAsync();
        if (open.HasValue) return new StartResult(prescriptionId, open.Value); // already in progress: reuse
        var max = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId).MaxAsync(a => (int?)a.No) ?? 0;
        var appointment = new Appointment { PrescriptionId = prescriptionId, No = max + 1, HasDone = false, CreatedDate = DateTime.Now };
        _db.Appointments.Add(appointment);
        await _db.SaveChangesAsync();
        return new StartResult(prescriptionId, appointment.No);
    }

    public async Task DeleteOpenAppointmentAsync(long prescriptionId, int no, bool cancelAppointmentElse)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.PrescriptionId == prescriptionId && a.No == no)
            ?? throw new NotFoundException("Không tìm thấy đợt khám");
        if (appointment.HasDone) throw new ApiException("Phiếu khám này đã xong nên không hủy được.");
        var prescription = await _db.Prescriptions.Include(p => p.Customer).FirstAsync(p => p.Id == prescriptionId);

        await using var tx = await _db.Database.BeginTransactionAsync();
        await RemoveAppointmentContentAsync(prescriptionId, no, prescription.Customer!.BranchId, removeConsulting: no == 1);
        _db.Appointments.Remove(appointment);
        await _db.SaveChangesAsync();

        var lastDone = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId && a.HasDone).OrderByDescending(a => a.No).FirstOrDefaultAsync();
        if (lastDone is null)
        {
            var remaining = await _db.Appointments.AnyAsync(a => a.PrescriptionId == prescriptionId);
            if (!remaining)
            {
                _db.ContainedDiscounts.RemoveRange(_db.ContainedDiscounts.Where(d => d.PrescriptionId == prescriptionId));
                _db.Payments.RemoveRange(_db.Payments.Where(p => p.PrescriptionId == prescriptionId));
                _db.Prescriptions.Remove(prescription);
            }
        }
        else if (cancelAppointmentElse)
        {
            lastDone.AppointmentDate = null;
        }
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    /// <summary>Deletes receipts, used equipment (restoring stock), pictures, treatments(+labo) of an appointment.</summary>
    private async Task RemoveAppointmentContentAsync(long prescriptionId, int no, int branchId, bool removeConsulting)
    {
        _db.ContainedReceipts.RemoveRange(_db.ContainedReceipts.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no));
        _db.ContainedPictures.RemoveRange(_db.ContainedPictures.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no));
        await RemoveUsedEquipmentAsync(prescriptionId, no, branchId);
        var treatments = await _db.ContainedTreatments.Include(t => t.ContainedLabos)
            .Where(t => t.PrescriptionId == prescriptionId && t.AppointmentNo == no).ToListAsync();
        foreach (var t in treatments) _db.ContainedLabos.RemoveRange(t.ContainedLabos);
        _db.ContainedTreatments.RemoveRange(treatments);
        if (removeConsulting)
        {
            var cons = await _db.ContainedConsultings.Include(c => c.ContainedConsultingLabos).Where(c => c.PrescriptionId == prescriptionId).ToListAsync();
            foreach (var c in cons) _db.ContainedConsultingLabos.RemoveRange(c.ContainedConsultingLabos);
            _db.ContainedConsultings.RemoveRange(cons);
        }
    }

    private async Task RemoveUsedEquipmentAsync(long prescriptionId, int no, int branchId)
    {
        var used = await _db.ContainedUsedEquipments.Where(u => u.PrescriptionId == prescriptionId && u.AppointmentNo == no).ToListAsync();
        foreach (var u in used)
        {
            var stock = await _db.EquimentInBranches.FindAsync(branchId, u.EquipmentId);
            if (stock is not null) stock.Quantity += u.Quantity;
        }
        _db.ContainedUsedEquipments.RemoveRange(used);
    }

    // ------------------------------------------------------------------ detail
    public async Task<ExaminationDetail> GetDetailAsync(long prescriptionId, int no)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.PrescriptionId == prescriptionId && a.No == no)
            ?? throw new NotFoundException("Không tìm thấy đợt khám");
        var prescription = await _db.Prescriptions.Include(p => p.Customer).ThenInclude(c => c!.Branch).FirstAsync(p => p.Id == prescriptionId);
        var customer = prescription.Customer!;
        var branch = customer.Branch!;
        var format = await CustomerFormatAsync();
        var maxNo = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId).MaxAsync(a => a.No);

        var all = await _db.ContainedTreatments.Include(t => t.ContainedLabos)
            .Where(t => t.PrescriptionId == prescriptionId).OrderBy(t => t.SubId).ToListAsync();
        var current = all.Where(t => t.AppointmentNo == no).Select(ToDto).ToList();
        var elder = all.Where(t => t.AppointmentNo < no).Select(ToDto).ToList();
        var previousDebt = current.Count > 0 ? current[0].PreviousDebt : await PrescriptionRemainAsync(prescriptionId);

        var consultings = await _db.ContainedConsultings.Include(c => c.ContainedConsultingLabos)
            .Where(c => c.PrescriptionId == prescriptionId).OrderBy(c => c.SubId)
            .ToListAsync();
        var consultingDtos = consultings.Select(c => new ConsultingDto(c.SubId, c.ServiceId, c.OriginalPrice, c.Price, c.CreatedDate, c.Comment, c.zQuantity, c.ToothPositions,
            ToLabo(c.ContainedConsultingLabos.FirstOrDefault()))).ToList();

        var receipts = await _db.ContainedReceipts.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no)
            .Select(r => new ReceiptDto(r.DrugId, r.Amount, r.Instruction, r.PerDay, r.PerTimes)).ToListAsync();
        var usedEq = await _db.ContainedUsedEquipments.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no)
            .Select(r => new UsedEquipmentDto(r.EquipmentId, r.Quantity)).ToListAsync();
        var pictureIds = await _db.ContainedPictures.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no).OrderBy(r => r.Id).Select(r => r.Id).ToListAsync();
        var payments = await _db.Payments.Where(p => p.PrescriptionId == prescriptionId).OrderBy(p => p.CreatedDate).ThenBy(p => p.Id)
            .Select(p => new PaymentDto(p.Id, p.CreatedDate, p.Amount, p.ReceptionistName)).ToListAsync();

        var discountType = prescription.MembershipId != null ? "discount" : prescription.DiscountValue == 1.0 ? "free" : "normal";
        var discounts = await _db.ContainedDiscounts.Where(d => d.PrescriptionId == prescriptionId)
            .Select(d => new DiscountItemDto(d.ServiceId, d.Service!.Name, d.Value)).ToListAsync();

        MembershipInfo? membership = null;
        if (prescription.MembershipId != null)
        {
            membership = await LoadMembershipAsync(prescription.MembershipId, validate: false);
            if (membership is not null && discounts.Count > 0)
                membership = membership with { Discounts = discounts };
        }

        var remain = await PrescriptionRemainAsync(prescriptionId);
        return new ExaminationDetail(
            new CustomerInfo(customer.Id, customer.Name, FormatId(format, customer.VisiableId), customer.Sex, customer.BirthYear, customer.Address, customer.Phone, customer.BranchId),
            new BranchInfo(branch.Id, branch.Name, branch.Address, branch.Tel),
            new PrescriptionInfo(prescription.Id, prescription.CustomerId, prescription.HasDone, prescription.Treatment, prescription.Diagnostic, prescription.Symptoms,
                prescription.CreatedDate, prescription.MembershipId, prescription.DiscountValue, discountType, discounts, remain),
            new AppointmentInfo(appointment.PrescriptionId, appointment.No, appointment.CreatedDate, appointment.HasDone, appointment.AppointmentDate,
                appointment.AppointmentReason, appointment.Advised, appointment.DoctorName, maxNo),
            consultingDtos, current, elder, previousDebt, receipts, usedEq, pictureIds, payments, membership, await IsSuperUserAsync());
    }

    public async Task<PrescriptionSummary> GetPrescriptionSummaryAsync(long prescriptionId)
    {
        var p = await _db.Prescriptions.Include(x => x.Customer).FirstOrDefaultAsync(x => x.Id == prescriptionId) ?? throw new NotFoundException("Không tìm thấy phiếu khám");
        var nos = await _db.Appointments.Where(a => a.PrescriptionId == prescriptionId).OrderByDescending(a => a.No).Select(a => a.No).ToListAsync();
        return new PrescriptionSummary(p.Id, p.CustomerId, p.Customer!.Name, p.CreatedDate, p.HasDone, nos);
    }

    // ------------------------------------------------------------------ membership
    public async Task<MembershipInfo?> LoadMembershipAsync(string id, bool validate)
    {
        var upper = id.Trim().ToUpperInvariant();
        var today = DateTime.Today;
        var branch = _user.IsAdministrator ? 0 : _user.BranchId;
        var q = _db.Memberships.Include(m => m.Group).Where(m => m.Id.ToUpper() == upper);
        if (validate)
            q = q.Where(m => (m.Group!.BranchId == 0 || branch == 0 || m.Group.BranchId == branch)
                && (m.ExpiredDate == null || (m.CreatedDate.Date <= today && today <= m.ExpiredDate)));
        var m = await q.FirstOrDefaultAsync();
        if (m is null) return null;
        var g = m.Group!;
        var discounts = g.IsFixed ? new List<DiscountItemDto>() :
            await _db.ServiceDiscounts.Where(d => d.MemberShipGroupId == g.Id).Select(d => new DiscountItemDto(d.ServiceId, d.Service!.Name, d.Value)).ToListAsync();
        return new MembershipInfo(m.Id, m.CreatedDate, m.ExpiredDate, m.Description, g.Id, g.Name, g.IsFixed, g.FixedValue, discounts);
    }

    // ------------------------------------------------------------------ save
    public async Task SaveAsync(long prescriptionId, int no, SaveRequest req)
    {
        var appointment = await _db.Appointments.FirstOrDefaultAsync(a => a.PrescriptionId == prescriptionId && a.No == no)
            ?? throw new NotFoundException("Không tìm thấy đợt khám");
        var prescription = await _db.Prescriptions.Include(p => p.Customer).FirstAsync(p => p.Id == prescriptionId);
        var customer = prescription.Customer!;
        if (appointment.HasDone && !await IsSuperUserAsync())
            throw new ApiException("Đợt khám đã xong, chỉ quản trị viên mới được chỉnh sửa");

        var status = (req.Status ?? "notyet").ToLowerInvariant();
        if (status is not ("notyet" or "appoint" or "done")) throw new ApiException("Trạng thái không hợp lệ");
        if (status == "appoint" && !req.AppointmentDate.HasValue) throw new ApiException("Vui lòng chọn ngày hẹn khám lại");

        // ---- validation (same messages as legacy) ----
        var receipts = req.Receipts ?? new List<ReceiptInput>();
        var drugIds = await _db.Drugs.Select(d => d.Id).ToListAsync();
        for (var j = 0; j < receipts.Count; j++)
        {
            if (!drugIds.Contains(receipts[j].DrugId)) throw new ApiException("Một số thuốc không có trong CSDL.");
            if (receipts.Skip(j + 1).Any(x => x.DrugId == receipts[j].DrugId)) throw new ApiException("Một số thuốc bị trùng.");
            if (receipts[j].Amount <= 0 || receipts[j].PerDay <= 0 || receipts[j].PerTimes <= 0) throw new ApiException("Các trường số liệu phải lớn hơn 0.");
        }
        var serviceNames = await _db.Services.ToDictionaryAsync(s => s.Id, s => s.Name);
        var treatments = req.Treatments ?? new List<TreatmentDto>();
        var n = 0;
        foreach (var t in treatments)
        {
            n++;
            if (!serviceNames.TryGetValue(t.ServiceId, out var sname)) throw new ApiException("Điều trị thứ " + n + " thiếu tên dịch vụ.");
            var (_, qtyFromTeeth) = ToothPositions.Parse(t.ToothPositions);
            var qty = qtyFromTeeth > 0 ? qtyFromTeeth : t.Quantity;
            if (qty <= 0) throw new ApiException($"Dịch vụ {sname}, có số lượng là {qty}. Số lượng dịch vụ phải lớn hơn 0.");
            if (t.Price < 0) throw new ApiException($"Dịch vụ {sname}, có số giá là {t.Price}. Giá dịch vụ phải lớn hơn hoặc bằng 0.");
            if (string.IsNullOrWhiteSpace(t.DoctorName)) throw new ApiException($"Vui lòng điền bác sĩ cho dịch vụ {sname}");
        }
        var consultings = req.Consultings ?? new List<ConsultingDto>();
        if (no == 1)
        {
            n = 0;
            foreach (var c in consultings)
            {
                n++;
                if (!serviceNames.TryGetValue(c.ServiceId, out var sname)) throw new ApiException("Tư vấn: Điều trị thứ " + n + " thiếu tên dịch vụ.");
                var (_, qtyFromTeeth) = ToothPositions.Parse(c.ToothPositions);
                var qty = qtyFromTeeth > 0 ? qtyFromTeeth : c.Quantity;
                if (qty <= 0) throw new ApiException($"Tư vấn: Dịch vụ {sname}, có số lượng là {qty}. Số lượng dịch vụ phải lớn hơn 0.");
                if (c.Price < 0) throw new ApiException($"Tư vấn: Dịch vụ {sname}, có số giá là {c.Price}. Giá dịch vụ phải lớn hơn hoặc bằng 0.");
            }
        }

        await using var tx = await _db.Database.BeginTransactionAsync();

        var wasDone = appointment.HasDone;
        var oldTreatments = await _db.ContainedTreatments.Include(t => t.ContainedLabos)
            .Where(t => t.PrescriptionId == prescriptionId && t.AppointmentNo == no).OrderBy(t => t.SubId).ToListAsync();
        var maxSubId = await _db.ContainedTreatments.Where(t => t.PrescriptionId == prescriptionId).MaxAsync(t => (int?)t.SubId) ?? 0;
        var previousDebtBase = oldTreatments.Count > 0 ? oldTreatments[0].PreviousDebt : await PrescriptionRemainAsync(prescriptionId, excludeAppointmentNo: no);
        var oldPaidByDate = oldTreatments.GroupBy(t => t.CreatedDate.Date).Select(g => new { Date = g.Key, Amount = g.Sum(t => t.Paid) }).ToList();

        // ---- delete current content (replace-all semantics of the legacy Save) ----
        _db.ContainedReceipts.RemoveRange(_db.ContainedReceipts.Where(r => r.PrescriptionId == prescriptionId && r.AppointmentNo == no));
        foreach (var t in oldTreatments) _db.ContainedLabos.RemoveRange(t.ContainedLabos);
        _db.ContainedTreatments.RemoveRange(oldTreatments);
        if (no == 1)
        {
            var oldCons = await _db.ContainedConsultings.Include(c => c.ContainedConsultingLabos).Where(c => c.PrescriptionId == prescriptionId).ToListAsync();
            foreach (var c in oldCons) _db.ContainedConsultingLabos.RemoveRange(c.ContainedConsultingLabos);
            _db.ContainedConsultings.RemoveRange(oldCons);
        }
        await RemoveUsedEquipmentAsync(prescriptionId, no, customer.BranchId);
        await _db.SaveChangesAsync();

        // ---- discount (only on first appointment, when the appointment is being finished) ----
        if (no == 1 && status != "notyet" && req.Discount is not null)
        {
            _db.ContainedDiscounts.RemoveRange(_db.ContainedDiscounts.Where(d => d.PrescriptionId == prescriptionId));
            var type = (req.Discount.Type ?? "normal").ToLowerInvariant();
            if (type == "free")
            {
                prescription.MembershipId = null; prescription.DiscountValue = 1.0;
            }
            else if (type == "discount" && !string.IsNullOrWhiteSpace(req.Discount.MembershipId))
            {
                var m = await LoadMembershipAsync(req.Discount.MembershipId, validate: true) ?? throw new ApiException("Không tìm thấy mã VIP");
                if (m.IsFixed)
                {
                    prescription.MembershipId = null; prescription.DiscountValue = m.FixedValue;
                }
                else
                {
                    prescription.MembershipId = m.Id; prescription.DiscountValue = 0;
                    foreach (var d in m.Discounts)
                        _db.ContainedDiscounts.Add(new ContainedDiscount { PrescriptionId = prescriptionId, ServiceId = d.ServiceId, Value = d.Value });
                }
            }
            else
            {
                prescription.MembershipId = null; prescription.DiscountValue = 0;
            }
        }

        // ---- prescription / appointment header ----
        prescription.Diagnostic = req.Diagnostic;
        prescription.Symptoms = req.Symptoms;
        prescription.Treatment = req.Treatment;
        prescription.HasDone = status == "done";
        if (no == 1 && req.PrescriptionCreatedDate.HasValue) prescription.CreatedDate = req.PrescriptionCreatedDate.Value;
        appointment.HasDone = status != "notyet";
        appointment.AppointmentDate = status == "appoint" ? req.AppointmentDate : null;
        appointment.Advised = req.Advised;
        appointment.AppointmentReason = appointment.AppointmentDate.HasValue ? req.AppointmentReason : null;
        appointment.CreatedDate = req.AppointmentCreatedDate ?? DateTime.Now;
        appointment.DoctorName = string.IsNullOrWhiteSpace(req.DoctorName) ? null : req.DoctorName;

        // ---- receipts (đơn thuốc) ----
        foreach (var r in receipts)
            _db.ContainedReceipts.Add(new ContainedReceipt { PrescriptionId = prescriptionId, AppointmentNo = no, DrugId = r.DrugId, Amount = r.Amount, Instruction = r.Instruction, PerDay = r.PerDay, PerTimes = r.PerTimes });

        // ---- treatments (điều trị) with debt chain ----
        long sumPaid = 0;
        long lastRemain = 0;
        var chain = previousDebtBase;
        var subId = maxSubId;
        var newTreatments = new List<ContainedTreatment>();
        foreach (var t in treatments)
        {
            var (positions, qtyFromTeeth) = ToothPositions.Parse(t.ToothPositions);
            var qty = qtyFromTeeth > 0 ? qtyFromTeeth : t.Quantity;
            subId++;
            var ct = new ContainedTreatment
            {
                PrescriptionId = prescriptionId, AppointmentNo = no, SubId = subId, ServiceId = t.ServiceId,
                Price = t.Price, OriginalPrice = t.OriginalPrice, CreatedDate = t.CreatedDate == default ? DateTime.Today : t.CreatedDate,
                Paid = t.Paid, DoctorName = t.DoctorName, AssistantName = string.IsNullOrWhiteSpace(t.AssistantName) ? null : t.AssistantName,
                Comment = t.Comment, zQuantity = qty, ToothPositions = positions, PreviousDebt = chain, IsCancelled = t.IsCancelled, LaboId = t.LaboId,
            };
            lastRemain = RemainOf(chain, qty, t.Price, t.Paid, t.IsCancelled);
            chain = lastRemain;
            if (t.Labo is { LaboVendorId: not null, LaboTypeId: not null })
            {
                ct.ContainedLabos.Add(new ContainedLabo
                {
                    PrescriptionId = prescriptionId, AppointmentNo = no, SubId = subId,
                    LaboVendorId = t.Labo.LaboVendorId.Value, LaboTypeId = t.Labo.LaboTypeId.Value, Color = t.Labo.Color,
                    ReceiverDate = t.Labo.ReceiverDate ?? DateTime.Today, DeliverDate = t.Labo.DeliverDate ?? DateTime.Today,
                    Delivered = t.Labo.Delivered, Price = t.Labo.Price,
                });
            }
            newTreatments.Add(ct);
            sumPaid += t.Paid;
        }
        _db.ContainedTreatments.AddRange(newTreatments);

        // ---- overpaid customer when finishing the prescription -> expense "Trả tiền dư" ----
        if (lastRemain < 0 && status == "done")
        {
            var expense = new Expense { AccountantName = _user.UserName, CreatedDate = DateTime.Now };
            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();
            _db.ContainedExpenses.Add(new ContainedExpense
            {
                ExpenseId = expense.Id, Amount = -lastRemain,
                Description = ("Trả tiền dư khách hàng " + customer.Name + " Mã khách hàng: " + customer.Id + ", Ngày: " + DateTime.Now).Length > 150
                    ? ("Trả tiền dư KH " + customer.Name + " (" + customer.Id + ")")[..Math.Min(150, ("Trả tiền dư KH " + customer.Name + " (" + customer.Id + ")").Length)]
                    : "Trả tiền dư khách hàng " + customer.Name + " Mã khách hàng: " + customer.Id + ", Ngày: " + DateTime.Now,
            });
        }

        // ---- consulting (tư vấn) – only editable on the first appointment ----
        if (no == 1)
        {
            var cSub = 0;
            foreach (var c in consultings)
            {
                var (positions, qtyFromTeeth) = ToothPositions.Parse(c.ToothPositions);
                cSub++;
                var cc = new ContainedConsulting
                {
                    PrescriptionId = prescriptionId, SubId = cSub, ServiceId = c.ServiceId, Price = c.Price, OriginalPrice = c.OriginalPrice,
                    CreatedDate = c.CreatedDate == default ? DateTime.Today : c.CreatedDate, Comment = c.Comment,
                    zQuantity = qtyFromTeeth > 0 ? qtyFromTeeth : c.Quantity, ToothPositions = positions,
                };
                if (c.Labo is { LaboVendorId: not null, LaboTypeId: not null })
                {
                    cc.ContainedConsultingLabos.Add(new ContainedConsultingLabo
                    {
                        PrescriptionId = prescriptionId, SubId = cSub, LaboVendorId = c.Labo.LaboVendorId.Value, LaboTypeId = c.Labo.LaboTypeId.Value,
                        Color = c.Labo.Color, ReceiverDate = c.Labo.ReceiverDate ?? DateTime.Today, DeliverDate = c.Labo.DeliverDate ?? DateTime.Today,
                        Delivered = c.Labo.Delivered, Price = c.Labo.Price,
                    });
                }
                _db.ContainedConsultings.Add(cc);
            }
        }

        // ---- used equipment (vật tư sử dụng) – deduct branch stock ----
        foreach (var u in (req.UsedEquipments ?? new List<UsedEquipmentDto>()).Where(u => u.Quantity > 0).GroupBy(u => u.EquipmentId))
        {
            var qty = u.Sum(x => x.Quantity);
            var stock = await _db.EquimentInBranches.FindAsync(customer.BranchId, u.Key);
            if (stock is null)
            {
                stock = new EquimentInBranch { BranchId = customer.BranchId, EquipmentId = u.Key, Quantity = 0 };
                _db.EquimentInBranches.Add(stock);
            }
            stock.Quantity -= qty;
            _db.ContainedUsedEquipments.Add(new ContainedUsedEquipment { PrescriptionId = prescriptionId, AppointmentNo = no, EquipmentId = u.Key, Quantity = qty });
        }

        // ---- payments: undo previously confirmed payments of this appointment, then re-create ----
        if (wasDone)
        {
            foreach (var g in oldPaidByDate)
            {
                var remaining = g.Amount;
                if (remaining == 0) continue;
                var pays = await _db.Payments.Where(p => p.PrescriptionId == prescriptionId && p.CreatedDate.Date >= g.Date).OrderBy(p => p.Id).ToListAsync();
                foreach (var p in pays)
                {
                    if (p.Amount <= remaining) { remaining -= p.Amount; _db.Payments.Remove(p); }
                    else { p.Amount -= remaining; remaining = 0; }
                    if (remaining == 0) break;
                }
                if (remaining > 0)
                {
                    await tx.RollbackAsync();
                    throw new ApiException($"Không thể chỉnh dữ liệu.\nNgày {g.Date:dd/MM/yyyy} đã xác nhận thanh toán là {g.Amount:n0}. Nhưng không tìm thấy đủ số tiền đó trong bảng \"Số tiền đã thanh toán\" (Payments).");
                }
            }
        }
        if (sumPaid != 0 && appointment.HasDone)
        {
            foreach (var g in newTreatments.GroupBy(t => t.CreatedDate.Date))
            {
                var paid = g.Sum(t => t.Paid);
                if (paid == 0) continue;
                _db.Payments.Add(new Payment { PrescriptionId = prescriptionId, Amount = paid, CreatedDate = g.Key, ReceptionistName = _user.UserName });
            }
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    // ------------------------------------------------------------------ cancel a service of a previous appointment
    public async Task CancelServiceAsync(long prescriptionId, int subId)
    {
        var t = await _db.ContainedTreatments.FirstOrDefaultAsync(x => x.PrescriptionId == prescriptionId && x.SubId == subId)
            ?? throw new NotFoundException("Không tìm thấy dịch vụ");
        t.IsCancelled = true;
        // RescanTreatment: rebuild the debt chain of the whole prescription
        var all = await _db.ContainedTreatments.Where(x => x.PrescriptionId == prescriptionId).OrderBy(x => x.SubId).ToListAsync();
        long prev = 0;
        foreach (var item in all)
        {
            item.PreviousDebt = prev;
            prev = RemainOf(item.PreviousDebt, item.zQuantity, item.Price, item.Paid, item.IsCancelled);
        }
        await _db.SaveChangesAsync();
    }

    // ------------------------------------------------------------------ pictures
    public async Task<long> AddPictureAsync(long prescriptionId, int no, string? name, byte[] data)
    {
        if (!await _db.Appointments.AnyAsync(a => a.PrescriptionId == prescriptionId && a.No == no)) throw new NotFoundException("Không tìm thấy đợt khám");
        if (data.Length == 0) throw new ApiException("Tập tin hình ảnh trống");
        var p = new ContainedPicture { PrescriptionId = prescriptionId, AppointmentNo = no, Name = name?.Length > 50 ? name[..50] : name, Image = data };
        _db.ContainedPictures.Add(p);
        await _db.SaveChangesAsync();
        return p.Id;
    }

    public async Task DeletePictureAsync(long id)
    {
        var p = await _db.ContainedPictures.FindAsync(id) ?? throw new NotFoundException("Không tìm thấy hình ảnh");
        _db.ContainedPictures.Remove(p);
        await _db.SaveChangesAsync();
    }

    public async Task<(byte[] Data, string? Name)?> GetPictureAsync(long id)
    {
        var p = await _db.ContainedPictures.FindAsync(id);
        return p is null ? null : (p.Image, p.Name);
    }
}
