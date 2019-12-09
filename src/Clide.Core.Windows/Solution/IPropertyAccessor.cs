namespace Clide
{
    interface IPropertyAccessor
    {
        bool TryGetProperty(string propertyName, out object result);

        bool TrySetProperty(string propertyName, object value);
    }
}
