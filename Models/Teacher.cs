namespace TeachFlex.Models
{
    public class Teacher :
        BaseEntity
    {
        public int SchoolId
        {
            get;
            set;
        }

        public string EmployeeNumber
        {
            get;
            set;
        } = string.Empty;

        public string FirstName
        {
            get;
            set;
        } = string.Empty;

        public string MiddleName
        {
            get;
            set;
        } = string.Empty;

        public string LastName
        {
            get;
            set;
        } = string.Empty;

        public string Suffix
        {
            get;
            set;
        } = string.Empty;

        public string Position
        {
            get;
            set;
        } = "Teacher I";

        public string PagIbigNumber
        {
            get;
            set;
        } = string.Empty;

        public string UmidNumber
        {
            get;
            set;
        } = string.Empty;

        public string SssNumber
        {
            get;
            set;
        } = string.Empty;

        public string PhilHealthNumber
        {
            get;
            set;
        } = string.Empty;

        public string EmailAddress
        {
            get;
            set;
        } = string.Empty;

        public string ContactNumber
        {
            get;
            set;
        } = string.Empty;

        public string ProfileImagePath
        {
            get;
            set;
        } = string.Empty;

        public bool IsActive
        {
            get;
            set;
        } = true;

        public School? School
        {
            get;
            set;
        }

        public string FullName =>
            string.Join(
                " ",
                new[]
                {
                    FirstName,
                    MiddleName,
                    LastName,
                    Suffix
                })
            .Replace(
                "  ",
                " ")
            .Trim();
    }
}