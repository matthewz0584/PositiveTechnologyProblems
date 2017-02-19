using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using NSubstitute;
using NUnit.Framework;

namespace PositiveTechnologies
{
    public class FibonacciSequencesManagerTests
    {
        [Test]
        public void Init()
        {
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            var fcm = new FibonacciSequencesManager(2, sequencerCreator);

            fcm.Init();
            
            Assert.That(fcm.Count, Is.EqualTo(2));

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 1, State = 0 }));
            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 0 }));
            sequencerCreator.Received(2).Invoke(new FibonacciState(0));

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
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            var fcm = new FibonacciSequencesManager(0, sequencerCreator);
            fcm.Init();

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 5, State = 0 });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 5, State = 1 }));
            Assert.That(fcm.Count, Is.EqualTo(1));
            sequencerCreator.Received().Invoke(new FibonacciState(1));

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 0 });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 1 }));
            Assert.That(fcm.Count, Is.EqualTo(2));
            sequencerCreator.Received(2).Invoke(new FibonacciState(1));
        }

        [Test]
        [Ignore("Not implemented")]
        public void InitEventHandling_AlreadyInited()
        {
        }

        [Test]
        public void UpdateEventHandling()
        {
            var sequencer1 = Substitute.For<IFibonacciSequence>();
            sequencer1.Next(Arg.Any<FibonacciState>()).Returns(new FibonacciState(100));
            var sequencer2 = Substitute.For<IFibonacciSequence>();
            sequencer2.Next(Arg.Any<FibonacciState>()).Returns(new FibonacciState(1000));
            
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            sequencerCreator.Invoke(Arg.Any<FibonacciState>()).Returns(sequencer1, sequencer2);
                
            var fcm = new FibonacciSequencesManager(2, sequencerCreator);
            fcm.Init();

            //Flush initial messages
            fcm.OutPort.Receive();
            fcm.OutPort.Receive();

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 1, State = 50 });
            
            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 1, State = 100 }));
            sequencer1.Received().Next(new FibonacciState(50));

            fcm.InPort.Post(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 500 });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciSequencesManager.UpdateEvent { SequenceId = 2, State = 1000 }));
            sequencer2.Received().Next(new FibonacciState(500));
        }

        [Test]
        [Ignore("Not implemented")]
        public void UpdateEventHandling_UninitializedSequence()
        {
        }

        [Test]
        public void Integration()
        {
            Func<FibonacciState, IFibonacciSequence> seqCreator = s => new FibonacciSkipOneSequence(s);
            var agent1 = new FibonacciSequencesManager(2, seqCreator);
            var agent2 = new FibonacciSequencesManager(0, seqCreator);

            var messageLog = new List<FibonacciSequencesManager.UpdateEvent>();

            var logBlock1 = new TransformBlock<FibonacciSequencesManager.UpdateEvent, FibonacciSequencesManager.UpdateEvent>(
                ue => { lock (messageLog) {
                        messageLog.Add(ue); return ue;
                    }
                });
            agent1.OutPort.LinkTo(logBlock1);
            logBlock1.LinkTo(agent2.InPort);

            var logBlock2 = new TransformBlock<FibonacciSequencesManager.UpdateEvent, FibonacciSequencesManager.UpdateEvent>(
                ue => { lock (messageLog) {
                        messageLog.Add(ue); return ue;
                    }
                });
            agent2.OutPort.LinkTo(logBlock2);
            logBlock2.LinkTo(agent1.InPort);

            agent1.Init();
            agent2.Init();

            Thread.Sleep(100);

            Assert.That(messageLog.ToArray().Where(ue => ue.SequenceId == 1).Select(ue => ue.State).Take(10).ToList(),
                Is.EqualTo(new[] {0, 1, 1, 2, 3, 5, 8, 13, 21, 34}));
            Assert.That(messageLog.ToArray().Where(ue => ue.SequenceId == 2).Select(ue => ue.State).Take(10).ToList(),
                Is.EqualTo(new[] {0, 1, 1, 2, 3, 5, 8, 13, 21, 34}));
        }
    }
}