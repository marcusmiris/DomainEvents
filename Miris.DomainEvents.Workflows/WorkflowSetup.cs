using System.Reflection;

namespace Miris.DomainEvents.Workflows
{
    public static class WorkflowSetup
    {

        public static void RegisterAll(params IWorkflow[] workflows)
        {
            foreach (var w in workflows) w.Setup();
        }

        public static WorkflowContext<TEvent> When<TEvent>() where TEvent : IDomainEvent => new();

        public static WorkflowContext<TEvent> And<TEvent>(this WorkflowContext<TEvent> onEvent, Predicate<TEvent> predicate)
            where TEvent : IDomainEvent
        {
            onEvent.Conditions.Add(predicate);
            return onEvent;
        }

        public static void Then<TEvent>(this WorkflowContext<TEvent> onEvent, Action<TEvent> callback) where TEvent : IDomainEvent
        {
            DomainEvents.Register<TEvent>(e =>
            {
                if (!onEvent.Conditions.All(p => p(e))) return;
                callback(e);
            });
        }

        #region ' inner classes '

        public class WorkflowContext<TEvent> where TEvent : IDomainEvent
        {
            internal WorkflowContext() { }

            public List<Predicate<TEvent>> Conditions { get; } = new List<Predicate<TEvent>>();
        }


        #endregion
    }
}
