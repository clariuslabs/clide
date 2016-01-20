namespace Clide
{
	/// <summary>
	/// Exposes the <see cref="As{T}"/> method that allows any 
	/// object to be adapted to any other type (provided there 
	/// is a compatible adapter registered in the system).
	/// </summary>
	public interface IAdaptable<TSource> : IFluentInterface
        where TSource : class
    {
        /// <summary>
        /// Adapts the instance to the given target type.
        /// </summary>
        /// <returns>The adapted instance or <see langword="null"/> if no compatible adapter was found.</returns>
        T As<T>() where T : class;
    }
}
