using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ISF9ExportService
    {
        string ExportOfficialSf9Pdf(
            SF9ExportRequest request,
            string outputPath);
    }
}
