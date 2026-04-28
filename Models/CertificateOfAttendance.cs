using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AUCACertSystem.Models;

[Table("CertificateOfAttendance")]
public class CertificateOfAttendance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    [Display(Name = "Student ID")]
    [Required(ErrorMessage = "Student ID is required")]
    public string? StudentID { get; set; }

    [Display(Name = "Student Name")]
    [Required(ErrorMessage = "Student name is required")]
    public string? StudentName { get; set; }

    [Display(Name = "Date of Birth")]
    [Required(ErrorMessage = "Date of birth is required")]
    public DateTime? BornDate { get; set; }

    [Display(Name = "Studied From")]
    [Required(ErrorMessage = "Start period is required")]
    public string? StudiedFrom { get; set; }

    [Display(Name = "Studied To")]
    public string? StudiedTo { get; set; }

    [Display(Name = "Year / Semester")]
    public string? Year { get; set; }

    [Display(Name = "Faculty")]
    [Required(ErrorMessage = "Faculty is required")]
    public string? Faculty { get; set; }

    [Display(Name = "Major")]
    [Required(ErrorMessage = "Major is required")]
    public string? Major { get; set; }

    [Display(Name = "Academic Year")]
    public string? AcademicYear { get; set; }

    [Display(Name = "Approved By")]
    public string? ApprovedBy { get; set; }

    [Display(Name = "Comment")]
    public string? Comment { get; set; }

    [Display(Name = "Status")]
    public string? Status { get; set; }

    [NotMapped]
    public string FormattedBirthDate =>
        BornDate.HasValue ? BornDate.Value.ToString("MMMM dd, yyyy") : "";

    [NotMapped]
    public string CertificateValidity
    {
        get
        {
            if (string.IsNullOrWhiteSpace(AcademicYear)) return "";
            var open = AcademicYear.IndexOf('(');
            var close = AcademicYear.IndexOf(')');
            return open >= 0 && close > open ? AcademicYear[(open + 1)..close] : AcademicYear;
        }
    }

    [NotMapped]
    public string DisplayStudiedTo =>
        string.IsNullOrWhiteSpace(StudiedTo) ||
        StudiedTo.Equals("date", StringComparison.OrdinalIgnoreCase)
            ? "Date" : StudiedTo;

    [NotMapped]
    public string StudentNameForCert =>
        string.IsNullOrWhiteSpace(StudentName) ? ""
        : StudentName.TrimEnd().EndsWith(",") ? StudentName : $"{StudentName},";

    [NotMapped]
    public string? OriginalStudentID { get; set; }
}
