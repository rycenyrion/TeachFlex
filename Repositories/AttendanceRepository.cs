using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TeachFlex.Data;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface IAttendanceRepository
    {
        Task<AttendanceDay?>
            GetDayAsync(
                int schoolClassId,
                DateTime attendanceDate,
                CancellationToken cancellationToken =
                    default);

        Task<AttendanceDay>
            SaveDayAsync(
                AttendanceDay attendanceDay,
                CancellationToken cancellationToken =
                    default);

        Task<IReadOnlyList<LearnerAttendance>>
            GetLearnerAttendancesAsync(
                int attendanceDayId,
                CancellationToken cancellationToken =
                    default);

        Task SaveLearnerAttendancesAsync(
            int attendanceDayId,
            IEnumerable<LearnerAttendance>
                learnerAttendances,
            CancellationToken cancellationToken =
                default);

        Task<IReadOnlyList<AttendanceDay>>
            GetMonthDaysAsync(
                int schoolClassId,
                int year,
                int month,
                CancellationToken cancellationToken =
                    default);

        Task<IReadOnlyList<LearnerAttendance>>
            GetMonthAttendancesAsync(
                int schoolClassId,
                int year,
                int month,
                CancellationToken cancellationToken =
                    default);
    }

    public class AttendanceRepository :
        IAttendanceRepository
    {
        private readonly IDbContextFactory<
            TeachFlexDbContext>
                _dbContextFactory;

        public AttendanceRepository(
            IDbContextFactory<
                TeachFlexDbContext>
                    dbContextFactory)
        {
            _dbContextFactory =
                dbContextFactory;
        }

        public async Task<AttendanceDay?>
            GetDayAsync(
                int schoolClassId,
                DateTime attendanceDate,
                CancellationToken cancellationToken =
                    default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime selectedDate =
                attendanceDate.Date;

            return await database.AttendanceDays
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    attendanceDay =>
                        attendanceDay.SchoolClassId ==
                            schoolClassId &&
                        attendanceDay.AttendanceDate ==
                            selectedDate,
                    cancellationToken);
        }

        public async Task<AttendanceDay>
            SaveDayAsync(
                AttendanceDay attendanceDay,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                attendanceDay);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime selectedDate =
                attendanceDay.AttendanceDate.Date;

            AttendanceDay? existingDay =
                await database.AttendanceDays
                    .FirstOrDefaultAsync(
                        savedDay =>
                            savedDay.SchoolClassId ==
                                attendanceDay.SchoolClassId &&
                            savedDay.AttendanceDate ==
                                selectedDate,
                        cancellationToken);

            if (existingDay == null)
            {
                attendanceDay.AttendanceDate =
                    selectedDate;

                attendanceDay.CreatedAtUtc =
                    DateTime.UtcNow;

                attendanceDay.UpdatedAtUtc =
                    DateTime.UtcNow;

                await database.AttendanceDays.AddAsync(
                    attendanceDay,
                    cancellationToken);

                await database.SaveChangesAsync(
                    cancellationToken);

                return attendanceDay;
            }

            existingDay.DayStatus =
                attendanceDay.DayStatus;

            existingDay.Description =
                attendanceDay.Description.Trim();

            existingDay.UpdatedAtUtc =
                DateTime.UtcNow;

            await database.SaveChangesAsync(
                cancellationToken);

            return existingDay;
        }

        public async Task<
            IReadOnlyList<LearnerAttendance>>
                GetLearnerAttendancesAsync(
                    int attendanceDayId,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            return await database.LearnerAttendances
                .AsNoTracking()
                .Include(
                    learnerAttendance =>
                        learnerAttendance.Learner)
                .Where(
                    learnerAttendance =>
                        learnerAttendance.AttendanceDayId ==
                            attendanceDayId)
                .OrderBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.Sex ==
                            "Male"
                            ? 0
                            : 1)
                .ThenBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.LastName)
                .ThenBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.FirstName)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task
            SaveLearnerAttendancesAsync(
                int attendanceDayId,
                IEnumerable<LearnerAttendance>
                    learnerAttendances,
                CancellationToken cancellationToken =
                    default)
        {
            ArgumentNullException.ThrowIfNull(
                learnerAttendances);

            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            List<LearnerAttendance> entries =
                learnerAttendances.ToList();

            List<LearnerAttendance> existingEntries =
                await database.LearnerAttendances
                    .Where(
                        learnerAttendance =>
                            learnerAttendance
                                .AttendanceDayId ==
                                    attendanceDayId)
                    .ToListAsync(
                        cancellationToken);

            foreach (LearnerAttendance entry
                     in entries)
            {
                LearnerAttendance? existingEntry =
                    existingEntries.FirstOrDefault(
                        savedEntry =>
                            savedEntry.LearnerId ==
                                entry.LearnerId);

                if (existingEntry == null)
                {
                    entry.AttendanceDayId =
                        attendanceDayId;

                    entry.CreatedAtUtc =
                        DateTime.UtcNow;

                    entry.UpdatedAtUtc =
                        DateTime.UtcNow;

                    await database
                        .LearnerAttendances
                        .AddAsync(
                            entry,
                            cancellationToken);

                    continue;
                }

                existingEntry.AttendanceStatus =
                    entry.AttendanceStatus;

                existingEntry.Remarks =
                    entry.Remarks.Trim();

                existingEntry.UpdatedAtUtc =
                    DateTime.UtcNow;
            }

            await database.SaveChangesAsync(
                cancellationToken);
        }

        public async Task<
            IReadOnlyList<AttendanceDay>>
                GetMonthDaysAsync(
                    int schoolClassId,
                    int year,
                    int month,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime firstDate =
                new DateTime(
                    year,
                    month,
                    1);

            DateTime nextMonth =
                firstDate.AddMonths(
                    1);

            return await database.AttendanceDays
                .AsNoTracking()
                .Where(
                    attendanceDay =>
                        attendanceDay.SchoolClassId ==
                            schoolClassId &&
                        attendanceDay.AttendanceDate >=
                            firstDate &&
                        attendanceDay.AttendanceDate <
                            nextMonth)
                .OrderBy(
                    attendanceDay =>
                        attendanceDay.AttendanceDate)
                .ToListAsync(
                    cancellationToken);
        }

        public async Task<
            IReadOnlyList<LearnerAttendance>>
                GetMonthAttendancesAsync(
                    int schoolClassId,
                    int year,
                    int month,
                    CancellationToken cancellationToken =
                        default)
        {
            await using TeachFlexDbContext database =
                await _dbContextFactory
                    .CreateDbContextAsync(
                        cancellationToken);

            DateTime firstDate =
                new DateTime(
                    year,
                    month,
                    1);

            DateTime nextMonth =
                firstDate.AddMonths(
                    1);

            return await database.LearnerAttendances
                .AsNoTracking()
                .Include(
                    learnerAttendance =>
                        learnerAttendance.AttendanceDay)
                .Include(
                    learnerAttendance =>
                        learnerAttendance.Learner)
                .Where(
                    learnerAttendance =>
                        learnerAttendance.AttendanceDay !=
                            null &&
                        learnerAttendance
                            .AttendanceDay
                            .SchoolClassId ==
                                schoolClassId &&
                        learnerAttendance
                            .AttendanceDay
                            .AttendanceDate >=
                                firstDate &&
                        learnerAttendance
                            .AttendanceDay
                            .AttendanceDate <
                                nextMonth)
                .OrderBy(
                    learnerAttendance =>
                        learnerAttendance
                            .AttendanceDay!
                            .AttendanceDate)
                .ThenBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.Sex ==
                            "Male"
                            ? 0
                            : 1)
                .ThenBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.LastName)
                .ThenBy(
                    learnerAttendance =>
                        learnerAttendance.Learner!.FirstName)
                .ToListAsync(
                    cancellationToken);
        }
    }
}