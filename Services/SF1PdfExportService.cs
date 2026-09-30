using System;
using System.IO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1PdfExportService :
        ISF1PdfExportService
    {
        public string ExportOfficialSF1(
            SF1ExportRequest request,
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

            IDocument document =
                CreateSf1Document(
                    request);

            document.GeneratePdf(
                fullOutputPath);

            return fullOutputPath;
        }
    }
}