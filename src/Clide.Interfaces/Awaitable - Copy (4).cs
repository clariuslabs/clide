using System;
using System.ComponentModel;
using System.Threading.Tasks;

namespace Clide
{
    /// <summary>
    /// Provides a factory for <see cref="Awaitable{T}"/>.
    /// </summary>
    public static class Awaitable
    {
        /// <summary>
        /// Creates an <see cref="Awaitable{T}"/> for the given getter.
        /// </summary>
        public static Awaitable<T> Create<T>(Func<Task<T>> getter) => new Awaitable<T>(getter);
    }

    /// <summary>
    /// Allows retrieving a property value by awaiting it,
    /// instead of turning such properties into <c>GetXXXAsync</c> 
    /// methods all over the place.
    /// </summary>
    public sealed class Awaitable<T>
    {
        Func<Task<T>> getter;

        /// <summary>
        /// Creates the awaitable value with a getter that retrieves the 
        /// task to calculate the value.
        /// </summary>
        public Awaitable(Func<Task<T>> getter) => this.getter = getter;

        /// <summary>
        /// <c>await</c> the value instead of invoking this method.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Runtime.CompilerServices.TaskAwaiter<T> GetAwaiter() => getter.Invoke().GetAwaiter();

        /// <summary>
        /// See <see cref="object.Equals(object)"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj) => base.Equals(obj);

        /// <summary>
        /// See <see cref="object.GetHashCode"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode();

        /// <summary>
        /// See <see cref="object.ToString"/>.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString() => base.ToString();
    }
}
