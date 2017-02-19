using System;
using System.Linq;
using System.Threading.Tasks.Dataflow;

namespace PositiveTechnologies
{
    public class FibonacciSequencesManager
    {
        public int Count { get; private set; }

        public FibonacciSequencesManager(int count) { Count = count; }

        private readonly BufferBlock<UpdateEvent> _outPort = new BufferBlock<UpdateEvent>();
        public ISourceBlock<UpdateEvent> OutPort { get { return _outPort; } }

        private readonly BufferBlock<UpdateEvent> _inPort = new BufferBlock<UpdateEvent>();
        public ITargetBlock<UpdateEvent> InPort { get { return _inPort; } }

        public void Init()
        {
            foreach (var seqId in Enumerable.Range(1, Count))
                LinkUpdateSequenceBlock(_outPort, seqId);

            LinkAddNewSequencerBlock(CreateAddNewSequencerBlock());

            foreach (var seqId in Enumerable.Range(1, Count))
                _outPort.Post(new UpdateEvent { SequenceId = seqId, State = 0 });
        }

        private void LinkAddNewSequencerBlock(ITargetBlock<UpdateEvent> addNewSequencerBlock)
        {
            _inPort.LinkTo(addNewSequencerBlock, new DataflowLinkOptions {MaxMessages = 1}, ue => ue.State == 0);
        }

        private void LinkUpdateSequenceBlock(ITargetBlock<UpdateEvent> updateSequenceBlock, int seqId)
        {
            _inPort.LinkTo(updateSequenceBlock, ue => ue.SequenceId == seqId);
        }

        private ActionBlock<UpdateEvent> CreateAddNewSequencerBlock()
        {
            ActionBlock<UpdateEvent> addNewSequencer = null;
            addNewSequencer = new ActionBlock<UpdateEvent>(ue =>
            {
                LinkUpdateSequenceBlock(_outPort, ue.SequenceId);
                Count++;

                LinkAddNewSequencerBlock(addNewSequencer);
                _outPort.Post(new UpdateEvent { SequenceId = ue.SequenceId, State = 1 });
            });
            return addNewSequencer;
        }

        public struct UpdateEvent
        {
            public int SequenceId { get; set; }
            public int State { get; set; }

            public override string ToString()
            {
                return String.Format("Sequence {0}, state - {1}", SequenceId, State);
            }
        }
    }
}