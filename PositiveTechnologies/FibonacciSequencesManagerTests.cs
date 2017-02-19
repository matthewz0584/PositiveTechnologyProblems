using System;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace PositiveTechnologies
{
    public class FibonacciSequencesManagerTests
    {
        [Test]
        public void Init()
        {
            var fcm = new FibonacciSequencesManager(2);

            fcm.Init();

            Assert.That(fcm.Count, Is.EqualTo(2));

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 1, State = 0 }));
            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 0 }));

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 1, State = 1 });
            Assert.That(fcm.OutPort.ReceiveWithTimeout().SequenceId, Is.EqualTo(1));
            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 1 });
            Assert.That(fcm.OutPort.ReceiveWithTimeout().SequenceId, Is.EqualTo(2));
            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 3, State = 1 });
            Assert.Throws<TimeoutException>(() => fcm.OutPort.ReceiveWithTimeout());
        }

        [Test]
        public void InitEventHandling()
        {
            var fcm = new FibonacciSequencesManager(0);
            fcm.Init();

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 5, State = 0 });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 5, State = 1 }));
            Assert.That(fcm.Count, Is.EqualTo(1));

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 0 });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 1 }));
            Assert.That(fcm.Count, Is.EqualTo(2));
        }

        [Test]
        [Ignore("Not implemented")]
        public void InitEventHandling_AlreadyInited()
        {
        }

        [Test]
        [Ignore("Not implemented")]
        public void UpdateEventHandling()
        {
        }

        [Test]
        [Ignore("Not implemented")]
        public void UpdateEvent_UninitializedSequences()
        {
            Assert.Fail();
        }
    }
}