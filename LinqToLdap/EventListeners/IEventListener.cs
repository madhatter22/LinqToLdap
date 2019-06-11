using System;
using System.Collections.Generic;
using System.DirectoryServices.Protocols;
using System.Text;

namespace LinqToLdap.EventListeners
{
    /// <summary>
    /// Interface for events available for subscription notifications.
    /// </summary>
    public interface IEventListener
    {
    }

    /// <summary>
    /// Interface for update events.
    /// </summary>
    public interface IUpdateEventListener : IEventListener
    {
    }

    /// <summary>
    /// Interface for add events.
    /// </summary>
    public interface IAddEventListener : IEventListener
    {
    }

    /// <summary>
    /// Interface for delete events.
    /// </summary>
    public interface IDeleteEventListener : IEventListener
    {
    }

    /// <summary>
    /// Interface for events available for subscription notifications.
    /// </summary>
    /// <typeparam name="TObject">The instance for the event.</typeparam>
    /// <typeparam name="TRequest">The partially populated request to be sent to the directory.</typeparam>
    public interface IPreEventListener<TObject, TRequest> : IEventListener where TRequest : DirectoryRequest where TObject : class
    {
        /// <summary>
        /// Executes when the event is raised.
        /// </summary>
        /// <param name="preArgs">The arguments.</param>
        void Notify(ListenerPreArgs<TObject, TRequest> preArgs);
    }

    /// <summary>
    /// Interface for events available for subscription notifications.
    /// </summary>
    /// <typeparam name="TObject">The instance for the event.</typeparam>
    /// /// <typeparam name="TRequest">The request sent to the directory</typeparam>
    /// <typeparam name="TResponse">The response from the directory.</typeparam>
    public interface IPostEventListener<TObject, TRequest, TResponse> : IEventListener
        where TResponse : DirectoryResponse
        where TRequest : DirectoryRequest
        where TObject : class
    {
        /// <summary>
        /// Executes when the event is raised.
        /// </summary>
        /// <param name="preArgs">The arguments.</param>
        void Notify(ListenerPostArgs<TObject, TRequest, TResponse> preArgs);
    }
}