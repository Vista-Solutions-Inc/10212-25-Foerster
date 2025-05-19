namespace Foerster.ViewModels.ConfigMediators
{
    public partial class ConfigMediator : ObservableObject, IConfigMediator
    {
        #region Fields
        private readonly List<IValidatable> _configSections = new();
        private string _errorMessage;
        #endregion

        #region Observable Properties
        [ObservableProperty] private bool _isConfigurationValid;
        #endregion

        #region Properties
        public string ErrorMessage => _errorMessage;
        #endregion

        #region Public Methods
        public void RegisterSections(List<IValidatable> configSections)
        {
            foreach (var section in configSections)
            {
                _configSections.Add(section);
                // Subscribe to the section validation event
                section.ValidationChanged += OnSectionValidationChanged;
            }
        }
        #endregion

        #region Private Methods
        private void OnSectionValidationChanged(object sender, EventArgs eventArgs)
        {
            // Find the issuing sections
            var invalidSection = _configSections.FirstOrDefault(s => !s.IsValid);
            _errorMessage = invalidSection?.ErrorMessage ?? string.Empty;
            // Update observable property
            IsConfigurationValid = (_errorMessage != string.Empty) ? false : true;
        }
        #endregion
    }
}
