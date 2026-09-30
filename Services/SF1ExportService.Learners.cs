using System;
using System.Collections.Generic;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.Services
{
    public partial class SF1ExportService
    {
        private static void WriteLearnerInformation(
            dynamic sheet,
            SF1ExportRequest request,
            Sf1SheetLayout layout)
        {
            List<SF1LearnerRow> maleLearners =
                request.Learners
                    .Where(
                        learner =>
                            learner.Sex.Equals(
                                "Male",
                                StringComparison.OrdinalIgnoreCase))
                    .OrderBy(
                        learner =>
                            learner.LearnerName)
                    .ToList();

            List<SF1LearnerRow> femaleLearners =
                request.Learners
                    .Where(
                        learner =>
                            learner.Sex.Equals(
                                "Female",
                                StringComparison.OrdinalIgnoreCase))
                    .OrderBy(
                        learner =>
                            learner.LearnerName)
                    .ToList();

            WriteLearnerGroup(
                sheet,
                maleLearners,
                layout.MaleStartRow);

            WriteLearnerGroup(
                sheet,
                femaleLearners,
                layout.FemaleStartRow);

            SetCellValue(
                sheet,
                layout.MaleTotalRow,
                1,
                maleLearners.Count);

            SetCellValue(
                sheet,
                layout.FemaleTotalRow,
                1,
                femaleLearners.Count);

            SetCellValue(
                sheet,
                layout.CombinedTotalRow,
                1,
                maleLearners.Count +
                femaleLearners.Count);
        }

        private static void WriteLearnerGroup(
            dynamic sheet,
            IReadOnlyList<SF1LearnerRow> learners,
            int startRow)
        {
            for (int index = 0;
                 index < learners.Count;
                 index++)
            {
                WriteLearnerRow(
                    sheet,
                    startRow + index,
                    learners[index]);
            }
        }

        private static void WriteLearnerRow(
            dynamic sheet,
            int row,
            SF1LearnerRow learner)
        {
            SetTextCellValue(
                sheet,
                row,
                1,
                learner.Lrn);

            SetCellValue(
                sheet,
                row,
                3,
                learner.LearnerName);

            SetCellValue(
                sheet,
                row,
                7,
                GetSexCode(
                    learner.Sex));

            SetDateCellValue(
                sheet,
                row,
                8,
                learner.BirthDate);

            SetCellValue(
                sheet,
                row,
                10,
                learner.AgeAsOfFirstFriday);

            SetCellValue(
                sheet,
                row,
                12,
                learner.MotherTongue);

            SetCellValue(
                sheet,
                row,
                14,
                learner.IndigenousPeopleEthnicGroup);

            SetCellValue(
                sheet,
                row,
                15,
                learner.Religion);

            SetCellValue(
                sheet,
                row,
                16,
                learner.HouseStreetSitioPurok);

            SetCellValue(
                sheet,
                row,
                18,
                learner.Barangay);

            SetCellValue(
                sheet,
                row,
                21,
                learner.MunicipalityCity);

            SetCellValue(
                sheet,
                row,
                23,
                learner.Province);

            SetCellValue(
                sheet,
                row,
                28,
                learner.FatherName);

            SetCellValue(
                sheet,
                row,
                32,
                learner.MotherMaidenName);

            SetCellValue(
                sheet,
                row,
                37,
                learner.GuardianName);

            SetCellValue(
                sheet,
                row,
                41,
                learner.GuardianRelationship);

            SetCellValue(
                sheet,
                row,
                42,
                learner.ContactNumber);

            SetCellValue(
                sheet,
                row,
                44,
                learner.LearningModality);

            SetCellValue(
                sheet,
                row,
                45,
                CreateLearnerRemarks(
                    learner));
        }

        private static void SetTextCellValue(
            dynamic sheet,
            int row,
            int column,
            string value)
        {
            dynamic cell =
                sheet.Cells[
                    row,
                    column];

            cell.NumberFormat =
                "@";

            cell.Value2 =
                value;
        }

        private static void SetDateCellValue(
            dynamic sheet,
            int row,
            int column,
            DateTime? value)
        {
            dynamic cell =
                sheet.Cells[
                    row,
                    column];

            if (!value.HasValue)
            {
                cell.Value2 =
                    null;

                return;
            }

            cell.Value2 =
                value.Value.ToOADate();

            cell.NumberFormat =
                "mm/dd/yyyy";
        }

        private static string GetSexCode(
            string sex)
        {
            if (sex.Equals(
                    "Male",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "M";
            }

            if (sex.Equals(
                    "Female",
                    StringComparison.OrdinalIgnoreCase))
            {
                return "F";
            }

            return sex;
        }

        private static string CreateLearnerRemarks(
            SF1LearnerRow learner)
        {
            List<string> remarks =
                new();

            if (learner.EnrollmentType.Equals(
                    "Late Enrollee",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddRemark(
                    remarks,
                    "LE",
                    FormatDate(
                        learner.EnrollmentDate));
            }

            if (learner.EnrollmentType.Equals(
                    "Transferred In",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddRemark(
                    remarks,
                    "T/I",
                    JoinDetails(
                        learner.PreviousSchoolName,
                        FormatDate(
                            learner.EnrollmentDate)));
            }

            if (learner.ExitReason.Equals(
                    "Transferred Out",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddRemark(
                    remarks,
                    "T/O",
                    JoinDetails(
                        learner.NextSchoolName,
                        FormatDate(
                            learner.ExitDate)));
            }

            if (learner.ExitReason.Equals(
                    "Dropped Out",
                    StringComparison.OrdinalIgnoreCase))
            {
                AddRemark(
                    remarks,
                    "DRP",
                    FormatDate(
                        learner.ExitDate));
            }

            if (learner.IsCctRecipient)
            {
                AddRemark(
                    remarks,
                    "CCT",
                    learner.CctReferenceNumber);
            }

            if (learner.IsBalikAral)
            {
                AddRemark(
                    remarks,
                    "B/A",
                    JoinDetails(
                        learner.LastSchoolAttended,
                        learner.LastSchoolYearAttended));
            }

            if (learner.IsSpecialNeedsEducation)
            {
                AddRemark(
                    remarks,
                    "SNED",
                    learner.SpecialNeedsDetails);
            }

            if (learner.IsAccelerated)
            {
                AddRemark(
                    remarks,
                    "ACL",
                    learner.AcceleratedLevel);
            }

            if (!string.IsNullOrWhiteSpace(
                    learner.AdditionalRemarks))
            {
                remarks.Add(
                    learner.AdditionalRemarks.Trim());
            }

            return string.Join(
                "; ",
                remarks);
        }

        private static void AddRemark(
            ICollection<string> remarks,
            string code,
            string details)
        {
            remarks.Add(
                string.IsNullOrWhiteSpace(
                    details)
                    ? code
                    : $"{code} - {details}");
        }

        private static string JoinDetails(
            params string[] values)
        {
            return string.Join(
                " - ",
                values.Where(
                    value =>
                        !string.IsNullOrWhiteSpace(
                            value)));
        }

        private static string FormatDate(
            DateTime? value)
        {
            return value.HasValue
                ? value.Value.ToString(
                    "MM/dd/yyyy")
                : string.Empty;
        }
    }
}