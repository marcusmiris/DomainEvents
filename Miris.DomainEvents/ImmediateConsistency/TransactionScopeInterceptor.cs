using System;
using System.Transactions;
using Castle.DynamicProxy;

namespace Miris.DomainEvents.ImmediateConsistency
{
    public class TransactionScopeInterceptor
        : IInterceptor
    {

        #region ' IInterceptor'

        public void Intercept(IInvocation invocation)
        {
            if (Transaction.Current != null)
                invocation.Proceed();

            else

                using (var trans = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
                {
                    invocation.Proceed();

                    trans.Complete();
                }
        }

        #endregion

        /// <summary>
        ///     Set <see cref="TransactionScopeInterceptor"/> interception on <see cref="TCandidate"/>.
        /// </summary>
        /// <typeparam name="TCandidate">
        ///     Interface or class to be implemented.
        ///     Tip: Prefer interface over class interception. 
        ///     Class interception requires virtual methods declaration.
        /// </typeparam>
        /// <param name="candidate">
        ///     Wrapped instance.
        /// </param>
        /// <returns>
        ///     Proxied version of <see cref="TCandidate"/> with <see cref="TransactionScopeInterceptor"/> interception.
        /// </returns>
        public static TCandidate SetOn<TCandidate>(
            TCandidate candidate)
            where TCandidate : class
        {
            var proxyGenerator = new Castle.DynamicProxy.ProxyGenerator();

            // determina factory
            var proxyFactory = typeof(TCandidate).IsInterface
                    ? (Func<TCandidate, IInterceptor[], TCandidate>)proxyGenerator.CreateInterfaceProxyWithTarget
                    : (Func<TCandidate, IInterceptor[], TCandidate>)proxyGenerator.CreateClassProxyWithTarget;

            // cria e retorna proxy.
            return proxyFactory.Invoke(
                candidate,
                new IInterceptor[]
                {
                    new TransactionScopeInterceptor()
                });

        }
    }
}
