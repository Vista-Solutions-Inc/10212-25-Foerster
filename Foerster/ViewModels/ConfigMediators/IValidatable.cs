namespace Foerster.ViewModels.ConfigMediators
{
    /// <summary>
    /// Defines the basic properties and methods that a configuration page must implement to be validated via a config mediator.
    /// </summary>
    public interface IValidatable
    {
        #region Events
        event EventHandler ValidationChanged;
        #endregion

        #region Properties
        bool IsValid { get; }
        string ErrorMessage { get; }
        #endregion
    }
}
