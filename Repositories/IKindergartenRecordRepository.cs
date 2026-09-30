using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TeachFlex.Models;

namespace TeachFlex.Repositories
{
    public interface
        IKindergartenRecordRepository
    {
        Task<IReadOnlyList<
            KindergartenCompetency>>
                GetCompetenciesAsync(
                    CancellationToken cancellationToken =
                        default);

        Task SaveCompetenciesAsync(
            IReadOnlyList<
                KindergartenCompetency>
                    competencies,
            CancellationToken cancellationToken =
                default);

        Task<IReadOnlyList<
            KindergartenLearnerRating>>
                GetRatingsAsync(
                    int schoolClassId,
                    int termNumber,
                    CancellationToken cancellationToken =
                        default);

        Task SaveRatingsAsync(
            IReadOnlyList<
                KindergartenLearnerRating>
                    ratings,
            CancellationToken cancellationToken =
                default);

        Task<KindergartenTermRemark?>
            GetTermRemarkAsync(
                int schoolClassId,
                int learnerId,
                int termNumber,
                CancellationToken cancellationToken =
                    default);
        Task<IReadOnlyList<
    KindergartenTermRemark>>
        GetTermRemarksAsync(
            int schoolClassId,
            CancellationToken cancellationToken =
                default);

        Task SaveTermRemarkAsync(
            KindergartenTermRemark remark,
            CancellationToken cancellationToken =
                default);
    }
}