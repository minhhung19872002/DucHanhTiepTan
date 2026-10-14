using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class Doctor
{
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    public Employee? User { get; set; }
    public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    public ICollection<ContainedTreatment> ContainedTreatments { get; set; } = new List<ContainedTreatment>();
    public ICollection<DoctorSalary> DoctorSalaries { get; set; } = new List<DoctorSalary>();
}
