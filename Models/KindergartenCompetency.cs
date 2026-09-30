namespace TeachFlex.Models
{
    public class KindergartenCompetency :
        BaseEntity
    {
        public string CompetencyCode
        {
            get;
            set;
        } = string.Empty;

        public string DevelopmentArea
        {
            get;
            set;
        } = string.Empty;

        public string SubDomain
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

        public string DisplayName =>
            string.IsNullOrWhiteSpace(
                CompetencyCode)
                ? Description
                : $"{CompetencyCode}. {Description}";
    }
}