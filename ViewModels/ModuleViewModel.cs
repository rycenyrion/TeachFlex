namespace TeachFlex.ViewModels
{
    public class ModuleViewModel :
        ViewModelBase
    {
        public ModuleViewModel(
            string title,
            string description,
            string developmentPhase)
        {
            Title =
                title;

            Description =
                description;

            DevelopmentPhase =
                developmentPhase;
        }

        public string Title
        {
            get;
        }

        public string Description
        {
            get;
        }

        public string DevelopmentPhase
        {
            get;
        }
    }
}