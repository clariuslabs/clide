namespace Clide.Sdk
{
    /// <summary>
    /// Marker interface for all static adapters.
    /// </summary>
    public interface IAdapter
    {
    }

    /// <summary>
    /// Interface implemented by adapters that know how to expose a 
    /// type as a different interface.
    /// </summary>
    /// <typeparam name="TFrom">The type that this adapter supports adapting from.</typeparam>
    /// <typeparam name="TTo">The type that this adapter adapts to.</typeparam>
    public interface IAdapter<in TFrom, out TTo> : IAdapter
    {
        /// <summary>
        /// Adapts the specified object from the <typeparamref name="TFrom"/> type to the 
        /// target <typeparamref name="TTo"/> type.
        /// </summary>
        TTo Adapt(TFrom from);
    }
}
