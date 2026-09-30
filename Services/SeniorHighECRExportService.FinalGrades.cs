using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SeniorHighECRExportService
    {
        static partial void WriteFinalGrades(
            dynamic workbook,
            SeniorHighEcrExportRequest request)
        {
            workbook.Application
                .CalculateFullRebuild();
        }
    }
}