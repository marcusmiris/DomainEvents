using System.Collections.Generic;

namespace Miris.DomainEvents.Handlers
{
    /// <summary>
    ///     Objeto que determina quais são os handlers de um determinado evento.
    /// </summary>
    public interface IDomainEventHandlersRegistry
    {
        /// <summary>
        ///     Registra handlers.
        /// </summary>
        void Register<TEvent>(IDomainEventHandler<TEvent> handler) where TEvent : IDomainEvent;

        /// <summary>
        ///     Retorna todos os objetos que escutam determinado evento.
        /// </summary>
        IEnumerable<IDomainEventHandler<TEvent>> GetAllHandlers<TEvent>() where TEvent : IDomainEvent;
    }
}
