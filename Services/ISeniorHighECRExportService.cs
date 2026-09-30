using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ISeniorHighECRExportService
    {
        string PrepareOfficialSeniorHighECR(
            SeniorHighEcrExportRequest request,
            string outputPath);
    }
}