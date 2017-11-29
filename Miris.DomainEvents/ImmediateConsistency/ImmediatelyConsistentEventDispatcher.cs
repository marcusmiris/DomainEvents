using System;
using System.Collections.Generic;
using System.Transactions;
using Miris.DomainEvents.Dispatching;

namespace Miris.DomainEvents.ImmediateConsistency
{
    /// <summary>
    ///     Implementação de <see cref="IDomainEventDispatcher"/> que
    ///     garante consistência imediata/forte no tratamento dos eventos.
    /// </summary>
    public class ImmediatelyConsistentEventDispatcher
        : IDomainEventDispatcher
    {
        #region ' IDomainEventDispatcher '

        public void Submit<TEvent>(TEvent domainEvent) where TEvent : IDomainEvent
        {
            var transaction = Transaction.Current;

            if (transaction == null )
                throw new TransactionException(@"Ambiente transacional não estabelecido.");

            GetEventQueueFor(transaction).Enqueue(domainEvent);
        }

        #endregion

        #region ' internals '

        /// <summary>
        ///     Mantem o registro de <see cref="EventQueue"/> 
        ///     por <see cref="Transaction"/> aberta.
        /// </summary>
        private static readonly Dictionary<TransactionId, EventQueue> TrackedQueues
            = new Dictionary<TransactionId, EventQueue>();

        /// <summary>
        ///     Retorna a <see cref="EventQueue"/> válida
        ///     para a transação atual.
        /// </summary>
        /// <exception cref="ArgumentNullException">
        ///     <see cref="transaction"/> is null.
        /// </exception>
        private static EventQueue GetEventQueueFor(Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var transId = transaction.GetId();

            // tenta encontrar uma queue existente.
            EventQueue eventQueue;
            TrackedQueues.TryGetValue(transId, out eventQueue);

            // retorna a existente ou uma nova.
            return eventQueue ?? NewTrackedQueue(transaction);
        }

        /// <summary>
        ///     Registra uma nova Queue para a transação informada.
        /// </summary>
        /// <returns>
        ///     A <see cref="EventQueue"/> associada à transação informada.
        /// </returns>
        private static EventQueue NewTrackedQueue(
            Transaction transaction)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));

            var transId = transaction.GetId();

            if (TrackedQueues.ContainsKey(transId))
                throw new InvalidOperationException(@"Transacion já registrada");

            // Remove queue quando a transação estiver completa.
            transaction.TransactionCompleted += (sender, args) => TrackedQueues.Remove(transId);

            // cria e retorna a queue.
            return TrackedQueues[transId] = new EventQueue(transaction);
        }


        #endregion

    }
}
