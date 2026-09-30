using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using TeachFlex.Models;
using TeachFlex.Repositories;

namespace TeachFlex.ViewModels
{
    public partial class MyClassesViewModel
    {
        private readonly ISubjectRepository
            _subjectRepository;

        public ObservableCollection<Subject>
            ClassSubjects
        {
            get;
        }

        private async Task LoadClassSubjectsAsync()
        {
            ClassSubjects.Clear();

            if (SelectedClass == null)
            {
                return;
            }

            try
            {
                IReadOnlyList<Subject> subjects =
                    await _subjectRepository
                        .GetByClassAsync(
                            SelectedClass.Id);

                foreach (Subject subject
                         in subjects)
                {
                    ClassSubjects.Add(
                        subject);
                }
            }
            catch (Exception exception)
            {
                _dialogService.ShowError(
                    $"TeachFlex could not load the " +
                    $"class subjects.\n\n" +
                    $"{exception.Message}",
                    "Subject List Error");
            }
        }
    }
}