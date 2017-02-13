using System;
using System.Diagnostics;

namespace Miris.DomainEvents.Handlers
{

    /// <summary>
    ///     Permite o uso de uma <see cref="Action{T}"/>
    ///     como um handler de um evento.
    /// </summary>
    /// <typeparam name="T">
    ///     O tipo do evento a ser escutado.
    /// </typeparam>
    internal class DelegateAsEventHandlerAdapter<T>
        : IDomainEventHandler<T> where T : IDomainEvent
    {
        private readonly Action<T> _handler;

        #region ' Consctructor '

        public DelegateAsEventHandlerAdapter(Action<T> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            _handler = handler;
        }

        #endregion

        #region ' IDomainEventHandler '

        [DebuggerStepThrough]
        public void Handle(T @event) => _handler.Invoke(@event);

        #endregion
    }
}
