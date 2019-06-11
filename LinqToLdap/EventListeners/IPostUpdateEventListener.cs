using System.DirectoryServices.Protocols;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// The event raised after an update occurs.
    /// </summary>
    public interface IPostUpdateEventListener : IPostEventListener<object, ModifyRequest, ModifyResponse>, IUpdateEventListener
    {
    }
}