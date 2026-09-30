using System;
using System.Linq;
using System.Threading.Tasks;
using TeachFlex.Repositories;

namespace TeachFlex.ViewModels
{
    public partial class MainWindowViewModel
    {
        private ISchoolRepository?
            _profileSchoolRepository;

        private ITeacherRepository?
            _profileTeacherRepository;

        private string _teacherDisplayName =
            "Teacher";

        private string _teacherInitials =
            "T";

        private string _teacherPosition =
            "Teacher Profile";

        private string _teacherEmployeeNumber =
            "Not provided";

        private string _teacherSchoolName =
            "School not configured";
        private string _teacherProfileImagePath =
    string.Empty;

        public string TeacherDisplayName
        {
            get => _teacherDisplayName;

            private set => SetProperty(
                ref _teacherDisplayName,
                value);
        }

        public string TeacherInitials
        {
            get => _teacherInitials;

            private set => SetProperty(
                ref _teacherInitials,
                value);
        }

        public string TeacherPosition
        {
            get => _teacherPosition;

            private set => SetProperty(
                ref _teacherPosition,
                value);
        }

        public string TeacherEmployeeNumber
        {
            get => _teacherEmployeeNumber;

            private set => SetProperty(
                ref _teacherEmployeeNumber,
                value);
        }

        public string TeacherSchoolName
        {
            get => _teacherSchoolName;

            private set => SetProperty(
                ref _teacherSchoolName,
                value);
        }
        public string TeacherProfileImagePath
        {
            get => _teacherProfileImagePath;

            private set => SetProperty(
                ref _teacherProfileImagePath,
                value);
        }

        private void ConfigureTeacherProfile(
            ISchoolRepository schoolRepository,
            ITeacherRepository teacherRepository)
        {
            _profileSchoolRepository =
                schoolRepository;

            _profileTeacherRepository =
                teacherRepository;

            _ = RefreshTeacherProfileAsync();
        }

        public async Task RefreshTeacherProfileAsync()
        {
            if (_profileSchoolRepository == null ||
                _profileTeacherRepository == null)
            {
                return;
            }

            try
            {
                var school =
                    await _profileSchoolRepository
                        .GetActiveSchoolAsync();

                if (school == null)
                {
                    ResetTeacherProfile();

                    return;
                }

                TeacherSchoolName =
                    string.IsNullOrWhiteSpace(
                        school.SchoolName)
                        ? "School not configured"
                        : school.SchoolName.Trim();

                var teacher =
                    await _profileTeacherRepository
                        .GetActiveTeacherAsync(
                            school.Id);

                if (teacher == null)
                {
                    ResetTeacherInformation();

                    return;
                }

                string middleInitial =
                    string.IsNullOrWhiteSpace(
                        teacher.MiddleName)
                        ? string.Empty
                        : $"{teacher.MiddleName.Trim()[0]}.";

                TeacherDisplayName =
                    string.Join(
                        " ",
                        new[]
                        {
                            teacher.FirstName?.Trim(),
                            middleInitial,
                            teacher.LastName?.Trim(),
                            teacher.Suffix?.Trim()
                        }
                        .Where(
                            value =>
                                !string.IsNullOrWhiteSpace(
                                    value)));

                TeacherPosition =
                    string.IsNullOrWhiteSpace(
                        teacher.Position)
                        ? "Teacher"
                        : teacher.Position.Trim();

                TeacherEmployeeNumber =
                    string.IsNullOrWhiteSpace(
                        teacher.EmployeeNumber)
                        ? "Not provided"
                        : teacher.EmployeeNumber.Trim();
                TeacherProfileImagePath =
    teacher.ProfileImagePath ?? string.Empty;

                TeacherInitials =
                    CreateTeacherInitials(
                        teacher.FirstName,
                        teacher.LastName);
            }
            catch
            {
                ResetTeacherProfile();
            }
        }

        private void ResetTeacherProfile()
        {
            TeacherSchoolName =
                "School not configured";

            ResetTeacherInformation();
        }

        private void ResetTeacherInformation()
        {
            TeacherDisplayName =
                "Teacher";

            TeacherInitials =
                "T";

            TeacherPosition =
                "Teacher Profile";

            TeacherEmployeeNumber =
                "Not provided";
            TeacherProfileImagePath =
    string.Empty;
        }

        private static string CreateTeacherInitials(
            string? firstName,
            string? lastName)
        {
            string firstInitial =
                string.IsNullOrWhiteSpace(
                    firstName)
                    ? string.Empty
                    : firstName.Trim()[0]
                        .ToString()
                        .ToUpperInvariant();

            string lastInitial =
                string.IsNullOrWhiteSpace(
                    lastName)
                    ? string.Empty
                    : lastName.Trim()[0]
                        .ToString()
                        .ToUpperInvariant();

            string initials =
                $"{firstInitial}{lastInitial}";

            return string.IsNullOrWhiteSpace(
                initials)
                ? "T"
                : initials;
        }
    }
}