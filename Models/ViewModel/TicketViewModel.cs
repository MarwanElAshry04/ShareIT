using System.ComponentModel.DataAnnotations;

namespace ShareIT.Models.ViewModel
{
    public class TicketViewModel
    {
        public int TempEmailId { get; set; }

        [Required(ErrorMessage = "نوع الشكوى مطلوب")]
        [Display(Name = "نوع الشكوى")]
        public int CategoryId { get; set; }

        [Display(Name = "النوع الفرعي")]
        public int? ComSubTypeId { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; }

        [Display(Name = "وصف الشكوى")]
        public string? Desc { get; set; }

        [Display(Name = "الشركة")]
        public string? ConcerningCompany { get; set; }

        [Display(Name = "الإدارة")]
        public string? ConcerningDepartment { get; set; }

        [Display(Name = "أخرى")]
        public string? ConcerningOther { get; set; }

        [Display(Name = "الشخص المسؤول")]
        public string? ConcerningPerson { get; set; }

        [Display(Name = "اللغة")]
        public string? LangFlag { get; set; } = "ar";

        // بيانات Reporter
        [Display(Name = "اسم المبلغ")]
        public string? ReporterName { get; set; }

        [Display(Name = "البريد الإلكتروني")]
        [EmailAddress]
        public string? ReporterEmail { get; set; }

        [Display(Name = "رقم الجوال")]
        public string? ReporterMobile { get; set; }

        [Display(Name = "القسم")]
        public int? DepartmentId { get; set; }

        [Display(Name = "كود الموظف")]
        public string? EmpCode { get; set; }
    }
}
