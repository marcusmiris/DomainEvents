namespace Miris.DomainEvents.Handlers
{
    public interface IDomainEventHandler<in TDomainEvent>
        : IDomainEventHandler
        where TDomainEvent : IDomainEvent
    {
        void Handle(TDomainEvent @event);

    }
}
