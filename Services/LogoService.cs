using System;
using System.IO;
using Microsoft.Win32;

namespace TeachFlex.Services
{
    public interface ILogoService
    {
        string? SelectAndSaveDepEdLogo();

        string? SelectAndSaveSchoolLogo();
        string? SelectAndSaveTeacherProfileImage();
    }

    public class LogoService :
        ILogoService
    {
        private readonly string
            _logoFolder;

        public LogoService()
        {
            _logoFolder =
                Path.Combine(
                    Environment.GetFolderPath(
                        Environment.SpecialFolder
                            .LocalApplicationData),
                    "TeachFlex",
                    "Assets",
                    "Logos");
        }

        public string? SelectAndSaveDepEdLogo()
        {
            return SelectAndSaveLogo(
                "deped-logo");
        }

        public string? SelectAndSaveSchoolLogo()
        {
            return SelectAndSaveLogo(
                "school-logo");
        }
        public string? SelectAndSaveTeacherProfileImage()
        {
            return SelectAndSaveLogo(
                "teacher-profile");
        }

        private string? SelectAndSaveLogo(
            string destinationName)
        {
            OpenFileDialog dialog =
                new OpenFileDialog
                {
                    Title =
                        "Select Logo Image",

                    Filter =
                        "Image Files|" +
                        "*.png;*.jpg;*.jpeg;*.bmp",

                    CheckFileExists =
                        true,

                    Multiselect =
                        false
                };

            bool? result =
                dialog.ShowDialog();

            if (result != true)
            {
                return null;
            }

            Directory.CreateDirectory(
                _logoFolder);

            string extension =
                Path.GetExtension(
                    dialog.FileName)
                    .ToLowerInvariant();

            string uniqueFileName =
                destinationName +
                "-" +
                Guid.NewGuid()
                    .ToString("N") +
                extension;

            string destinationPath =
                Path.Combine(
                    _logoFolder,
                    uniqueFileName);

            string sourcePath =
                Path.GetFullPath(
                    dialog.FileName);

            string fullDestinationPath =
                Path.GetFullPath(
                    destinationPath);

            File.Copy(
                sourcePath,
                fullDestinationPath,
                false);

            DeleteOldLogoFiles(
                destinationName,
                fullDestinationPath);

            return fullDestinationPath;
        }

        private void DeleteOldLogoFiles(
            string destinationName,
            string currentLogoPath)
        {
            if (!Directory.Exists(
                    _logoFolder))
            {
                return;
            }

            string searchPattern =
                destinationName + "-*.*";

            foreach (string existingFile
                     in Directory.GetFiles(
                         _logoFolder,
                         searchPattern))
            {
                if (existingFile.Equals(
                        currentLogoPath,
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    File.Delete(
                        existingFile);
                }
                catch (IOException)
                {
                    // The previous logo may still be used
                    // by the current WPF image preview.
                }
                catch (UnauthorizedAccessException)
                {
                    // Keep the old file when Windows
                    // does not allow it to be removed.
                }
            }
        }
    }
}