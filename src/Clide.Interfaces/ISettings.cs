using System.ComponentModel;
namespace Clide
{

    /// <summary>
    /// Interface used by settings that leverage the <see cref="OptionsPage{TControl, TSettings}"/> 
    /// base class for Tools|Options extensibility, and which are annotated with the <see cref="SettingsAttribute"/>.
    /// </summary>
    public interface ISettings : IEditableObject
    {
        /// <summary>
        /// Explicitly saves the settings. This happens automatically 
        /// also when using the <see cref="IEditableObject.BeginEdit"/> 
        /// and <see cref="IEditableObject.EndEdit"/> methods.
        /// </summary>
        /// <param name="saveDefaults">The optional parameter can be used 
        /// to forcely persist all properties, including those that 
        /// only have default values.</param>
        void Save(bool saveDefaults = false);
    }
}