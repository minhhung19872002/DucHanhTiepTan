using System.ComponentModel.DataAnnotations;

namespace DucHanhTiepTan.Api.Entities;

public class CustomerMoreInformation
{
    public int WhyYouSeeMe { get; set; }
    [MaxLength(150)]
    public string? TheReasonYouSeeMe { get; set; }
    public bool SubcribeByPhone { get; set; }
    public bool SubcribeByEmail { get; set; }
    public bool SubcribeByMail { get; set; }
    [MaxLength(50)]
    public string? PhoneToSubcribe { get; set; }
    [MaxLength(50)]
    public string? EmailToSubcribe { get; set; }
    [MaxLength(150)]
    public string? MailToSubcribe { get; set; }
    public long CustomerId { get; set; }
    public int YesQuestions { get; set; }

}
