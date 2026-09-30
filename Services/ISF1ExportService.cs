using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ISF1ExportService
    {
        string PrepareOfficialSF1(
            SF1ExportRequest request,
            string outputPath);

        string ExportOfficialSF1Pdf(
            SF1ExportRequest request,
            string outputPath);
    }
}