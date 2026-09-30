using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface IDashboardSummaryService
    {
        Task<DashboardSummary> LoadAsync(
            CancellationToken cancellationToken =
                default);
    }

    public class DashboardSummaryService :
        IDashboardSummaryService
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public DashboardSummaryService(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<DashboardSummary> LoadAsync(
            CancellationToken cancellationToken =
                default)
        {
            DashboardSummary summary =
                new DashboardSummary();

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            School? school =
                await database.Schools
                    .AsNoTracking()
                    .Where(
                        record =>
                            record.IsActive)
                    .OrderBy(
                        record =>
                            record.Id)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (school == null)
            {
                return summary;
            }

            summary.SchoolName =
                school.SchoolName;

            summary.SchoolId =
                $"School ID: {school.SchoolId}";

            summary.SchoolLogoPath =
                school.SchoolLogoPath;

            Teacher? teacher =
                await database.Teachers
                    .AsNoTracking()
                    .Where(
                        record =>
                            record.SchoolId ==
                                school.Id &&
                            record.IsActive)
                    .OrderBy(
                        record =>
                            record.Id)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (teacher != null)
            {
                summary.TeacherName =
                    teacher.FullName;
            }

            AcademicYear? academicYear =
                await database.AcademicYears
                    .AsNoTracking()
                    .Where(
                        record =>
                            record.SchoolId ==
                                school.Id &&
                            record.IsCurrent)
                    .OrderByDescending(
                        record =>
                            record.StartYear)
                    .FirstOrDefaultAsync(
                        cancellationToken);

            if (academicYear == null)
            {
                return summary;
            }

            summary.SchoolYear =
                "School Year: " +
                academicYear.DisplayName;

            List<SchoolClass> availableClasses =
                await database.Classes
                    .AsNoTracking()
                    .Where(
                        schoolClass =>
                            schoolClass.SchoolId ==
                                school.Id &&
                            schoolClass.AcademicYearId ==
                                academicYear.Id &&
                            schoolClass.IsActive &&
                            (teacher != null && schoolClass.AdviserId == teacher.Id ||
                             database.ClassSubjects.Any(assignment =>
                                 assignment.SchoolClassId == schoolClass.Id &&
                                 assignment.IsActive)))
                    .OrderByDescending(
                        schoolClass =>
                            teacher != null &&
                            schoolClass.AdviserId ==
                                teacher.Id)
                    .ThenBy(
                        schoolClass =>
                            schoolClass.GradeLevel)
                    .ThenBy(
                        schoolClass =>
                            schoolClass.SectionName)
                    .ToListAsync(
                        cancellationToken);

            // Advisers see their own classes. Without an advisory class,
            // use active classes with assigned subjects as the fallback.
            if (availableClasses.Any(schoolClass =>
                    teacher != null && schoolClass.AdviserId == teacher.Id))
            {
                availableClasses = availableClasses
                    .Where(schoolClass => schoolClass.AdviserId == teacher!.Id)
                    .ToList();
            }

            if (availableClasses.Count == 0)
            {
                return summary;
            }

            List<int> availableClassIds =
                availableClasses
                    .Select(
                        schoolClass =>
                            schoolClass.Id)
                    .ToList();

            var learnerCounts =
                await database.Learners
                    .AsNoTracking()
                    .Where(
                        learner =>
                            availableClassIds.Contains(
                                learner.SchoolClassId) &&
                            learner.Status !=
                                "Archived")
                    .GroupBy(learner => learner.SchoolClassId)
                    .Select(group => new
                    {
                        SchoolClassId = group.Key,
                        Total = group.Count(),
                        Male = group.Count(learner => learner.Sex == "Male"),
                        Female = group.Count(learner => learner.Sex == "Female")
                    })
                    .ToListAsync(
                        cancellationToken);

            var countsByClass = learnerCounts.ToDictionary(
                count => count.SchoolClassId);

            var todayAttendance = await (
                from day in database.AttendanceDays.AsNoTracking()
                join attendance in database.LearnerAttendances.AsNoTracking()
                    on day.Id equals attendance.AttendanceDayId
                where availableClassIds.Contains(day.SchoolClassId) &&
                      day.AttendanceDate == System.DateTime.Today &&
                      (day.DayStatus == "Regular Class Day" ||
                       day.DayStatus == "Make-up Class")
                group attendance by day.SchoolClassId into groupRows
                select new
                {
                    SchoolClassId = groupRows.Key,
                    Present = groupRows.Count(row => row.AttendanceStatus == "Present"),
                    Absent = groupRows.Count(row => row.AttendanceStatus == "Absent")
                }).ToListAsync(cancellationToken);
            var attendanceByClass = todayAttendance.ToDictionary(
                record => record.SchoolClassId);

            foreach (SchoolClass schoolClass
                     in availableClasses)
            {
                countsByClass.TryGetValue(schoolClass.Id, out var count);
                attendanceByClass.TryGetValue(schoolClass.Id, out var attendance);

                DashboardClassOption option =
                    new DashboardClassOption
                    {
                        SchoolClassId =
                            schoolClass.Id,

                        GradeLevel =
                            schoolClass.GradeLevel,

                        SectionName =
                            schoolClass.SectionName,

                        IsAdvisoryClass =
                            teacher != null &&
                            schoolClass.AdviserId ==
                                teacher.Id,

                        NumberOfLearners =
                            count?.Total ?? 0,

                        MaleLearners =
                            count?.Male ?? 0,

                        FemaleLearners =
                            count?.Female ?? 0,

                        PresentToday = attendance?.Present,
                        AbsentToday = attendance?.Absent
                    };

                summary.AvailableClasses.Add(
                    option);
            }

            DashboardClassOption defaultClass =
                summary.AvailableClasses
                    .FirstOrDefault(
                        option =>
                            option.IsAdvisoryClass)
                ?? summary.AvailableClasses[0];

            summary.DefaultSchoolClassId =
                defaultClass.SchoolClassId;

            summary.GradeAndSection =
                defaultClass.DisplayName;

            summary.NumberOfLearners =
                defaultClass.NumberOfLearners;

            summary.MaleLearners =
                defaultClass.MaleLearners;

            summary.FemaleLearners =
                defaultClass.FemaleLearners;

            return summary;
        }
    }
}
