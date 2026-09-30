using System;
using System.Linq;
using TeachFlex.Models;

namespace TeachFlex.ViewModels
{
    public partial class LearnersViewModel
    {
        private string _learnerName =
            string.Empty;

        public string LearnerName
        {
            get => _learnerName;

            set
            {
                if (SetProperty(
                        ref _learnerName,
                        value))
                {
                    SaveLearnerCommand
                        .NotifyCanExecuteChanged();
                }
            }
        }

        private static bool TryParseLearnerName(
            string learnerName,
            out string lastName,
            out string firstName,
            out string middleName)
        {
            lastName =
                string.Empty;

            firstName =
                string.Empty;

            middleName =
                string.Empty;

            if (string.IsNullOrWhiteSpace(
                    learnerName))
            {
                return false;
            }

            string[] nameParts =
                learnerName.Split(
                    ',',
                    2,
                    StringSplitOptions
                        .TrimEntries);

            if (nameParts.Length != 2 ||
                string.IsNullOrWhiteSpace(
                    nameParts[0]) ||
                string.IsNullOrWhiteSpace(
                    nameParts[1]))
            {
                return false;
            }

            lastName =
                nameParts[0].Trim();

            string[] givenNameParts =
                nameParts[1]
                    .Split(
                        ' ',
                        StringSplitOptions
                            .RemoveEmptyEntries |
                        StringSplitOptions
                            .TrimEntries);

            if (givenNameParts.Length == 0)
            {
                return false;
            }

            string possibleMiddleInitial =
                givenNameParts[^1]
                    .Trim()
                    .TrimEnd('.');

            bool hasMiddleInitial =
                possibleMiddleInitial.Length == 1 &&
                char.IsLetter(
                    possibleMiddleInitial[0]);

            if (hasMiddleInitial)
            {
                middleName =
                    possibleMiddleInitial;

                firstName =
                    string.Join(
                        " ",
                        givenNameParts
                            .Take(
                                givenNameParts.Length -
                                1));
            }
            else
            {
                firstName =
                    string.Join(
                        " ",
                        givenNameParts);
            }

            return !string.IsNullOrWhiteSpace(
                firstName);
        }

        private static string FormatLearnerName(
            Learner learner)
        {
            string middleInitial =
                string.IsNullOrWhiteSpace(
                    learner.MiddleName)
                    ? string.Empty
                    : $" {learner.MiddleName.Trim()[0]}.";

            return
                $"{learner.LastName}, " +
                $"{learner.FirstName}" +
                $"{middleInitial}";
        }
    }
}