namespace LinqToLdap.Mapping
{
    /// <summary>
    /// Controls the read only strategy for a property.
    /// </summary>
    public enum ReadOnly
    {
        /// <summary>
        /// Never read only
        /// </summary>
        Never = 0,

        /// <summary>
        /// Read only during add operations but will be considered during update operations.
        /// </summary>
        OnAdd = 1,

        /// <summary>
        /// Read only during update operations but will be considered during add operations.
        /// </summary>
        OnUpdate = 2,

        /// <summary>
        /// Always read only
        /// </summary>
        Always = 3
    }
}