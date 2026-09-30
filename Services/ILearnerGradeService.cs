using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public interface ILearnerGradeService
    {
        Task<Dictionary<int, int?>>
            GetTermGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                int termNumber,
                CancellationToken cancellationToken =
                    default);

        Task<Dictionary<int, int?>>
            GetFinalGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                CancellationToken cancellationToken =
                    default);

        Task<Dictionary<int, int?>>
            GetComponentTermGradesAsync(
                SchoolClass schoolClass,
                AcademicYear academicYear,
                Subject subject,
                IReadOnlyList<Learner> learners,
                int termNumber,
                string componentName,
                CancellationToken cancellationToken =
                    default);
    }
}
