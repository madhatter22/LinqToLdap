using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised after an add occurs.
    /// </summary>
    public interface IPostAddEventListener : IPostEventListener<object, AddRequest, AddResponse>, IAddEventListener
    {
    }
}
