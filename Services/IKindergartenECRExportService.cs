using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IKindergartenECRExportService
    {
        string PrepareOfficialKindergartenECR(
            KindergartenEcrExportRequest request,
            string outputPath);
    }
}