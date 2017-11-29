using System;

namespace Miris.DomainEvents.ImmediateConsistency
{

    /// <summary>
    ///     Usado para especificar se <see cref="TransactionScopeInterceptor"/> vai criar um 
    ///     escopo transacional para o Proxied method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class ImplicitTransactionScopeAttribute
        : Attribute
    {

        public ImplicitTransactionScopeAttribute(bool criar)
        {
            Criar = criar;
        }

        public readonly bool Criar;


    }

}
