using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AUCACertSystem.Data;
using AUCACertSystem.Models;
using AUCACertSystem.Services;

namespace AUCACertSystem.Controllers;

public class CertificatesController : Controller
{
    private readonly AppDbContext _db;
    private readonly PdfGenerator _pdf;
    private readonly ILogger<CertificatesController> _logger;

    public CertificatesController(AppDbContext db, PdfGenerator pdf, ILogger<CertificatesController> logger)
    {
        _db = db;
        _pdf = pdf;
        _logger = logger;
    }

    // GET: /Certificates
    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = _db.Certificates.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(c =>
                (c.StudentName != null && c.StudentName.Contains(search)) ||
                (c.StudentID != null && c.StudentID.Contains(search)));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(c => c.Status == status);

        var list = await query.OrderBy(c => c.StudentName).ToListAsync();

        ViewBag.Search = search;
        ViewBag.Status = status;
        ViewBag.Total = await _db.Certificates.CountAsync();
        ViewBag.ActiveCount = await _db.Certificates.CountAsync(c => c.Status == "Active");

        return View(list);
    }

    // GET: /Certificates/Create
    public IActionResult Create()
    {
        var model = new CertificateOfAttendance
        {
            StudiedFrom = "January 2026",
            StudiedTo = "Date",
            AcademicYear = "2025-2026 (September 2025 - August 2026)",
            ApprovedBy = "Eng. Nsengiyumva Juvenal",
            Status = "Active"
        };
        return View(model);
    }

    // POST: /Certificates/Create
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CertificateOfAttendance model)
    {
        if (!ModelState.IsValid) return View(model);

        Normalize(model);

        if (string.IsNullOrWhiteSpace(model.StudentID))
        {
            ModelState.AddModelError(nameof(model.StudentID), "Student ID is required.");
            return View(model);
        }

        if (await _db.Certificates.AnyAsync(c => c.StudentID == model.StudentID))
        {
            ModelState.AddModelError(nameof(model.StudentID), "A certificate with this Student ID already exists.");
            return View(model);
        }

        _db.Certificates.Add(model);
        await _db.SaveChangesAsync();

        TempData["Success"] = $"Certificate for {model.StudentName} created successfully!";
        return RedirectToAction(nameof(Details), new { studentId = model.StudentID });
    }

    // GET: /Certificates/Details/{studentId}
    public async Task<IActionResult> Details(string studentId)
    {
        var cert = await _db.Certificates.FindAsync(studentId);
        if (cert == null) return NotFound();
        return View(cert);
    }

    // GET: /Certificates/Edit/{studentId}
    public async Task<IActionResult> Edit(string studentId)
    {
        var cert = await _db.Certificates.FindAsync(studentId);
        if (cert == null) return NotFound();
        cert.OriginalStudentID = cert.StudentID;
        return View(cert);
    }

    // POST: /Certificates/Edit/{studentId}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string studentId, CertificateOfAttendance model)
    {
        if (!ModelState.IsValid) return View(model);

        Normalize(model);

        var originalId = model.OriginalStudentID?.Trim() ?? studentId.Trim();
        var newId = model.StudentID?.Trim();

        if (string.IsNullOrWhiteSpace(originalId) || string.IsNullOrWhiteSpace(newId))
            return BadRequest();

        var existing = await _db.Certificates.FindAsync(originalId);
        if (existing == null) return NotFound();

        if (!string.Equals(originalId, newId, StringComparison.OrdinalIgnoreCase))
        {
            if (await _db.Certificates.AnyAsync(c => c.StudentID == newId))
            {
                ModelState.AddModelError(nameof(model.StudentID), "A certificate with this Student ID already exists.");
                return View(model);
            }

            _db.Certificates.Remove(existing);
            await _db.SaveChangesAsync();

            model.StudentID = newId;
            _db.Certificates.Add(model);
        }
        else
        {
            existing.StudentName = model.StudentName;
            existing.BornDate = model.BornDate;
            existing.StudiedFrom = model.StudiedFrom;
            existing.StudiedTo = model.StudiedTo;
            existing.Year = model.Year;
            existing.Faculty = model.Faculty;
            existing.Major = model.Major;
            existing.AcademicYear = model.AcademicYear;
            existing.ApprovedBy = model.ApprovedBy;
            existing.Comment = model.Comment;
            existing.Status = model.Status;
        }

        await _db.SaveChangesAsync();
        TempData["Success"] = "Certificate updated successfully!";
        return RedirectToAction(nameof(Details), new { studentId = newId });
    }

    // POST: /Certificates/Delete/{studentId}
    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(string studentId)
    {
        var cert = await _db.Certificates.FindAsync(studentId);
        if (cert != null)
        {
            _db.Certificates.Remove(cert);
            await _db.SaveChangesAsync();
            TempData["Success"] = "Certificate deleted.";
        }
        return RedirectToAction(nameof(Index));
    }

    // GET: /Certificates/DownloadPdf/{studentId}
    public async Task<IActionResult> DownloadPdf(string studentId)
    {
        var cert = await _db.Certificates.FindAsync(studentId);
        if (cert == null) return NotFound();

        try
        {
            var bytes = _pdf.Generate(cert);
            return File(bytes, "application/pdf", $"Certificate_{cert.StudentID}_{DateTime.Now:yyyyMMdd}.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PDF generation failed for {StudentId}", studentId);
            TempData["Error"] = "PDF generation failed. Please try again.";
            return RedirectToAction(nameof(Details), new { studentId });
        }
    }

    // GET: /Certificates/PreviewPdf/{studentId}
    public async Task<IActionResult> PreviewPdf(string studentId)
    {
        var cert = await _db.Certificates.FindAsync(studentId);
        if (cert == null) return NotFound();
        var bytes = _pdf.Generate(cert);
        return File(bytes, "application/pdf");
    }

    private static void Normalize(CertificateOfAttendance m)
    {
        m.StudentName = m.StudentName?.Trim();
        m.StudentID = m.StudentID?.Trim();
        m.StudiedFrom = m.StudiedFrom?.Trim();
        m.StudiedTo = m.StudiedTo?.Trim();
        m.Year = m.Year?.Trim();
        m.Faculty = m.Faculty?.Trim();
        m.Major = m.Major?.Trim();
        m.AcademicYear = m.AcademicYear?.Trim();
        m.ApprovedBy = m.ApprovedBy?.Trim();
        m.Comment = m.Comment?.Trim();
        m.Status = m.Status?.Trim();

        if (m.BornDate.HasValue)
            m.BornDate = DateTime.SpecifyKind(m.BornDate.Value.Date, DateTimeKind.Utc);
    }
}
