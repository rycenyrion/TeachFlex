using System.Linq;
using TeachFlex.Models;
namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        private void SaveSf1Information(
            Learner learner)
        {
            learner.MotherTongue =
                MotherTongue.Trim();

            learner.IndigenousPeopleEthnicGroup =
                IndigenousPeopleEthnicGroup.Trim();

            learner.Religion =
                Religion.Trim();

            learner.HouseStreetSitioPurok =
                HouseStreetSitioPurok.Trim();

            learner.Barangay =
                Barangay.Trim();

            learner.MunicipalityCity =
                MunicipalityCity.Trim();

            learner.Province =
                Province.Trim();

            learner.FatherName =
                FatherName.Trim();

            learner.MotherMaidenName =
                MotherMaidenName.Trim();

            learner.GuardianName =
                GuardianName.Trim();

            learner.GuardianRelationship =
                GuardianRelationship.Trim();

            learner.LearningModality =
                LearningModality.Trim();

            learner.IsCctRecipient =
                IsCctRecipient;

            learner.CctReferenceNumber =
                CctReferenceNumber.Trim();

            learner.IsBalikAral =
                IsBalikAral;

            learner.LastSchoolAttended =
                LastSchoolAttended.Trim();

            learner.LastSchoolYearAttended =
                LastSchoolYearAttended.Trim();

            learner.IsSpecialNeedsEducation =
                IsSpecialNeedsEducation;

            learner.SpecialNeedsDetails =
                SpecialNeedsDetails.Trim();

            learner.IsAccelerated =
                IsAccelerated;

            learner.AcceleratedLevel =
                AcceleratedLevel.Trim();

            learner.Sf1Remarks =
                Sf1Remarks.Trim();

            // Maintain the existing fields used by other modules.
            // Existing values are preserved while the new SF1
            // information has not yet been entered.

            string completeAddress =
                CreateCompleteAddress();

            if (!string.IsNullOrWhiteSpace(
                    completeAddress))
            {
                learner.Address =
                    completeAddress;
            }

            string primaryParentOrGuardian =
                !string.IsNullOrWhiteSpace(
                    GuardianName)
                    ? GuardianName.Trim()
                    : GetPrimaryParentName();

            if (!string.IsNullOrWhiteSpace(
                    primaryParentOrGuardian))
            {
                learner.ParentGuardianName =
                    primaryParentOrGuardian;
            }
        }

        private void LoadSf1Information(
            Learner learner)
        {
            MotherTongue =
                learner.MotherTongue;

            IndigenousPeopleEthnicGroup =
                learner.IndigenousPeopleEthnicGroup;

            Religion =
                learner.Religion;

            HouseStreetSitioPurok =
                learner.HouseStreetSitioPurok;

            Barangay =
                learner.Barangay;

            MunicipalityCity =
                learner.MunicipalityCity;

            Province =
                learner.Province;

            FatherName =
                learner.FatherName;

            MotherMaidenName =
                learner.MotherMaidenName;

            GuardianName =
                learner.GuardianName;

            GuardianRelationship =
                learner.GuardianRelationship;

            LearningModality =
                learner.LearningModality;

            IsCctRecipient =
                learner.IsCctRecipient;

            CctReferenceNumber =
                learner.CctReferenceNumber;

            IsBalikAral =
                learner.IsBalikAral;

            LastSchoolAttended =
                learner.LastSchoolAttended;

            LastSchoolYearAttended =
                learner.LastSchoolYearAttended;

            IsSpecialNeedsEducation =
                learner.IsSpecialNeedsEducation;

            SpecialNeedsDetails =
                learner.SpecialNeedsDetails;

            IsAccelerated =
                learner.IsAccelerated;

            AcceleratedLevel =
                learner.AcceleratedLevel;

            Sf1Remarks =
                learner.Sf1Remarks;
        }

        private void ClearSf1Information()
        {
            MotherTongue =
                string.Empty;

            IndigenousPeopleEthnicGroup =
                string.Empty;

            Religion =
                string.Empty;

            HouseStreetSitioPurok =
                string.Empty;

            Barangay =
                string.Empty;

            MunicipalityCity =
                string.Empty;

            Province =
                string.Empty;

            FatherName =
                string.Empty;

            MotherMaidenName =
                string.Empty;

            GuardianName =
                string.Empty;

            GuardianRelationship =
                string.Empty;

            LearningModality =
                string.Empty;

            IsCctRecipient =
                false;

            CctReferenceNumber =
                string.Empty;

            IsBalikAral =
                false;

            LastSchoolAttended =
                string.Empty;

            LastSchoolYearAttended =
                string.Empty;

            IsSpecialNeedsEducation =
                false;

            SpecialNeedsDetails =
                string.Empty;

            IsAccelerated =
                false;

            AcceleratedLevel =
                string.Empty;

            Sf1Remarks =
                string.Empty;
        }

        private string CreateCompleteAddress()
        {
            return string.Join(
                ", ",
                new[]
                {
                    HouseStreetSitioPurok.Trim(),
                    Barangay.Trim(),
                    MunicipalityCity.Trim(),
                    Province.Trim()
                }
                .Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value)));
        }

        private string GetPrimaryParentName()
        {
            if (!string.IsNullOrWhiteSpace(
                    FatherName))
            {
                return FatherName.Trim();
            }

            return MotherMaidenName.Trim();
        }
    }
}