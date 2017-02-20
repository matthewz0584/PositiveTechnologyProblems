using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks.Dataflow;
using NSubstitute;
using NUnit.Framework;

namespace PositiveTechnologiesProblems.Domain.Tests
{
    public class FibonacciDistributedSequencesManagerTests
    {
        readonly FibonacciState FS = new FibonacciState(0);
        readonly FibonacciState SS = new FibonacciState(1);

        [Test]
        public void Init()
        {
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            var fcm = new FibonacciDistributedSequencesManager(2, sequencerCreator, FS, SS);

            fcm.Init();
            
            Assert.That(fcm.Count, Is.EqualTo(2));

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 1, State = FS }));
            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = FS }));
            sequencerCreator.Received(2).Invoke(new FibonacciState(0));

            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 1, State = SS });
            Assert.That(fcm.OutPort.ReceiveWithTimeout().SequenceId, Is.EqualTo(1));
            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = SS });
            Assert.That(fcm.OutPort.ReceiveWithTimeout().SequenceId, Is.EqualTo(2));
            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 3, State = SS });
            Assert.Throws<TimeoutException>(() => fcm.OutPort.ReceiveWithTimeout());
        }

        [Test]
        public void InitMessageHandling()
        {
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            var fcm = new FibonacciDistributedSequencesManager(0, sequencerCreator, FS, SS);
            fcm.Init();

            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 5, State = FS });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 5, State = SS }));
            Assert.That(fcm.Count, Is.EqualTo(1));
            sequencerCreator.Received().Invoke(new FibonacciState(1));

            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = FS });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = SS }));
            Assert.That(fcm.Count, Is.EqualTo(2));
            sequencerCreator.Received(2).Invoke(new FibonacciState(1));
        }

        [Test]
        [Ignore("Not implemented")]
        public void InitMessageHandling_AlreadyInited()
        {
        }

        [Test]
        public void UpdateMessageHandling()
        {
            var sequencer1 = Substitute.For<IFibonacciSequence>();
            sequencer1.Next(Arg.Any<FibonacciState>()).Returns(new FibonacciState(100));
            var sequencer2 = Substitute.For<IFibonacciSequence>();
            sequencer2.Next(Arg.Any<FibonacciState>()).Returns(new FibonacciState(1000));
            
            var sequencerCreator = Substitute.For<Func<FibonacciState, IFibonacciSequence>>();
            sequencerCreator.Invoke(Arg.Any<FibonacciState>()).Returns(sequencer1, sequencer2);

            var fcm = new FibonacciDistributedSequencesManager(2, sequencerCreator, FS, SS);
            fcm.Init();

            //Flush initial messages
            fcm.OutPort.Receive();
            fcm.OutPort.Receive();

            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 1, State = new FibonacciState(50) });
            
            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 1, State = new FibonacciState(100) }));
            sequencer1.Received().Next(new FibonacciState(50));

            fcm.InPort.Post(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = new FibonacciState(500) });

            Assert.That(fcm.OutPort.ReceiveWithTimeout(), Is.EqualTo(new FibonacciDistributedSequencesManager.UpdateMessage { SequenceId = 2, State = new FibonacciState(1000) }));
            sequencer2.Received().Next(new FibonacciState(500));
        }

        [Test]
        [Ignore("Not implemented")]
        public void UpdateMessageHandling_UninitializedSequence()
        {
        }

        [Test]
        public void Integration()
        {
            Func<FibonacciState, IFibonacciSequence> seqCreator = s => new FibonacciSkipOneSequence(s);
            var agent1 = new FibonacciDistributedSequencesManager(2, seqCreator, FS, SS);
            var agent2 = new FibonacciDistributedSequencesManager(0, seqCreator, FS, SS);

            var messageLog = new List<FibonacciDistributedSequencesManager.UpdateMessage>();

            var logBlock1 = new TransformBlock<FibonacciDistributedSequencesManager.UpdateMessage, FibonacciDistributedSequencesManager.UpdateMessage>(
                ue => { lock (messageLog) {
                        messageLog.Add(ue); return ue;
                    }
                });
            agent1.OutPort.LinkTo(logBlock1);
            logBlock1.LinkTo(agent2.InPort);

            var logBlock2 = new TransformBlock<FibonacciDistributedSequencesManager.UpdateMessage, FibonacciDistributedSequencesManager.UpdateMessage>(
                ue => { lock (messageLog) {
                        messageLog.Add(ue); return ue;
                    }
                });
            agent2.OutPort.LinkTo(logBlock2);
            logBlock2.LinkTo(agent1.InPort);

            agent1.Init();
            agent2.Init();

            Thread.Sleep(4000);

            Assert.That(messageLog.ToArray().Where(ue => ue.SequenceId == 1).Select(ue => ue.State.Value).Take(8).ToList(),
                Is.EqualTo(new[] {0, 1, 1, 2, 3, 5, 8, 13}));
            Assert.That(messageLog.ToArray().Where(ue => ue.SequenceId == 2).Select(ue => ue.State.Value).Take(8).ToList(),
                Is.EqualTo(new[] {0, 1, 1, 2, 3, 5, 8, 13}));
        }
    }
}