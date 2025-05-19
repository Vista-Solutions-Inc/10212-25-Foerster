namespace Foerster.ViewModels.ConfigMediators
{
    /// <summary>
    /// Defines the basic properties and methods a class must implement to be used as a task configuration mediator.
    /// </summary>
    public interface IConfigMediator
    {
        #region Properties
        bool IsConfigurationValid { get; }
        string ErrorMessage { get; }
        #endregion

        #region Methods
        void RegisterSections(List<IValidatable> sections);
        #endregion
    }
}
