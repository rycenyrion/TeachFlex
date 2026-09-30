using System;
using System.Collections.Generic;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public class AutomatedRemarksService :
        IAutomatedRemarksService
    {
        public AutomatedRemarkLevel GetPerformanceLevel(
            decimal generalAverage)
        {
            decimal normalizedAverage =
                Math.Clamp(
                    generalAverage,
                    0m,
                    100m);

            if (normalizedAverage >= 90m)
            {
                return AutomatedRemarkLevel.Advancing;
            }

            if (normalizedAverage >= 80m)
            {
                return AutomatedRemarkLevel.Benchmarking;
            }

            if (normalizedAverage >= 75m)
            {
                return AutomatedRemarkLevel.Connecting;
            }

            if (normalizedAverage >= 65m)
            {
                return AutomatedRemarkLevel.Developing;
            }

            return AutomatedRemarkLevel.Emerging;
        }

        public IReadOnlyList<string> GetRemarks(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel)
        {
            return AutomatedRemarksCatalog.GetRemarks(
                gradeLevel,
                performanceLevel);
        }

        public string GenerateInitialRemark(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel,
            int learnerId,
            int termNumber)
        {
            return SelectRemark(
                gradeLevel,
                performanceLevel,
                learnerId,
                termNumber,
                0);
        }

        public string GenerateAnotherRemark(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel,
            int learnerId,
            int termNumber,
            int variationNumber)
        {
            return SelectRemark(
                gradeLevel,
                performanceLevel,
                learnerId,
                termNumber,
                Math.Max(1, variationNumber));
        }

        private static string SelectRemark(
            int gradeLevel,
            AutomatedRemarkLevel performanceLevel,
            int learnerId,
            int termNumber,
            int variationNumber)
        {
            IReadOnlyList<string> remarks =
                AutomatedRemarksCatalog.GetRemarks(
                    gradeLevel,
                    performanceLevel);

            if (remarks.Count == 0)
            {
                return string.Empty;
            }

            int seed =
                unchecked(
                    (gradeLevel * 397) ^
                    ((int)performanceLevel * 101) ^
                    (learnerId * 31) ^
                    (termNumber * 17));

            int index =
                (int)(
                    (uint)(seed + variationNumber) %
                    (uint)remarks.Count);

            return remarks[index];
        }
    }
}
