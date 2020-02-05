namespace Clide
{
    /// <summary>
    /// Main interface to deal with settings in the development environment. Provides 
    /// a friendly interface to read and save properties of an object to the environment 
    /// settings store.
    /// </summary>
    public interface ISettingsManager
    {
        /// <summary>
        /// Reads all saved properties of the given object. Default values are populated from 
        /// <see cref="System.ComponentModel.DefaultValueAttribute"/> too.
        /// </summary>
        void Read(object settings);

        /// <summary>
        /// Saves the specified settings, after validating it using the data annotations attributes.
        /// </summary>
        /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException"> The object is not valid.</exception>
        void Save(object settings, bool saveDefaults = false);
    }
}
