using System;
using System.Transactions;

namespace Miris.DomainEvents.Extensions
{
    public static class TransactionExtensions
    {
        public static void OnPhase0(this Transaction transaction, Action action)
        {
            if (transaction == null) throw new ArgumentNullException(nameof(transaction));
            if (action == null) throw new ArgumentNullException(nameof(action));

            transaction.EnlistVolatile(
                new AnonymousResourceManager(onPrepare: action),
                EnlistmentOptions.EnlistDuringPrepareRequired);
        }
    }
}
