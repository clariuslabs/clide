namespace Clide
{
    /// <summary>
    /// Service that provides pluggable adaptation of types.
    /// </summary>
    public interface IAdapterService
    {
        /// <summary>
        /// Returns an adaptable object for the given <paramref name="source"/>.
        /// </summary>
        /// <returns>The adaptable object for the given source type.</returns>
        IAdaptable<TSource> Adapt<TSource>(TSource source) where TSource : class;
    }
}