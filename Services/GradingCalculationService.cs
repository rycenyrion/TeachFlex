using System;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IGradingCalculationService
    {
        decimal CalculatePercentageScore(
            decimal totalScore,
            decimal highestPossibleScore);

        decimal CalculateInitialGrade(
            decimal writtenWorkPercentage,
            decimal performanceTaskPercentage,
            decimal examinationPercentage,
            GradingPolicy policy);

        int CalculateNumericalGrade(
            decimal initialGrade,
            GradingSystemType gradingSystem);

        string GetDescriptor(
            int numericalGrade);
    }

    public class GradingCalculationService :
        IGradingCalculationService
    {
        public decimal CalculatePercentageScore(
            decimal totalScore,
            decimal highestPossibleScore)
        {
            if (highestPossibleScore <= 0)
            {
                return 0;
            }

            decimal percentageScore =
                totalScore /
                highestPossibleScore *
                100m;

            return Math.Round(
                percentageScore,
                2,
                MidpointRounding.AwayFromZero);
        }

        public decimal CalculateInitialGrade(
            decimal writtenWorkPercentage,
            decimal performanceTaskPercentage,
            decimal examinationPercentage,
            GradingPolicy policy)
        {
            ArgumentNullException.ThrowIfNull(
                policy);

            if (!policy.UsesNumericalGrades)
            {
                return 0;
            }

            decimal writtenWorkWeightedScore =
                writtenWorkPercentage *
                (decimal)policy.WrittenWorkWeight /
                100m;

            decimal performanceTaskWeightedScore =
                performanceTaskPercentage *
                (decimal)policy
                    .PerformanceTaskWeight /
                100m;

            decimal examinationWeightedScore =
                examinationPercentage *
                (decimal)policy.ExaminationWeight /
                100m;

            decimal initialGrade =
                writtenWorkWeightedScore +
                performanceTaskWeightedScore +
                examinationWeightedScore;

            return Math.Round(
                initialGrade,
                2,
                MidpointRounding.AwayFromZero);
        }

        public int CalculateNumericalGrade(
            decimal initialGrade,
            GradingSystemType gradingSystem)
        {
            decimal safeInitialGrade =
                Math.Clamp(
                    initialGrade,
                    0m,
                    100m);

            if (gradingSystem ==
                GradingSystemType
                    .NumericalZeroBased)
            {
                return (int)Math.Round(
                    safeInitialGrade,
                    0,
                    MidpointRounding.AwayFromZero);
            }

            if (gradingSystem ==
                GradingSystemType
                    .NumericalAdjusted)
            {
                return GetAdjustedTransmutedGrade(
                    safeInitialGrade);
            }

            return 0;
        }

        public string GetDescriptor(
            int numericalGrade)
        {
            return numericalGrade switch
            {
                >= 90 =>
                    "Advancing",

                >= 80 =>
                    "Benchmarking",

                >= 75 =>
                    "Connecting",

                >= 65 =>
                    "Developing",

                _ =>
                    "Emerging"
            };
        }

        private static int
            GetAdjustedTransmutedGrade(
                decimal initialGrade)
        {
            if (initialGrade >= 99.50m)
            {
                return 100;
            }

            if (initialGrade >= 98.32m)
            {
                return 99;
            }

            if (initialGrade >= 97.14m)
            {
                return 98;
            }

            if (initialGrade >= 95.96m)
            {
                return 97;
            }

            if (initialGrade >= 94.78m)
            {
                return 96;
            }

            if (initialGrade >= 93.60m)
            {
                return 95;
            }

            if (initialGrade >= 92.42m)
            {
                return 94;
            }

            if (initialGrade >= 91.24m)
            {
                return 93;
            }

            if (initialGrade >= 90.06m)
            {
                return 92;
            }

            if (initialGrade >= 88.88m)
            {
                return 91;
            }

            if (initialGrade >= 87.70m)
            {
                return 90;
            }

            if (initialGrade >= 86.52m)
            {
                return 89;
            }

            if (initialGrade >= 85.34m)
            {
                return 88;
            }

            if (initialGrade >= 84.16m)
            {
                return 87;
            }

            if (initialGrade >= 82.98m)
            {
                return 86;
            }

            if (initialGrade >= 81.80m)
            {
                return 85;
            }

            if (initialGrade >= 80.62m)
            {
                return 84;
            }

            if (initialGrade >= 79.44m)
            {
                return 83;
            }

            if (initialGrade >= 78.26m)
            {
                return 82;
            }

            if (initialGrade >= 77.08m)
            {
                return 81;
            }

            if (initialGrade >= 75.90m)
            {
                return 80;
            }

            if (initialGrade >= 74.72m)
            {
                return 79;
            }

            if (initialGrade >= 73.54m)
            {
                return 78;
            }

            if (initialGrade >= 72.36m)
            {
                return 77;
            }

            if (initialGrade >= 71.18m)
            {
                return 76;
            }

            if (initialGrade >= 70.00m)
            {
                return 75;
            }

            if (initialGrade >= 65.34m)
            {
                return 74;
            }

            if (initialGrade >= 60.67m)
            {
                return 73;
            }

            if (initialGrade >= 56.01m)
            {
                return 72;
            }

            if (initialGrade >= 51.34m)
            {
                return 71;
            }

            if (initialGrade >= 46.67m)
            {
                return 70;
            }

            if (initialGrade >= 42.01m)
            {
                return 69;
            }

            if (initialGrade >= 37.34m)
            {
                return 68;
            }

            if (initialGrade >= 32.68m)
            {
                return 67;
            }

            if (initialGrade >= 28.01m)
            {
                return 66;
            }

            if (initialGrade >= 23.35m)
            {
                return 65;
            }

            if (initialGrade >= 18.68m)
            {
                return 64;
            }

            if (initialGrade >= 14.01m)
            {
                return 63;
            }

            if (initialGrade >= 9.35m)
            {
                return 62;
            }

            if (initialGrade >= 4.68m)
            {
                return 61;
            }

            return 60;
        }
    }
}