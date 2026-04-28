using AUCACertSystem.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AUCACertSystem.Services;

public class PdfGenerator
{
    private readonly byte[] _letterhead;
    private readonly byte[] _signature;
    private readonly byte[] _qrCode;

    private const string Font = "Times New Roman";
    private const float Body = 12f;
    private const float Spacing = 1.5f;

    public PdfGenerator(IWebHostEnvironment env)
    {
        var imgs = Path.Combine(env.WebRootPath, "images");
        _letterhead = File.ReadAllBytes(Path.Combine(imgs, "letterhead.jpg"));
        _signature = File.ReadAllBytes(Path.Combine(imgs, "signature.png"));
        _qrCode = File.ReadAllBytes(Path.Combine(imgs, "qr.png"));
    }

    public byte[] Generate(CertificateOfAttendance cert)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.MarginTop(35);
                page.MarginBottom(30);
                page.MarginHorizontal(55);

                page.DefaultTextStyle(x => x
                    .FontFamily(Font)
                    .FontSize(Body)
                    .FontColor("#000000")
                    .LineHeight(Spacing));

                page.Content().Column(col =>
                {
                    // ── LETTERHEAD BANNER (logo + university name) ──
                    col.Item().Image(_letterhead).FitWidth();

                    // ── Contact info lines ──
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text("Mobile Phone : (+250)724 796 996 / 724 474 805/ 788 473 035")
                        .FontSize(9);

                    col.Item().AlignCenter().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(9));
                        text.Span("Email: ");
                        text.Span("registrar@auca.ac.rw").FontColor("#0000EE").Underline();
                        text.Span(",  ||  ");
                        text.Span("juvenal.nsengiyumva@auca.ac.rw").FontColor("#0000EE").Underline();
                    });

                    // ── Horizontal separator ──
                    col.Item().PaddingTop(6)
                        .LineHorizontal(1.5f).LineColor("#000000");

                    // ── Date (left-aligned) ──
                    col.Item().PaddingTop(18)
                        .Text($"Kigali, {DateTime.Now:MMMM d, yyyy}")
                        .FontSize(Body);

                    // ── Title (centered, bold) ──
                    col.Item().PaddingTop(24).AlignCenter()
                        .Text("CERTIFICATE OF GOOD STANDING")
                        .Bold().FontSize(14);

                    // ── Intro paragraph ──
                    col.Item().PaddingTop(28).Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(Body).LineHeight(Spacing));
                        text.Span("I, the undersigned, ");
                        text.Span(cert.ApprovedBy ?? "Eng. Nsengiyumva Juvenal");
                        text.Span(", Director for Admissions and Academic Records of the ");
                        text.Span("Adventist University of Central Africa");
                        text.Span(", hereby certify that:");
                    });

                    // ── Student name (bold, underlined) ──
                    col.Item().PaddingTop(14)
                        .Text(cert.StudentNameForCert)
                        .Bold().Underline().FontSize(Body).LineHeight(Spacing);

                    // ── Born on ──
                    col.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(Body).LineHeight(Spacing));
                        text.Span("Born on ");
                        text.Span(cert.FormattedBirthDate).Bold();
                    });

                    // ── Registration ──
                    col.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(Body).LineHeight(Spacing));
                        text.Span("has been a regular student of this University, registered under ID No. ");
                        text.Span(cert.StudentID ?? "").Bold();
                        text.Span(",");
                    });

                    // ── Study period ──
                    col.Item().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(Body).LineHeight(Spacing));
                        text.Span("From ");
                        text.Span(cert.StudiedFrom ?? "").Bold();
                        text.Span(" to ");
                        text.Span(cert.DisplayStudiedTo).Bold();
                        text.Span(".");
                    });

                    // ── Details (inline, not table) ──
                    DetailLine(col, "Year: ", cert.Year);
                    DetailLine(col, "Faculty: ", cert.Faculty);
                    DetailLine(col, "Major: ", cert.Major);
                    DetailLine(col, "Academic year: ", cert.AcademicYear);
                    DetailLine(col, "Validity: ", cert.CertificateValidity);

                    // ── Optional comment ──
                    if (!string.IsNullOrWhiteSpace(cert.Comment))
                    {
                        col.Item().PaddingTop(10)
                            .Text(cert.Comment)
                            .Italic().FontSize(11).LineHeight(Spacing);
                    }

                    // ── Purpose statement ──
                    col.Item().PaddingTop(16)
                        .Text("This certificate is issued for any legal or administrative purpose it may serve")
                        .FontSize(Body).LineHeight(Spacing);

                    // ── Footer row: Signature (left) + QR code (right) ──
                    col.Item().PaddingTop(24).Row(row =>
                    {
                        // Signature block (left side)
                        row.RelativeItem().Column(sig =>
                        {
                            sig.Item().Width(160).Height(80).Image(_signature);
                            sig.Item().Width(280).LineHorizontal(0.8f).LineColor("#000000");
                            sig.Item().PaddingTop(4)
                                .Text(cert.ApprovedBy ?? "Eng. Nsengiyumva Juvenal")
                                .FontSize(Body).LineHeight(Spacing);
                            sig.Item()
                                .Text("Director for Admissions and Academic Records")
                                .Bold().FontSize(Body).LineHeight(Spacing);
                            sig.Item()
                                .Text("Adventist University of Central Africa")
                                .FontSize(Body).LineHeight(Spacing);
                        });

                        // QR code (right side)
                        row.ConstantItem(120).AlignRight().AlignBottom().Column(qr =>
                        {
                            qr.Item().AlignCenter().Width(100).Height(100).Image(_qrCode);
                            qr.Item().PaddingTop(4).AlignCenter()
                                .Text("Scan to verify my\nValidity")
                                .FontSize(9).LineHeight(1.2f);
                        });
                    });
                });
            });
        });

        return doc.GeneratePdf();
    }

    private static void DetailLine(ColumnDescriptor col, string label, string? value)
    {
        col.Item().Text(text =>
        {
            text.DefaultTextStyle(x => x.FontSize(Body).LineHeight(Spacing));
            text.Span(label);
            text.Span(value ?? "").Bold();
        });
    }
}
