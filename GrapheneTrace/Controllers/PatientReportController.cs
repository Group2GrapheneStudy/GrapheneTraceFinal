using GrapheneTrace.Data;
using GrapheneTrace.Helpers;
using GrapheneTrace.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;
using PdfSharpCore.Utils;

namespace GrapheneTrace.Controllers
{
    [RoleAuthorize("Patient")]
    public class PatientReportController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ITrendService _trends;
        private readonly IHeatmapService _heatmaps;

        public PatientReportController(AppDbContext context, ITrendService trends, IHeatmapService heatmaps)
        {
            _context = context;
            _trends = trends;
            _heatmaps = heatmaps;
        }

        public async Task<IActionResult> Download()
        {
            var userId = HttpContext.Session.GetInt32(SessionKeys.UserId);
            var patient = await _context.Patients.FirstAsync(p => p.UserId == userId);

            var heatmap = await _heatmaps.GetLatestHeatmapAsync(patient.PatientId);
            var trend = await _trends.GetPatientTrendAsync(patient.PatientId);

            // Create PDF
            var doc = new Document();
            var section = doc.AddSection();

            section.AddParagraph("Patient Pressure Report").Format.Font.Size = 18;

            section.AddParagraph($"Generated: {DateTime.Now}");

            // Trend Table
            section.AddParagraph("\nTrend Data:");

            var table = section.AddTable();
            table.Borders.Width = 0.5;
            table.AddColumn("3cm"); table.AddColumn("3cm");
            table.AddColumn("3cm");

            var header = table.AddRow();
            header.Cells[0].AddParagraph("Time");
            header.Cells[1].AddParagraph("Peak");
            header.Cells[2].AddParagraph("Area %");

            foreach (var p in trend)
            {
                var row = table.AddRow();
                row.Cells[0].AddParagraph(p.Timestamp.ToLocalTime().ToString());
                row.Cells[1].AddParagraph(p.PeakPressure.ToString());
                row.Cells[2].AddParagraph(p.ContactArea.ToString("0.0"));
            }

            // Render PDF
            var renderer = new PdfDocumentRenderer();
            renderer.Document = doc;
            renderer.RenderDocument();

            using var stream = new MemoryStream();
            renderer.PdfDocument.Save(stream, false);
            return File(stream.ToArray(), "application/pdf", "PatientReport.pdf");
        }
    }
}
