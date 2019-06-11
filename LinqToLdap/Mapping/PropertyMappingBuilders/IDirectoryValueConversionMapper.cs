namespace LinqToLdap.Mapping.PropertyMappingBuilders
{
    /// <summary>
    /// Interface for directory value conversion to instance value.
    /// </summary>
    /// <typeparam name="TPropertyMappingBuilder">Type of the mapping builder</typeparam>
    /// <typeparam name="TProperty">The property type</typeparam>
    public interface IDirectoryToConversion<TPropertyMappingBuilder, TProperty>
    {
        /// <summary>
        /// The value to return in place of the encountered directory value.
        /// </summary>
        /// <param name="value">The instance value</param>
        /// <returns></returns>
        TPropertyMappingBuilder Returns(TProperty value);
    }

    /// <summary>
    /// The mapper method for the directory value.
    /// </summary>
    /// <typeparam name="TPropertyMappingBuilder">The type of the property mapping builder.</typeparam>
    /// <typeparam name="TProperty">The type of the property.</typeparam>
    public interface IDirectoryValueConversionMapper<TPropertyMappingBuilder, TProperty>
    {
        /// <summary>
        /// Maps a single-valued directory value.
        /// </summary>
        /// <param name="directoryValue">The value from the directory.</param>
        /// <returns></returns>
        IDirectoryToConversion<TPropertyMappingBuilder, TProperty> DirectoryValue(string directoryValue);

        /// <summary>
        /// Maps a not set or empty value from the directory.
        /// </summary>
        /// <returns></returns>
        IDirectoryToConversion<TPropertyMappingBuilder, TProperty> DirectoryValueNotSetOrEmpty();
    }

    /// <summary>
    /// Interface for instance value conversion to a directory value.
    /// </summary>
    /// <typeparam name="TPropertyMappingBuilder">Type of the mapping builder</typeparam>
    public interface IInstanceToConversion<TPropertyMappingBuilder>
    {
        /// <summary>
        /// The value to send in place of the encountered instance value.
        /// </summary>
        /// <param name="value">The directory value</param>
        /// <returns></returns>
        TPropertyMappingBuilder Sends(string value);
    }

    /// <summary>
    /// The mapper method for the instance value.
    /// </summary>
    /// <typeparam name="TPropertyMappingBuilder"></typeparam>
    /// <typeparam name="TProperty"></typeparam>
    public interface IInstanceValueConversionMapper<TPropertyMappingBuilder, TProperty>
    {
        /// <summary>
        /// Maps a single-valued instace value.
        /// </summary>
        /// <param name="instacneValue">The value from the instance.</param>
        /// <returns></returns>
        IInstanceToConversion<TPropertyMappingBuilder> InstanceValue(TProperty instacneValue);

        /// <summary>
        /// Maps a not null or default value from the instance.
        /// </summary>
        /// <returns></returns>
        IInstanceToConversion<TPropertyMappingBuilder> InstanceValueNullOrDefault();
    }
}