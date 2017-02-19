using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace PositiveTechnologies
{
    public class FibonacciSequencesManager
    {
        public int Count { get; private set; }
        public Func<FibonacciState, IFibonacciSequence> SequencerCreator { get; private set; }
        public FibonacciState FirstState { get; private set; }
        public FibonacciState SecondState { get; private set; }

        public FibonacciSequencesManager(int count, Func<FibonacciState, IFibonacciSequence> sequencerCreator, FibonacciState firstState, FibonacciState secondState)
        {
            Count = count;
            SequencerCreator = sequencerCreator;
            FirstState = firstState;
            SecondState = secondState;
        }

        private readonly BufferBlock<UpdateMessage> _outPort = new BufferBlock<UpdateMessage>();
        public ISourceBlock<UpdateMessage> OutPort { get { return _outPort; } }

        private readonly BufferBlock<UpdateMessage> _inPort = new BufferBlock<UpdateMessage>();
        public ITargetBlock<UpdateMessage> InPort { get { return _inPort; } }

        public void Init()
        {
            foreach (var seqId in Enumerable.Range(1, Count))
                LinkUpdateSequenceBlock(CreateUpdateSequenceBlock(FirstState), seqId);

            LinkAddNewSequencerBlock(CreateAddNewSequencerBlock());

            foreach (var seqId in Enumerable.Range(1, Count))
                _outPort.Post(new UpdateMessage { SequenceId = seqId, State = FirstState });
        }

        private void LinkAddNewSequencerBlock(ITargetBlock<UpdateMessage> addNewSequencerBlock)
        {
            _inPort.LinkTo(addNewSequencerBlock, new DataflowLinkOptions { MaxMessages = 1 }, um => um.State.Equals(FirstState));
        }

        private void LinkUpdateSequenceBlock(IPropagatorBlock<UpdateMessage, UpdateMessage> updateSequenceBlock, int seqId)
        {
            _inPort.LinkTo(updateSequenceBlock, um => um.SequenceId == seqId);
            updateSequenceBlock.LinkTo(_outPort);
        }

        private TransformBlock<UpdateMessage, UpdateMessage> CreateUpdateSequenceBlock(FibonacciState state)
        {
            var sequencer = SequencerCreator(state);
            return new TransformBlock<UpdateMessage, UpdateMessage>(um => new UpdateMessage
                {
                    SequenceId = um.SequenceId,
                    State = sequencer.Next(um.State)
                });
        }

        private ActionBlock<UpdateMessage> CreateAddNewSequencerBlock()
        {
            ActionBlock<UpdateMessage> addNewSequencer = null;
            addNewSequencer = new ActionBlock<UpdateMessage>(um =>
            {
                LinkUpdateSequenceBlock(CreateUpdateSequenceBlock(SecondState), um.SequenceId);
                Count++;

                LinkAddNewSequencerBlock(addNewSequencer);
                _outPort.Post(new UpdateMessage { SequenceId = um.SequenceId, State = SecondState });
            });
            return addNewSequencer;
        }

        public struct UpdateMessage
        {
            public int SequenceId { get; set; }
            public FibonacciState State { get; set; }

            public override string ToString()
            {
                return String.Format("Sequence {0}, state - {1}", SequenceId, State.Value);
            }
        }
    }
}