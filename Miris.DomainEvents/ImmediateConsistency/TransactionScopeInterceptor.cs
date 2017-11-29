using System;
using System.Linq;
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
            // determina se deve ou não criar o escopo transactional
            var escopoTransacionalDeveSerCriado = ((Func<bool>)(() =>
           {
               var enabledByAttribute = ((Func<bool>)(() =>
               {
                   var implicitScopeConfig = (ImplicitTransactionScopeAttribute)invocation.MethodInvocationTarget.GetCustomAttributes(typeof(ImplicitTransactionScopeAttribute), inherit: true).SingleOrDefault();
                   return implicitScopeConfig == null || implicitScopeConfig.Criar;
               }))();

               var existeEscopoTransacional = Transaction.Current != null;

               //var retornaVoidResponse = typeof(VoidResponse).IsAssignableFrom(invocation.Method.ReturnType);

               return !existeEscopoTransacional
                   //&& retornaVoidResponse          // Presume que métodos que não retornam VoidResponse não necessitam de escopo transacional.
                   && enabledByAttribute
                   ;
           }))();

            if (!escopoTransacionalDeveSerCriado)
                invocation.Proceed();
            else

                try
                {
                    using (var trans = new TransactionScope(
                        TransactionScopeOption.Required,
                        new TransactionOptions()
                        {
                            //IsolationLevel = IsolationLevel.Snapshot,   // https://blogs.msdn.microsoft.com/diego/2012/03/31/tips-to-avoid-deadlocks-in-entity-framework-applications/
                            Timeout = TimeSpan.FromDays(1)
                        },
                        TransactionScopeAsyncFlowOption.Enabled))
                    {
                        invocation.Proceed();

                        //if (invocation.ReturnValue is VoidResponse response && !response.HasError)
                            trans.Complete();
                    }
                }
                catch (TransactionAbortedException e)
                {
                    throw e.InnerException ?? e;
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
