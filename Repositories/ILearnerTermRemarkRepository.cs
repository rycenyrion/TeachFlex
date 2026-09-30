using System.Threading;
using System.Threading.Tasks;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface ILearnerTermRemarkRepository
    {
        Task<LearnerTermRemark?> GetAsync(
            int schoolClassId,
            int learnerId,
            int termNumber,
            CancellationToken cancellationToken = default);

        Task<LearnerTermRemark> SaveAsync(
            LearnerTermRemark remark,
            CancellationToken cancellationToken = default);

        Task MarkNeedsReviewAsync(
            int schoolClassId,
            int learnerId,
            int termNumber,
            decimal? currentGeneralAverage,
            CancellationToken cancellationToken = default);
    }
}
