using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Transactions;
using Miris.DomainEvents.Extensions;

namespace Miris.DomainEvents.ImmediateConsistency
{

    /// <summary>
    ///     Queue dos eventos ocorridos dentro do escopo de uma transação.
    /// </summary>
    internal class EventQueue
    {

        #region ' members '

        /// <summary>
        ///     Ao invés de salvar a lista de eventos, aqui estão 
        ///     sendo salvos uma lista de delegates a serem invocados, um
        ///     para cada handler.
        /// </summary>
        private Queue<Action> _handlers;

        private readonly Transaction _transactionToEnlistIn;
        private bool _enlistedInTransaction = false;

        #endregion

        #region ' ctor '

        public EventQueue(Transaction transactionToEnlistIn)
        {
            _transactionToEnlistIn = transactionToEnlistIn;
        }

        #endregion

        public void Enqueue<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            if (!_enlistedInTransaction) this.SetupTransactionEnlistment(_transactionToEnlistIn);
            
            var handlers = _handlers ?? (_handlers = new Queue<Action>());
            handlers.Enqueue(delegate
            {
                // invoca cada handler.
                DomainEvents.HandlersRegistry
                    .GetValueOrThrowsException(@"Domain Events Handles Registry não está configurado corretamente.")    // garante que o registry está devidamente configurado.
                    .GetAllHandlers<TEvent>()
                    .ForEach(handler => handler.Handle(@event));
            });
        }

        #region ' private methods '

        private void SetupTransactionEnlistment(Transaction transaction)
        {
            // Phase0 Enlistment, uma vez que Event Handlers podem abrir novas sub-transações.
            // Mais informações em https://blogs.msdn.microsoft.com/florinlazar/2006/01/29/msdtc-the-magic-of-phase-zero-phase0-or-when-using-2pc-transactions-is-not-enough/
            transaction.OnPhase0(DefferEventHandlersExecution);
            _enlistedInTransaction = true;
        }

        private void ExecuteEventHandler(Action handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            using (var trans = new TransactionScope(_transactionToEnlistIn, TransactionScopeAsyncFlowOption.Enabled))
            {
                handler.Invoke();
                trans.Complete();
            }
        }

        /// <summary>
        ///     Atrasa o processamento dos eventos, jogando-os para o final do processamento da Phase0 da transação.
        /// </summary>
        /// <remarks>
        ///     A proposta deste método é tentar fazer com que o processamento dos eventos sejam uma das últimas coisas
        ///     a serem processadas dentro da transação. A necessidade de tal comportamento foi percebida quando um handler
        ///     de um evento tentava realizar Dirty Reads, porém o commit da operação principal não tinha sido realizado porque
        ///     o event queue havia sido alistado na transação antes do repositorio.
        /// </remarks>
        private void DefferEventHandlersExecution()
        {
            if (!_handlers.Any()) return;

            _transactionToEnlistIn.OnPhase0(() =>
            {
                if (!_handlers.Any()) return;

                // invoca o primeiro handler (head).
                ExecuteEventHandler(_handlers.Dequeue());

                // se ainda houver outros handlers (tail), posterga-os mais uma vez
                // com o fim de que outros resource managers apensados à transação pelo último handler possam ser processado.
                if (_handlers.Any()) DefferEventHandlersExecution();
            });
        }

        #endregion

    }

}
