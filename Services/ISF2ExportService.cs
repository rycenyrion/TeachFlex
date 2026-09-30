using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ISF2ExportService
    {
        string PrepareOfficialSF2(
            SF2ExportRequest request,
            string outputPath);

        string ExportOfficialSF2Pdf(
            SF2ExportRequest request,
            string outputPath);
    }
}