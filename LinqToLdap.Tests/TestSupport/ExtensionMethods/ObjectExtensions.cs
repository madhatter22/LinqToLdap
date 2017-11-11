using System;
using System.Linq;
using System.Reflection;

namespace LinqToLdap.Tests.TestSupport.ExtensionMethods
{
    public static class ObjectExtensions
    {
        private const BindingFlags Flags =
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy;

        public static T As<T>(this object obj) where T : class
        {
            return obj as T;
        }

        public static T CastTo<T>(this object obj)
        {
            return (T)obj;
        }

        public static object Call(this object obj, string methodName, params object[] parameters)
        {
            var type = obj.GetType();
            var method = type.GetMethod(methodName, Flags, null,
                           parameters == null ? Type.EmptyTypes : parameters.Select(p => p.GetType()).ToArray(), null);

            return method.Invoke(obj, parameters);
        }

        /// <summary>
        /// Returns a field given the name and the type of object.
        /// </summary>
        /// <param name="fieldName">The name of the field</param>
        /// <param name="objectType">The type of object to which the field belongs</param>
        /// <returns>The field</returns>
        private static FieldInfo GetField(string fieldName, Type objectType)
        {
            FieldInfo field = objectType.GetField(fieldName, Flags);
            if (field == null)
            {
                Type baseType = objectType.BaseType;
                while (baseType != typeof(object))
                {
                    field = GetField(fieldName, baseType);
                    if (field != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return field;
        }

        /// <summary>
        /// Returns a property given the name and the type of object.
        /// </summary>
        /// <param name="propertyName">The name of the property</param>
        /// <param name="objectType">The type of object to which the property belongs</param>
        /// <returns>The property</returns>
        private static PropertyInfo GetProperty(string propertyName, Type objectType)
        {
            PropertyInfo property = objectType.GetProperty(propertyName, Flags);
            if (property == null)
            {
                Type baseType = objectType.BaseType;
                while (baseType != typeof(object))
                {
                    property = GetProperty(propertyName, baseType);
                    if (property != null)
                    {
                        break;
                    }
                    baseType = baseType.BaseType;
                }
            }
            return property;
        }

        /// <summary>
        /// Gets the value of a field for an object.
        /// </summary>
        /// <typeparam name="TProperty">The type of the field to look for.</typeparam>
        /// <param name="propertyName">The name of the field.</param>
        /// <param name="source">>The object to which the field belongs</param>
        /// <returns>The value of the field</returns>
        public static TProperty PropertyValue<TProperty>(this object source, string propertyName)
        {
            PropertyInfo property = GetProperty(propertyName, source.GetType());

            return (TProperty)property.GetValue(source, Flags, null, null, null);
        }

        /// <summary>
        /// Sets the value of a property for an object.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property to look for.</typeparam>
        /// <param name="propertyName">The name of the property.</param>
        /// <param name="source">>The object to which the property belongs</param>
        /// <param name="value">The value to which the property will be set.</param>
        public static void SetPropertyValue<TProperty>(this object source, string propertyName, TProperty value)
        {
            PropertyInfo property = GetProperty(propertyName, source.GetType());
            property.SetValue(source, value, Flags, null, null, null);
        }

        /// <summary>
        /// Gets the value of a field for an object.
        /// </summary>
        /// <typeparam name="TField">The type of the field to look for.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="source">>The object to which the field belongs</param>
        /// <returns>The value of the field</returns>
        public static TField FieldValueEx<TField>(this object source, string fieldName)
        {
            FieldInfo field = GetField(fieldName, source.GetType());

            return (TField)field.GetValue(source);
        }

        /// <summary>
        /// Gets the value of a field for an object.
        /// </summary>
        /// <typeparam name="TField">The type of the field to look for.</typeparam>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="source">>The object to which the field belongs</param>
        /// <param name="value">The value to which the field will be set.</param>
        public static void SetFieldValue<TField>(this object source, string fieldName, TField value)
        {
            FieldInfo field = GetField(fieldName, source.GetType());

            field.SetValue(source, value);
        }
    }
}
