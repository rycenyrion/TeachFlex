using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using TeachFlex.Models;
using TeachFlex.Repositories;
using TeachFlex.Services;

namespace TeachFlex.ViewModels
{
    public partial class ActivitySheetViewModel : ViewModelBase
    {
        private readonly ISchoolRepository _schools;
        private readonly IAcademicYearRepository _years;
        private readonly ISchoolClassRepository _classes;
        private readonly ISubjectRepository _subjects;
        private readonly ILessonPlanStore _plans;
        private readonly ActivitySheetStore _store;
        private readonly ActivitySheetPdfService _pdf;
        private readonly CommonsActivityImageService _images;
        private readonly ActivityItemGenerator _items;
        private readonly IDialogService _dialogs;
        private School? _school;
        private AcademicYear? _year;
        private SchoolClass[] _classList = Array.Empty<SchoolClass>();
        private string _id = Guid.NewGuid().ToString("N");
        private LessonPlan? _selectedPlan;
        private ActivitySheet? _selectedSheet;
        private int _session = 1;
        private string _title = "";
        private string _instructions = "";
        private string _questions = "";
        private string _teacherGuide = "";
        private string _imageSearch = "";
        private CommonsImageResult? _selectedImage;
        private BitmapImage? _imagePreview;
        private string _imageBase64 = "";
        private string _imageTitle = "";
        private string _imageSourceUrl = "";
        private string _imageLicense = "";
        private string _imageLicenseUrl = "";
        private string _imageAuthor = "";
        private int _itemCount = 20;

        public ObservableCollection<LessonPlan> SavedPlans { get; } = new();
        public ObservableCollection<ActivitySheet> SavedSheets { get; } = new();
        public ObservableCollection<int> Sessions { get; } = new(Enumerable.Range(1, 5));
        public ObservableCollection<int> ItemCounts { get; } = new(new[] { 10, 15, 20 });
        public int ItemCount { get => _itemCount; set => SetProperty(ref _itemCount, value); }
        public ObservableCollection<CommonsImageResult> ImageResults { get; } = new();
        public string ImageSearch { get => _imageSearch; set => SetProperty(ref _imageSearch, value); }
        public CommonsImageResult? SelectedImage { get => _selectedImage; set => SetProperty(ref _selectedImage, value); }
        public BitmapImage? ImagePreview { get => _imagePreview; private set => SetProperty(ref _imagePreview, value); }
        public string ImageCredit => string.IsNullOrWhiteSpace(_imageTitle) ? "No image selected" :
            $"{_imageTitle} · {_imageAuthor} · {_imageLicense}";
        public LessonPlan? SelectedPlan
        {
            get => _selectedPlan;
            set
            {
                if (SetProperty(ref _selectedPlan, value) && value != null && string.IsNullOrWhiteSpace(ImageSearch))
                    ImageSearch = _items.CanGenerate(value.LearningCompetency) ? "number cards" :
                        string.Join(" ", value.Title.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2));
            }
        }
        public int Session { get => _session; set => SetProperty(ref _session, value); }
        public string Title { get => _title; set => SetProperty(ref _title, value); }
        public string Instructions { get => _instructions; set => SetProperty(ref _instructions, value); }
        public string Questions { get => _questions; set => SetProperty(ref _questions, value); }
        public string TeacherGuide { get => _teacherGuide; set => SetProperty(ref _teacherGuide, value); }
        public ActivitySheet? SelectedSheet
        {
            get => _selectedSheet;
            set
            {
                if (!SetProperty(ref _selectedSheet, value) || value == null) return;
                _id = value.Id;
                SelectedPlan = SavedPlans.FirstOrDefault(x => x.Id == value.LessonPlanId);
                Session = value.Session;
                Title = value.Title;
                Instructions = value.Instructions;
                Questions = value.Questions;
                TeacherGuide = value.TeacherGuide;
                _imageBase64 = value.ImageBase64;
                _imageTitle = value.ImageTitle;
                _imageSourceUrl = value.ImageSourceUrl;
                _imageLicense = value.ImageLicense;
                _imageLicenseUrl = value.ImageLicenseUrl;
                _imageAuthor = value.ImageAuthor;
                UpdatePreview();
                StatusMessage = "Saved activity sheet loaded.";
            }
        }

        public ActivitySheetViewModel(ISchoolRepository schools, IAcademicYearRepository years,
            ISchoolClassRepository classes, ISubjectRepository subjects, ILessonPlanStore plans,
            ActivitySheetStore store, ActivitySheetPdfService pdf,
            CommonsActivityImageService images, ActivityItemGenerator items, IDialogService dialogs)
        {
            _schools = schools; _years = years; _classes = classes; _subjects = subjects;
            _plans = plans; _store = store; _pdf = pdf; _images = images; _items = items; _dialogs = dialogs;
            _ = LoadAsync();
        }

