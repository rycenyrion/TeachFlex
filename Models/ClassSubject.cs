namespace TeachFlex.Models
{
    public class ClassSubject :
        BaseEntity
    {
        public int SchoolClassId
        {
            get;
            set;
        }

        public int SubjectId
        {
            get;
            set;
        }

        public bool IsActive
        {
            get;
            set;
        } = true;

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }

        public Subject? Subject
        {
            get;
            set;
        }
    }
}