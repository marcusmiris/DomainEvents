using System;
using System.Collections.Generic;
using System.Transactions;
using Miris.DomainEvents.Extensions;

namespace Miris.DomainEvents.ImmediateConsistency
{

    /// <summary>
    ///     Queue dos eventos ocorridos dentro do escopo de uma transação.
    /// </summary>
    internal class EventQueue
        : ISinglePhaseNotification
    {
        #region ' members '

        /// <summary>
        ///     Ao invés de salvar a lista de eventos, aqui estão 
        ///     sendo salvos uma lista de delegates a serem invocados, um
        ///     para cada handler.
        /// </summary>
        private Queue<Action> _handlers;

        #endregion

        public void Enqueue<TEvent>(TEvent @event) where TEvent : IDomainEvent
        {
            var handlers = _handlers ?? (_handlers = new Queue<Action>());

            handlers.Enqueue(() => DomainEvents.HandlersRegistry?
                .GetAllHandlers<TEvent>()
                .ForEach(handler => handler.Handle(@event)));
        }

        #region ' IEnlistmentNotification '

        void IEnlistmentNotification.Prepare(PreparingEnlistment preparingEnlistment)
        {
            try
            {
                DispatchEvents();
                preparingEnlistment.Prepared();
            }
            catch (TransactionException ex)
            {
                preparingEnlistment.ForceRollback(ex.InnerException);
            }
            catch (Exception ex)
            {
                preparingEnlistment.ForceRollback(ex);
            }
        }

        void IEnlistmentNotification.Commit(Enlistment enlistment)
            => enlistment.Done();

        void IEnlistmentNotification.Rollback(Enlistment enlistment)
            => enlistment.Done();

        void IEnlistmentNotification.InDoubt(Enlistment enlistment)
            => enlistment.Done();   // ToDo: deve ser implementado alguma coisa?

        #endregion

        #region ' ISinglePhaseNotification '

        void ISinglePhaseNotification.SinglePhaseCommit(SinglePhaseEnlistment singlePhaseEnlistment)
        {
            try
            {
                DispatchEvents();
                singlePhaseEnlistment.Committed();
            }
            catch (Exception ex)
            {
                singlePhaseEnlistment.Aborted(ex);
            }
        }

        #endregion

        #region ' private methods '

        /// <summary>
        ///     Invoca os handlers afim de processar os eventos.
        /// </summary>
        private void DispatchEvents()
        {
            using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                while (_handlers.Count > 0) _handlers.Dequeue().Invoke();
                trans.Complete();
            }
        }

        #endregion

    }

}
