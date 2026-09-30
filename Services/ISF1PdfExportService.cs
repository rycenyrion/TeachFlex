using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ISF1PdfExportService
    {
        string ExportOfficialSF1(
            SF1ExportRequest request,
            string outputPath);
    }
}