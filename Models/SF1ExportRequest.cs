using System;
using System.Collections.Generic;

namespace TeachFlex.Models
{
    public class SF1ExportRequest
    {
        public string SchoolId
        {
            get;
            set;
        } = string.Empty;

        public string SchoolName
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

        public string SchoolYear
        {
            get;
            set;
        } = string.Empty;

        public int SchoolYearStartYear
        {
            get;
            set;
        }

        public string GradeLevel
        {
            get;
            set;
        } = string.Empty;

        public string SectionName
        {
            get;
            set;
        } = string.Empty;

        public string AdviserName
        {
            get;
            set;
        } = string.Empty;

        public string SchoolHeadName
        {
            get;
            set;
        } = string.Empty;

        public DateTime DatePrepared
        {
            get;
            set;
        } = DateTime.Today;

        public List<SF1LearnerRow> Learners
        {
            get;
            set;
        } = new();
    }

    public class SF1LearnerRow
    {
        public int LearnerId
        {
            get;
            set;
        }

        public string Lrn
        {
            get;
            set;
        } = string.Empty;

        public string LearnerName
        {
            get;
            set;
        } = string.Empty;

        public string Sex
        {
            get;
            set;
        } = string.Empty;

        public DateTime? BirthDate
        {
            get;
            set;
        }

        public int? AgeAsOfFirstFriday
        {
            get;
            set;
        }

        public string MotherTongue
        {
            get;
            set;
        } = string.Empty;

        public string IndigenousPeopleEthnicGroup
        {
            get;
            set;
        } = string.Empty;

        public string Religion
        {
            get;
            set;
        } = string.Empty;

        public string HouseStreetSitioPurok
        {
            get;
            set;
        } = string.Empty;

        public string Barangay
        {
            get;
            set;
        } = string.Empty;

        public string MunicipalityCity
        {
            get;
            set;
        } = string.Empty;

        public string Province
        {
            get;
            set;
        } = string.Empty;

        public string FatherName
        {
            get;
            set;
        } = string.Empty;

        public string MotherMaidenName
        {
            get;
            set;
        } = string.Empty;

        public string GuardianName
        {
            get;
            set;
        } = string.Empty;

        public string GuardianRelationship
        {
            get;
            set;
        } = string.Empty;

        public string ContactNumber
        {
            get;
            set;
        } = string.Empty;

        public string LearningModality
        {
            get;
            set;
        } = string.Empty;

        public DateTime? EnrollmentDate
        {
            get;
            set;
        }

        public string EnrollmentType
        {
            get;
            set;
        } = string.Empty;

        public string PreviousSchoolName
        {
            get;
            set;
        } = string.Empty;

        public DateTime? ExitDate
        {
            get;
            set;
        }

        public string ExitReason
        {
            get;
            set;
        } = string.Empty;

        public string NextSchoolName
        {
            get;
            set;
        } = string.Empty;

        public bool IsCctRecipient
        {
            get;
            set;
        }

        public string CctReferenceNumber
        {
            get;
            set;
        } = string.Empty;

        public bool IsBalikAral
        {
            get;
            set;
        }

        public string LastSchoolAttended
        {
            get;
            set;
        } = string.Empty;

        public string LastSchoolYearAttended
        {
            get;
            set;
        } = string.Empty;

        public bool IsSpecialNeedsEducation
        {
            get;
            set;
        }

        public string SpecialNeedsDetails
        {
            get;
            set;
        } = string.Empty;

        public bool IsAccelerated
        {
            get;
            set;
        }

        public string AcceleratedLevel
        {
            get;
            set;
        } = string.Empty;

        public string AdditionalRemarks
        {
            get;
            set;
        } = string.Empty;
    }
}