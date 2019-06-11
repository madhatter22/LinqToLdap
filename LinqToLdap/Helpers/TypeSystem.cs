using System;
using System.Collections.Generic;
using System.Linq;

namespace LinqToLdap.Helpers
{
    /// <summary>
    /// Pulled from http://msdn.microsoft.com/en-us/library/bb546158.aspx
    /// </summary>
    internal static class TypeSystem
    {
        internal static Type GetElementType(Type seqType)
        {
            Type ienum = FindIEnumerable(seqType);
            return ienum == null ? seqType : ienum.GetGenericArguments()[0];
        }

        private static Type FindIEnumerable(Type seqType)
        {
            if (seqType == null || seqType == typeof(string))
                return null;

            if (seqType.IsArray)
                return typeof(IEnumerable<>).MakeGenericType(seqType.GetElementType());

            if (seqType.IsGenericType)
            {
                foreach (Type ienum in
                    seqType.GetGenericArguments().Select(arg => typeof(IEnumerable<>).MakeGenericType(arg)).Where(ienum => ienum.IsAssignableFrom(seqType)))
                {
                    return ienum;
                }
            }

            Type[] ifaces = seqType.GetInterfaces();
            if (ifaces != null && ifaces.Length > 0)
            {
                foreach (Type ienum in ifaces.Select(FindIEnumerable).Where(ienum => ienum != null))
                {
                    return ienum;
                }
            }

            if (seqType.BaseType != null && seqType.BaseType != typeof(object))
            {
                return FindIEnumerable(seqType.BaseType);
            }

            return null;
        }
    }
}