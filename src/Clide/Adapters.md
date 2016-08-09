# Adapter Pattern Support

Clide provides built-in adapters to convert to and from a variety of types. 
This support is extensible by providing custom IAdapter<TFrom, TTo> components.

After importing Clide.Patterns.Adapter, the Adapt extension method will be 
available on any object. Provided an adapter implementation exists, using the 
As<T> smart cast operator will return a properly adapted implementation for
the given source object.

If a direct conversion is not possible using a single adapter, consider 
using an intermediate common representation as a bridge (i.e. Clide's 
solution tree abstraction can be used to bridge DTE and IVs* APIs).

## The following is a list of the built-in supported conversions

