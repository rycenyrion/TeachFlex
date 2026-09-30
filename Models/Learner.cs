using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace TeachFlex.Models
{
    public class Learner :
        BaseEntity
    {
        public int SchoolClassId
        {
            get;
            set;
        }

        public string Lrn
        {
            get;
            set;
        } = string.Empty;

        public string LastName
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

        public string Suffix
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

        // Existing fields retained for compatibility.

        public string Address
        {
            get;
            set;
        } = string.Empty;

        public string ParentGuardianName
        {
            get;
            set;
        } = string.Empty;

        public string ParentGuardianContactNumber
        {
            get;
            set;
        } = string.Empty;

        // SF1 learner profile.

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

        // SF1 address components.

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

        // SF1 parent and guardian information.

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

        public string LearningModality
        {
            get;
            set;
        } = string.Empty;

        // Additional SF1 indicators.

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

        public string Sf1Remarks
        {
            get;
            set;
        } = string.Empty;

        // Enrollment and movement information used by SF1 and SF2.

        public string Status
        {
            get;
            set;
        } = "Active";

        public DateTime? EnrollmentDate
        {
            get;
            set;
        }

        public string EnrollmentType
        {
            get;
            set;
        } = "Regular";

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

        public SchoolClass? SchoolClass
        {
            get;
            set;
        }

        [NotMapped]
        public string FullName
        {
            get
            {
                string middleInitial =
                    string.IsNullOrWhiteSpace(
                        MiddleName)
                        ? string.Empty
                        : $"{MiddleName.Trim()[0]}.";

                return string.Join(
                        " ",
                        new[]
                        {
                            FirstName,
                            middleInitial,
                            LastName,
                            Suffix
                        })
                    .Replace(
                        "  ",
                        " ")
                    .Trim();
            }
        }

        [NotMapped]
        public string OfficialName
        {
            get
            {
                string lastNameWithSuffix =
                    string.Join(
                        " ",
                        new[]
                        {
                            LastName?.Trim(),
                            Suffix?.Trim()
                        }
                        .Where(
                            value =>
                                !string.IsNullOrWhiteSpace(
                                    value)));

                string middleInitial =
                    string.IsNullOrWhiteSpace(
                        MiddleName)
                        ? string.Empty
                        : $"{MiddleName.Trim()[0]}.";

                string givenNames =
                    string.Join(
                        " ",
                        new[]
                        {
                            FirstName?.Trim(),
                            middleInitial
                        }
                        .Where(
                            value =>
                                !string.IsNullOrWhiteSpace(
                                    value)));

                if (string.IsNullOrWhiteSpace(
                        lastNameWithSuffix))
                {
                    return givenNames
                        .ToUpperInvariant();
                }

                return $"{lastNameWithSuffix}, " +
                       $"{givenNames}"
                           .Trim()
                           .ToUpperInvariant();
            }
        }

        [NotMapped]
        public int? Age
        {
            get
            {
                if (!BirthDate.HasValue)
                {
                    return null;
                }

                DateTime today =
                    DateTime.Today;

                int age =
                    today.Year -
                    BirthDate.Value.Year;

                if (BirthDate.Value.Date >
                    today.AddYears(
                        -age))
                {
                    age--;
                }

                return age;
            }
        }
    }
}