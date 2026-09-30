namespace TeachFlex.Models
{
    public class School :
        BaseEntity
    {
        public string SchoolName
        {
            get;
            set;
        } = string.Empty;

        public string SchoolId
        {
            get;
            set;
        } = string.Empty;

        public string Region
        {
            get;
            set;
        } = string.Empty;

        public string Division
        {
            get;
            set;
        } = string.Empty;

        public string District
        {
            get;
            set;
        } = string.Empty;

        public string SchoolAddress
        {
            get;
            set;
        } = string.Empty;

        public string SchoolHead
        {
            get;
            set;
        } = string.Empty;

        public string SchoolClassification
        {
            get;
            set;
        } = "Public School";

        public string DepEdLogoPath
        {
            get;
            set;
        } = string.Empty;

        public string SchoolLogoPath
        {
            get;
            set;
        } = string.Empty;

        public bool IsActive
        {
            get;
            set;
        } = true;
    }
}