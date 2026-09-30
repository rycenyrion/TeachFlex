using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IGradesExportService
    {
        string ExportConsolidatedGrades(
            GradesExportRequest request,
            string outputPath);
    }
}