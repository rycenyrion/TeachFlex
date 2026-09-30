using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IKindergartenProgressReportPdfService
    {
        string ExportToPdf(
            KindergartenProgressReportRequest request,
            string outputPath);
    }
}
