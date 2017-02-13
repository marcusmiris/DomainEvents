namespace Miris.DomainEvents.Dispatching
{
    /// <summary>
    ///     Representa um objeto que sabe para onde os eventos devem ser delegados.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        void Submit<TEvento>(TEvento domainEvent) where TEvento : IDomainEvent;
    }
}
