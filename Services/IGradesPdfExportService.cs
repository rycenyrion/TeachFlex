using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IGradesPdfExportService
    {
        string ExportConsolidatedGrades(
            GradesExportRequest request,
            string outputPath);
    }
}