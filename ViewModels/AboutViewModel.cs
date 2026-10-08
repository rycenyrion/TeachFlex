namespace TeachFlex.ViewModels
{
    public sealed class AboutViewModel : ViewModelBase
    {
        public string ApplicationName => "TeachFlex";
        public string Tagline => "One System, Every Classroom";
        public string Version => "1.0.0";
        public string Copyright => "© 2026 TeachFlex";
        public string Developer => "Developed by Ronel C. Ramos";
        public string Platform => "Offline-first Windows Desktop";
        public string RepositoryUrl => "https://github.com/rycenyrion/TeachFlex";
        public string Description =>
            "A teacher-focused desktop application for managing classroom records, lesson planning, learner information, and school forms.";
    }
}