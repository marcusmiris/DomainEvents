using System;
using System.Diagnostics;
using Miris.DomainEvents.Dispatching;
using Miris.DomainEvents.Handlers;
using Miris.DomainEvents.ImmediateConsistency;

namespace Miris.DomainEvents
{

    /// <summary>
    ///     Entry point helper class for Domain Events.
    /// </summary>
    public static class DomainEvents
    {

        /// <summary>
        ///     Lugar onde são/estão registrados todos os handlers de evento.
        /// </summary>
        public static IDomainEventHandlersRegistry HandlersRegistry;

        /// <summary>
        ///     O cara que sabe pra quem delegar os eventos.
        /// </summary>
        public static IDomainEventDispatcher Dispatcher;


        #region ' Register(...) '

        /// <summary>
        ///     Registers the specified handler for the given domain event.
        /// </summary>
        public static void Register<TEvento>(IDomainEventHandler<TEvento> handler) where TEvento : IDomainEvent
        {
            if (HandlersRegistry == null) HandlersRegistry = new DomainEventHandlersRegistry();
            HandlersRegistry.Register(handler);
        }

        /// <summary>
        ///     Registers the specified callback for the given domain event.
        /// </summary>
        /// <param name="callback">
        ///     The callback.
        /// </param>
        public static void Register<TEvento>(Action<TEvento> callback) where TEvento : IDomainEvent
            => Register(new DelegateAsEventHandlerAdapter<TEvento>(callback));

        #endregion

        /// <summary>
        ///     Raises the specified domain event and calls the event handlers.
        /// </summary>
        /// <typeparam name="TEvento"></typeparam>
        /// <param name="domainEvent">The domain event.</param>
        [DebuggerStepThrough]
        public static void Raise<TEvento>(TEvento domainEvent) where TEvento : IDomainEvent
        {
            if (Dispatcher == null) Dispatcher = new ImmediatelyConsistentEventDispatcher();
            Dispatcher.Submit(domainEvent);
        }

    }
}
