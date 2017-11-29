using System;
using System.Threading.Tasks;
using System.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Miris.DomainEvents.ImmediateConsistency;

namespace Miris.DomainEvents.Tests
{
    [TestClass]
    public class BasicTests
    {

        public class EventStub : IDomainEvent
        {
            public Guid Guid { get; set; }
        }


        [TestMethod]
        public void FuncionaTest()
        {

            var expected = Guid.NewGuid();
            var escutou = new TaskCompletionSource<bool>();

            // registra handler
            DomainEvents.Register<EventStub>(e => escutou.SetResult(e.Guid == expected));

            // levanta evento teste
            using (var trans = new TransactionScope()) // componente requer que haja ambiente transacional estabelecido.
            {
                DomainEvents.Raise(new EventStub() { Guid = expected });
                trans.Complete();
            }

            // aguarda execução do handler.
            var timeout = !escutou.Task.Wait(TimeSpan.FromSeconds(5));

            // Verificações
            Assert.IsFalse(timeout, @"Timeout");
            Assert.AreEqual(true, escutou.Task.Result, @"Valor inválido ao escutar evento.");

        }
    }
}
