using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Miris.DomainEvents.Handlers
{
    /// <summary>
    ///     Lugar onde ficam registrados os handlers dos eventos.
    /// </summary>
    public class DomainEventHandlersRegistry
        : IDomainEventHandlersRegistry
    {

        #region ' Atributos Privados '

        protected List<IDomainEventHandler> RegisteredHandlers { get; private set; }

        #endregion

        /// <summary>
        ///     Registra handlers.
        /// </summary>
        public virtual void Register<TEvent>(
            IDomainEventHandler<TEvent> handler) 
            where TEvent : IDomainEvent
        {
            if (RegisteredHandlers == null) RegisteredHandlers = new List<IDomainEventHandler>();
            RegisteredHandlers.Add(handler);
        }

        /// <summary>
        ///     Retorna todos os objetos que escutam determinado evento.
        /// </summary>
        [DebuggerStepThrough]
        public virtual IEnumerable<IDomainEventHandler<TEvent>> GetAllHandlers<TEvent>() 
            where TEvent : IDomainEvent 
            => RegisteredHandlers.OfType<IDomainEventHandler<TEvent>>();
    }
}
