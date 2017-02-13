using System;
using System.Transactions;

namespace Miris.DomainEvents.ImmediateConsistency
{
    internal static class TransactionExtensions
    {

        public static TransactionId GetId(
            this Transaction transaction)
        {
            if (transaction == null)
                throw new ArgumentNullException(nameof(transaction));

            return new TransactionId(transaction);
        }

    }
}
