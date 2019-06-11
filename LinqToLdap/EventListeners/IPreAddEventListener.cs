using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised before an add occurs.
    /// </summary>
    public interface IPreAddEventListener : IPreEventListener<object, AddRequest>, IAddEventListener
    {

    }
}