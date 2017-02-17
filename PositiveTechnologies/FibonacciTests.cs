using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace PositiveTechnologies
{
    public class FibonacciTests
    {
        [Test]
        public void FibonacciState_Next()
        {
            Assert.That(new FibonacciState(1).Next(new FibonacciState(0)).Value, Is.EqualTo(1));
            Assert.That(new FibonacciState(2).Next(new FibonacciState(1)).Value, Is.EqualTo(3));
        }

        [Test]
        public void FibonacciSequence_Next()
        {
            var fs = new FibonacciSequence(new FibonacciState(0));

            Assert.That(fs.Next(fs.Next(fs.Next(fs.Next(new FibonacciState(1))))).Value, Is.EqualTo(5));
            Assert.That(fs.Previous.Value, Is.EqualTo(3));
        }
    }

    public class FibonacciSequencesManagerTests
    {
        [Test]
        public void Init_Pull()
        {
            var fcm = new FibonacciSequencesManager(2);

            fcm.Init();

            Assert.That(fcm.Count, Is.EqualTo(2));
            Assert.That(fcm.State(1), Is.EqualTo(0));
            Assert.That(fcm.State(2), Is.EqualTo(0));

            Assert.That(fcm.OutPort.Receive(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { Id = 1, State = 0 }));
            Assert.That(fcm.OutPort.Receive(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { Id = 2, State = 0 }));
        }

        //[Test]
        //public void Init_Push()
        //{
        //    var fcm = new FibonacciSequencesManager(0);

        //    fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { Id = 1, State = 0 });

        //    Assert.That(fcm.OutPort.Receive(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { Id = 1, State = 0 }));
        //    Assert.That(fcm.OutPort.Receive(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { Id = 2, State = 0 }));
        //}
    }

    public class FibonacciSequencesManager
    {
        private readonly BufferBlock<UpdateEvent> _outPort = new BufferBlock<UpdateEvent>();
        public int Count { get; private set; }

        public FibonacciSequencesManager(int count)
        {
            Count = count;
        }

        public ISourceBlock<UpdateEvent> OutPort
        {
            get { return _outPort; }
        }

        public void Init()
        {
            for (var i = 1; i < Count + 1; ++i)
                _outPort.Post(new UpdateEvent { Id = i, State = 0});
        }

        public struct UpdateEvent
        {
            public int Id { get; set; }
            public int State { get; set; }
        }
    }
}