using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised before a delete occurs.
    /// </summary>
    public interface IPreDeleteEventListener : IPreEventListener<string, DeleteRequest>, IDeleteEventListener
    {
    }
}