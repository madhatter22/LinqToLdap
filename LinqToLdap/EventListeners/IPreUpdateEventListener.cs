using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised before an update occurs.
    /// </summary>
    public interface IPreUpdateEventListener : IPreEventListener<object, ModifyRequest>, IUpdateEventListener
    {
    }
}