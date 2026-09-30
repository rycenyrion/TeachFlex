using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class PaceCompetency :
        BaseEntity
    {
        public int SubjectId
        {
            get;
            set;
        }

        public int TermNumber
        {
            get;
            set;
        } = 1;

        public string LearningArea
        {
            get;
            set;
        } = string.Empty;

        public string DomainName
        {
            get;
            set;
        } = string.Empty;

        public string CompetencyCode
        {
            get;
            set;
        } = string.Empty;

        public string Description
        {
            get;
            set;
        } = string.Empty;

        public int DisplayOrder
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        } = true;

        public Subject? Subject
        {
            get;
            set;
        }

        public ICollection<LearnerPaceRating>
            LearnerRatings
        {
            get;
            set;
        } = new List<LearnerPaceRating>();
    }
}