using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IGradeOnePaceExportService
    {
        string PrepareOfficialGradeOnePace(
            GradeOnePaceExportRequest request,
            string outputPath);
    }
}