        private async Task LoadAsync()
        {
            try
            {
                IsBusy = true;
                _school = await _schools.GetActiveSchoolAsync();
                if (_school == null) { StatusMessage = "Set up the active school first."; return; }
                _year = await _years.GetCurrentAsync(_school.Id);
                if (_year == null) { StatusMessage = "Choose an active school year first."; return; }
                _classList = (await _classes.GetByAcademicYearAsync(_school.Id, _year.Id)).ToArray();
                foreach (var plan in await _plans.GetByYearAsync(_school.Id, _year.Id)) SavedPlans.Add(plan);
                foreach (var sheet in await _store.GetAsync(_school.Id, _year.Id)) SavedSheets.Add(sheet);
                StatusMessage = SavedPlans.Count == 0 ? "Save an ILAW lesson plan first." : "Pumili ng lesson plan at session.";
            }
            catch (Exception ex) { StatusMessage = ex.Message; _dialogs.ShowError(ex.Message, "Activity Sheets"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void NewSheet()
        {
            _selectedSheet = null; OnPropertyChanged(nameof(SelectedSheet));
            _id = Guid.NewGuid().ToString("N");
            Title = Instructions = Questions = TeacherGuide = "";
            _imageBase64 = _imageTitle = _imageSourceUrl = _imageLicense = _imageLicenseUrl = _imageAuthor = "";
            ImagePreview = null;
            OnPropertyChanged(nameof(ImageCredit));
            StatusMessage = "New activity sheet ready.";
        }

        private void UpdatePreview()
        {
            ImagePreview = null;
            if (!string.IsNullOrWhiteSpace(_imageBase64))
            {
                try
                {
                    using var stream = new MemoryStream(Convert.FromBase64String(_imageBase64));
                    var preview = new BitmapImage();
                    preview.BeginInit();
                    preview.CacheOption = BitmapCacheOption.OnLoad;
                    preview.StreamSource = stream;
                    preview.EndInit();
                    preview.Freeze();
                    ImagePreview = preview;
                }
                catch { StatusMessage = "Saved image preview could not be loaded."; }
            }
            OnPropertyChanged(nameof(ImageCredit));
        }

        [RelayCommand]
        private async Task SearchImagesAsync()
        {
            if (string.IsNullOrWhiteSpace(ImageSearch))
            { _dialogs.ShowWarning("Mag-type ng image search term, halimbawa: plato o triangles.", "Image Search"); return; }
            try
            {
                IsBusy = true;
                ImageResults.Clear();
                foreach (var result in await _images.SearchAsync(ImageSearch)) ImageResults.Add(result);
                StatusMessage = ImageResults.Count == 0 ? "Walang JPG/PNG na may license metadata. Subukan ang ibang search term." :
                    $"{ImageResults.Count} images found. Pumili at i-click ang Gamitin ang Larawan.";
            }
            catch (Exception ex) { StatusMessage = "Image search failed. Check internet connection."; _dialogs.ShowError(ex.Message, "Image Search"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task AttachImageAsync()
        {
            if (SelectedImage == null) { _dialogs.ShowWarning("Pumili muna ng larawan sa results.", "Image Search"); return; }
            try
            {
                IsBusy = true;
                byte[] bytes = await _images.DownloadAsync(SelectedImage);
                _imageBase64 = Convert.ToBase64String(bytes);
                _imageTitle = SelectedImage.Title;
                _imageSourceUrl = SelectedImage.PageUrl;
                _imageLicense = SelectedImage.License;
                _imageLicenseUrl = SelectedImage.LicenseUrl;
                _imageAuthor = SelectedImage.Author;
                UpdatePreview();
                StatusMessage = "Image attached. Save the activity sheet to retain it offline.";
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Attach Image"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private void RemoveImage()
        {
            _imageBase64 = _imageTitle = _imageSourceUrl = _imageLicense = _imageLicenseUrl = _imageAuthor = "";
            UpdatePreview();
        }

        [RelayCommand]
        private void ChooseLocalImage()
        {
            var dialog = new OpenFileDialog { Filter = "Image (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png" };
            if (dialog.ShowDialog() != true) return;
            try
            {
                var info = new FileInfo(dialog.FileName);
                if (info.Length > 3_000_000) throw new InvalidOperationException("Larawang hanggang 3 MB lamang ang puwedeng gamitin.");
                _imageBase64 = Convert.ToBase64String(File.ReadAllBytes(dialog.FileName));
                _imageTitle = info.Name;
                _imageSourceUrl = "Teacher-provided file";
                _imageLicense = "Review permission to use";
                _imageLicenseUrl = "";
                _imageAuthor = "Teacher-provided";
                UpdatePreview();
                StatusMessage = "Local image attached. Save sheet to retain it.";
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Choose Image"); }
        }

        [RelayCommand]
        private void GenerateDraft()
        {
            if (SelectedPlan == null) { _dialogs.ShowWarning("Pumili muna ng saved ILAW plan.", "Activity Sheet"); return; }
            if (!string.IsNullOrWhiteSpace(Questions) &&
                !_dialogs.Confirm("Palitan ang kasalukuyang activity draft?", "Activity Sheet")) return;
            string competency = SelectedPlan.LearningCompetency.Trim();
            if (string.IsNullOrWhiteSpace(competency)) { _dialogs.ShowWarning("Walang learning competency sa plan.", "Activity Sheet"); return; }
            if (!_items.CanGenerate(competency))
            {
                StatusMessage = "Wala pang verified item bank para sa competency na ito. Puwedeng mag-type ng sariling activity at answer key.";
                _dialogs.ShowWarning(StatusMessage, "Activity Sheet");
                return;
            }
            var draft = _items.Generate(competency, ItemCount);
            Title = $"{SelectedPlan.Title} · Session {Session}";
            Instructions = draft.Instructions;
            Questions = draft.Questions;
            TeacherGuide = draft.AnswerKey;
            StatusMessage = $"{ItemCount} distinct multiple-choice items generated with answer key. Suriin bago i-export.";
        }

        private ActivitySheet Current() => new()
        {
            Id = _id, SchoolId = _school!.Id, AcademicYearId = _year!.Id,
            LessonPlanId = SelectedPlan!.Id, Session = Session,
            Title = Title.Trim(), Instructions = Instructions.Trim(),
            Questions = Questions.Trim(), TeacherGuide = TeacherGuide.Trim(),
            ImageBase64 = _imageBase64, ImageTitle = _imageTitle,
            ImageSourceUrl = _imageSourceUrl, ImageLicense = _imageLicense,
            ImageLicenseUrl = _imageLicenseUrl, ImageAuthor = _imageAuthor
        };

        private bool Valid()
        {
            if (_school != null && _year != null && SelectedPlan != null &&
                !string.IsNullOrWhiteSpace(Title) && !string.IsNullOrWhiteSpace(Questions) &&
                !string.IsNullOrWhiteSpace(TeacherGuide)) return true;
            _dialogs.ShowWarning("Piliin ang plan at kumpletuhin ang title, learner activities at teacher guide.", "Activity Sheet");
            return false;
        }

        [RelayCommand]
        private async Task SaveAsync()
        {
            if (!Valid()) return;
            try
            {
                IsBusy = true;
                await _store.SaveAsync(Current());
                var sheets = await _store.GetAsync(_school!.Id, _year!.Id);
                SavedSheets.Clear();
                foreach (var sheet in sheets) SavedSheets.Add(sheet);
                StatusMessage = "Activity sheet saved.";
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Save Activity Sheet"); }
            finally { IsBusy = false; }
        }

        [RelayCommand]
        private async Task ExportLearnerAsync() => await ExportAsync(false);

        [RelayCommand]
        private async Task ExportTeacherAsync() => await ExportAsync(true);

        private async Task ExportAsync(bool teacher)
        {
            if (!Valid()) return;
            if (teacher && !_dialogs.Confirm("Nailagay at nasuri na ba ang eksaktong sagot sa Teacher Guide?",
                "Teacher Copy")) return;
            var dialog = new SaveFileDialog
            {
                Filter = "PDF Document (*.pdf)|*.pdf", DefaultExt = ".pdf", AddExtension = true,
                FileName = $"Activity-Sheet-Session-{Session}-{(teacher ? "Teacher" : "Learner")}.pdf"
            };
            if (dialog.ShowDialog() != true) return;
            try
            {
                IsBusy = true;
                var schoolClass = _classList.FirstOrDefault(x => x.Id == SelectedPlan!.SchoolClassId);
                var subjects = schoolClass == null ? null : await _subjects.GetByClassAsync(schoolClass.Id);
                var subject = subjects?.FirstOrDefault(x => x.Id == SelectedPlan!.SubjectId);
                _pdf.Export(Current(), SelectedPlan!, _school!.SchoolName, _year!.DisplayName,
                    schoolClass?.DisplayName ?? "Class", subject?.SubjectName ?? "Subject", teacher, dialog.FileName);
                StatusMessage = "PDF created: " + dialog.FileName;
                _dialogs.ShowInformation(dialog.FileName, "Activity Sheet PDF");
            }
            catch (Exception ex) { _dialogs.ShowError(ex.Message, "Export Activity Sheet"); }
            finally { IsBusy = false; }
        }
    }
}
