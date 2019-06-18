namespace LinqToLdap.QueryCommands
{
    /// <summary>
    /// Indicates the type of command to create.
    /// </summary>
    public enum QueryCommandType
    {
        /// <summary>
        /// Represents a standard command
        /// </summary>
        StandardCommand = 0,

        /// <summary>
        /// Any command
        /// </summary>
        AnyCommand = 1,

        /// <summary>
        /// First / FirstOrDefault command
        /// </summary>
        FirstOrDefaultCommand = 2,

        /// <summary>
        /// Single command
        /// </summary>
        SingleCommand = 3,

        /// <summary>
        /// SingleOrDefault command
        /// </summary>
        SingleOrDefaultCommand = 4,

        /// <summary>
        /// Count command
        /// </summary>
        CountCommand = 5,

        /// <summary>
        /// GetRequest command
        /// </summary>
        GetRequestCommand = 6,

        /// <summary>
        /// FirstCommand command
        /// </summary>
        FirstCommand = 7
    }
}