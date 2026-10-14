using DucHanhTiepTan.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace DucHanhTiepTan.Api.Data;

public partial class AppDbContext
{
    public DbSet<TreatmentAdapter> TreatmentAdapters => Set<TreatmentAdapter>();
    public DbSet<PrescriptionAdapter> PrescriptionAdapters => Set<PrescriptionAdapter>();
    public DbSet<AppointmentAdapter> AppointmentAdapters => Set<AppointmentAdapter>();
    public DbSet<EquipmentInvoiceView> EquipmentInvoiceViews => Set<EquipmentInvoiceView>();
    public DbSet<ExpenseView> ExpenseViews => Set<ExpenseView>();
    public DbSet<ExportInvoiceView> ExportInvoiceViews => Set<ExportInvoiceView>();
    public DbSet<InvoiceView> InvoiceViews => Set<InvoiceView>();

    private static void OnModelCreatingViews(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TreatmentAdapter>(e => { e.HasNoKey(); e.ToView("TreatmentAdapter"); });
        modelBuilder.Entity<PrescriptionAdapter>(e => { e.HasNoKey(); e.ToView("PrescriptionAdapter"); });
        modelBuilder.Entity<AppointmentAdapter>(e => { e.HasNoKey(); e.ToView("AppointmentAdapter"); });
        modelBuilder.Entity<EquipmentInvoiceView>(e => { e.HasNoKey(); e.ToView("EquipmentInvoiceView"); });
        modelBuilder.Entity<ExpenseView>(e => { e.HasNoKey(); e.ToView("ExpenseView"); });
        modelBuilder.Entity<ExportInvoiceView>(e => { e.HasNoKey(); e.ToView("ExportInvoiceView"); });
        modelBuilder.Entity<InvoiceView>(e => { e.HasNoKey(); e.ToView("InvoiceView"); });

        // All DateTime values are stored as "timestamp without time zone" (legacy data has no timezone info).
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (var property in entityType.GetProperties())
            {
                if (property.ClrType == typeof(DateTime) || property.ClrType == typeof(DateTime?))
                {
                    property.SetColumnType("timestamp without time zone");
                }
            }
        }
    }
}
