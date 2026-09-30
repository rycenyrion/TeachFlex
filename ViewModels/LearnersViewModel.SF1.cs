using System.Collections.Generic;

namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        private string _motherTongue =
            string.Empty;

        private string _indigenousPeopleEthnicGroup =
            string.Empty;

        private string _religion =
            string.Empty;

        private string _houseStreetSitioPurok =
            string.Empty;

        private string _barangay =
            string.Empty;

        private string _municipalityCity =
            string.Empty;

        private string _province =
            string.Empty;

        private string _fatherName =
            string.Empty;

        private string _motherMaidenName =
            string.Empty;

        private string _guardianName =
            string.Empty;

        private string _guardianRelationship =
            string.Empty;

        private string _learningModality =
            string.Empty;

        private bool _isCctRecipient;

        private string _cctReferenceNumber =
            string.Empty;

        private bool _isBalikAral;

        private string _lastSchoolAttended =
            string.Empty;

        private string _lastSchoolYearAttended =
            string.Empty;

        private bool _isSpecialNeedsEducation;

        private string _specialNeedsDetails =
            string.Empty;

        private bool _isAccelerated;

        private string _acceleratedLevel =
            string.Empty;

        private string _sf1Remarks =
            string.Empty;

        public IReadOnlyList<string>
            LearningModalityOptions
        {
            get;
        } = new[]
        {
            "",
            "Face-to-Face",
            "Modular Distance Learning",
            "Online Distance Learning",
            "Blended Learning",
            "Alternative Delivery Mode",
            "Other"
        };

        public IReadOnlyList<string>
            GuardianRelationshipOptions
        {
            get;
        } = new[]
        {
            "",
            "Grandparent",
            "Sibling",
            "Aunt",
            "Uncle",
            "Relative",
            "Foster Parent",
            "Other"
        };

        public string MotherTongue
        {
            get => _motherTongue;

            set => SetProperty(
                ref _motherTongue,
                value);
        }

        public string IndigenousPeopleEthnicGroup
        {
            get => _indigenousPeopleEthnicGroup;

            set => SetProperty(
                ref _indigenousPeopleEthnicGroup,
                value);
        }

        public string Religion
        {
            get => _religion;

            set => SetProperty(
                ref _religion,
                value);
        }

        public string HouseStreetSitioPurok
        {
            get => _houseStreetSitioPurok;

            set => SetProperty(
                ref _houseStreetSitioPurok,
                value);
        }

        public string Barangay
        {
            get => _barangay;

            set => SetProperty(
                ref _barangay,
                value);
        }

        public string MunicipalityCity
        {
            get => _municipalityCity;

            set => SetProperty(
                ref _municipalityCity,
                value);
        }

        public string Province
        {
            get => _province;

            set => SetProperty(
                ref _province,
                value);
        }

        public string FatherName
        {
            get => _fatherName;

            set => SetProperty(
                ref _fatherName,
                value);
        }

        public string MotherMaidenName
        {
            get => _motherMaidenName;

            set => SetProperty(
                ref _motherMaidenName,
                value);
        }

        public string GuardianName
        {
            get => _guardianName;

            set => SetProperty(
                ref _guardianName,
                value);
        }

        public string GuardianRelationship
        {
            get => _guardianRelationship;

            set => SetProperty(
                ref _guardianRelationship,
                value);
        }

        public string LearningModality
        {
            get => _learningModality;

            set => SetProperty(
                ref _learningModality,
                value);
        }

        public bool IsCctRecipient
        {
            get => _isCctRecipient;

            set => SetProperty(
                ref _isCctRecipient,
                value);
        }

        public string CctReferenceNumber
        {
            get => _cctReferenceNumber;

            set => SetProperty(
                ref _cctReferenceNumber,
                value);
        }

        public bool IsBalikAral
        {
            get => _isBalikAral;

            set => SetProperty(
                ref _isBalikAral,
                value);
        }

        public string LastSchoolAttended
        {
            get => _lastSchoolAttended;

            set => SetProperty(
                ref _lastSchoolAttended,
                value);
        }

        public string LastSchoolYearAttended
        {
            get => _lastSchoolYearAttended;

            set => SetProperty(
                ref _lastSchoolYearAttended,
                value);
        }

        public bool IsSpecialNeedsEducation
        {
            get => _isSpecialNeedsEducation;

            set => SetProperty(
                ref _isSpecialNeedsEducation,
                value);
        }

        public string SpecialNeedsDetails
        {
            get => _specialNeedsDetails;

            set => SetProperty(
                ref _specialNeedsDetails,
                value);
        }

        public bool IsAccelerated
        {
            get => _isAccelerated;

            set => SetProperty(
                ref _isAccelerated,
                value);
        }

        public string AcceleratedLevel
        {
            get => _acceleratedLevel;

            set => SetProperty(
                ref _acceleratedLevel,
                value);
        }

        public string Sf1Remarks
        {
            get => _sf1Remarks;

            set => SetProperty(
                ref _sf1Remarks,
                value);
        }
    }
}