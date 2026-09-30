using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IAutomatedRemarksService
    {
        AutomatedRemarkLevel GetPerformanceLevel(
            decimal generalAverage);

        IReadOnlyList<string> GetRemarks(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel);

        string GenerateInitialRemark(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel,
            int learnerId,
            int termNumber);

        string GenerateAnotherRemark(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel,
            int learnerId,
            int termNumber,
            int variationNumber);
    }
}
