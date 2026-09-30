using System;
using System.IO;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IECRExportService
    {
        string PrepareOfficialECR(
            ECRExportRequest request,
            string outputPath);
    }

    public partial class ECRExportService :
        IECRExportService
    {
        public string PrepareOfficialECR(
            ECRExportRequest request,
            string outputPath)
        {
            ArgumentNullException.ThrowIfNull(
                request);

            if (string.IsNullOrWhiteSpace(
                    outputPath))
            {
                throw new ArgumentException(
                    "An output file path is required.",
                    nameof(outputPath));
            }

            string templateFileName =
                GetTemplateFileName(
                    request);

            string templatePath =
                Path.Combine(
                    AppContext.BaseDirectory,
                    "Templates",
                    "ECR",
                    templateFileName);

            if (!File.Exists(
                    templatePath))
            {
                throw new FileNotFoundException(
                    $"The official ECR template " +
                    $"'{templateFileName}' was not found.",
                    templatePath);
            }

            string fullOutputPath =
                Path.GetFullPath(
                    outputPath);

            string? outputFolder =
                Path.GetDirectoryName(
                    fullOutputPath);

            if (!string.IsNullOrWhiteSpace(
                    outputFolder))
            {
                Directory.CreateDirectory(
                    outputFolder);
            }

            if (File.Exists(
                    fullOutputPath))
            {
                File.Delete(
                    fullOutputPath);
            }

            File.Copy(
                templatePath,
                fullOutputPath);

            try
            {
                PopulateOfficialWorkbook(
                    request,
                    fullOutputPath);
            }
            catch
            {
                try
                {
                    if (File.Exists(
                            fullOutputPath))
                    {
                        File.Delete(
                            fullOutputPath);
                    }
                }
                catch
                {
                    // Preserve the original export error.
                }

                throw;
            }

            return fullOutputPath;
        }

        private static string GetTemplateFileName(
            ECRExportRequest request)
        {
            int gradeNumber =
                GetGradeNumber(
                    request.GradeLevel);

            if (gradeNumber == 1)
            {
                return "ECR_Grade1.xlsx";
            }

            if (request.AssessmentType ==
                SubjectAssessmentType
                    .GmrcValuesEducation)
            {
                return "ECR_GMRC_Values.xlsx";
            }

            if (request.AssessmentType ==
                SubjectAssessmentType
                    .MapehComponents)
            {
                return "ECR_MAPEH.xlsx";
            }

            if (request.AssessmentType ==
                SubjectAssessmentType
                    .EppTleComponents)
            {
                return request.UsesComponentRecords
                    ? "ECR_EPP_TLE_PerComponent.xlsx"
                    : "ECR_EPP_TLE.xlsx";
            }

            return "ECR_Regular_Grades2To10.xlsx";
        }

        private static int GetGradeNumber(
            string gradeLevel)
        {
            if (string.IsNullOrWhiteSpace(
                    gradeLevel))
            {
                return 0;
            }

            string normalizedGradeLevel =
                gradeLevel
                    .Trim()
                    .ToLowerInvariant()
                    .Replace(
                        "grade",
                        string.Empty)
                    .Trim();

            return int.TryParse(
                normalizedGradeLevel,
                out int gradeNumber)
                    ? gradeNumber
                    : 0;
        }
    }
}