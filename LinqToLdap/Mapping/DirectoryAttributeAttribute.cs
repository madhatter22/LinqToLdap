using System;

namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Maps a property to an attribute in a directory.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class DirectoryAttributeAttribute : Attribute
    {
        private string _dateTimeFormat = "yyyyMMddHHmmss.0Z";

        /// <summary>
        /// Maps the property as is to an attribute.
        /// </summary>
        public DirectoryAttributeAttribute()
        {
        }

        /// <summary>
        /// Maps a property to the specified <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">Name to map to</param>
        public DirectoryAttributeAttribute(string attributeName)
        {
            AttributeName = attributeName;
        }

        /// <summary>
        /// Maps a property to the specified <paramref name="attributeName"/>.
        /// </summary>
        /// <param name="attributeName">Name to map to</param>
        /// <param name="readOnly">Indicates if the property is read only</param>
        public DirectoryAttributeAttribute(string attributeName, bool readOnly)
        {
            AttributeName = attributeName;
            ReadOnly = readOnly;
        }

        /// <summary>
        /// Maps the property as is to an attribute.
        /// </summary>
        /// <param name="readOnly">Indicates if the property is read only</param>
        public DirectoryAttributeAttribute(bool readOnly)
        {
            ReadOnly = readOnly;
        }

        /// <summary>
        /// The mapped attribute name.
        /// </summary>
        public string AttributeName { get; private set; }

        /// <summary>
        /// Speicfy that a <see cref="Enum"/> is stored as an int.  The default is string.
        /// If the property is not a <see cref="Enum"/> then this setting will be ignored.
        /// </summary>
        /// <returns></returns>
        public bool EnumStoredAsInt { get; set; }

        /// <summary>
        /// Specify if the attribute is read only. Sets both <see cref="ReadOnlyOnAdd"/> and <see cref="ReadOnlyOnSet"/> to the value.
        /// </summary>
        public bool ReadOnly
        {
            set
            {
                ReadOnlyOnAdd = value;
                ReadOnlyOnSet = value;
            }
        }

        /// <summary>
        /// Specify if the attribute is read only during add operations.
        /// </summary>
        public bool ReadOnlyOnAdd { get; set; }

        /// <summary>
        /// Specify if the attribute is read only during set operations.
        /// </summary>
        public bool ReadOnlyOnSet { get; set; }

        /// <summary>
        /// Specify the format of the DateTime in the directory.  Use null if the DateTime is stored in file time.
        /// Default format is yyyyMMddHHmmss.0Z.  If the property is not a <see cref="DateTime"/> then this setting will be ignored.
        /// </summary>
        /// <example>
        /// <para>Common formats</para>
        /// <para>
        /// Active Directory: yyyyMMddHHmmss.0Z
        /// </para>
        /// <para>
        /// eDirectory: yyyyMMddHHmmssZ or yyyyMMddHHmmss00Z
        /// </para>
        /// </example>
        /// <returns></returns>
        public string DateTimeFormat
        {
            get { return _dateTimeFormat; }
            set { _dateTimeFormat = value; }
        }
    }
}