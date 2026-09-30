using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class KindergartenProgressReportRequest
    {
        public SF9ExportRequest BaseRequest
        {
            get;
            set;
        } = null!;

        public IReadOnlyList<KindergartenCompetency> Competencies
        {
            get;
            set;
        } = new List<KindergartenCompetency>();

        public IReadOnlyList<KindergartenLearnerRating> Ratings
        {
            get;
            set;
        } = new List<KindergartenLearnerRating>();

        public IReadOnlyList<KindergartenTermRemark> Remarks
        {
            get;
            set;
        } = new List<KindergartenTermRemark>();
    }
}
