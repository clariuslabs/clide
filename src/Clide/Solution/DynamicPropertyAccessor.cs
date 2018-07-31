using System.Dynamic;

namespace Clide
{
    delegate bool TryGetMemberDelegate(GetMemberBinder binder, out object result);
    delegate bool TrySetMemberDelegate(SetMemberBinder binder, object value);
    delegate bool TryGetIndexDelegate(GetIndexBinder binder, object[] indexes, out object result);
    delegate bool TrySetIndexDelegate(SetIndexBinder binder, object[] indexes, object value);

    class DynamicPropertyAccessor
    {
        IPropertyAccessor accessor;

        public DynamicPropertyAccessor(IPropertyAccessor accessor)
        {
            this.accessor = accessor;
        }

        public bool TryGetMember(GetMemberBinder binder, out object result, TryGetMemberDelegate baseTryGetMember)
        {
            if (!accessor.TryGetProperty(binder.Name, out result))
                return baseTryGetMember(binder, out result);

            return true;
        }

        public bool TrySetMember(SetMemberBinder binder, object value, TrySetMemberDelegate baseTrySetMember)
        {
            if (!accessor.TrySetProperty(binder.Name, value))
                return baseTrySetMember(binder, value);

            return true;
        }

        public bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result, TryGetIndexDelegate baseTryGetIndex)
        {
            if (indexes.Length > 1 || !(indexes[0] is string))
            {
                result = null;
                return baseTryGetIndex(binder, indexes, out result);
            }

            var propertyName = (string)indexes[0];

            if (!accessor.TryGetProperty(propertyName, out result))
                return baseTryGetIndex(binder, indexes, out result);

            return true;
        }

        public bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value, TrySetIndexDelegate baseTrySetIndex)
        {
            if (indexes.Length > 1 || !(indexes[0] is string))
                return baseTrySetIndex(binder, indexes, value);

            var propertyName = (string)indexes[0];

            if (!accessor.TrySetProperty(propertyName, value))
                return baseTrySetIndex(binder, indexes, value);

            return true;
        }
    }
}
