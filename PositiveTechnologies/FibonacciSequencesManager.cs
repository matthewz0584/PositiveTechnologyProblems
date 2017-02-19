using System.Threading.Tasks.Dataflow;

namespace PositiveTechnologies
{
    public class FibonacciSequencesManager
    {
        public int Count { get; private set; }

        public FibonacciSequencesManager(int count)
        {
            Count = count;
        }

        private readonly BufferBlock<UpdateEvent> _outPort = new BufferBlock<UpdateEvent>();
        public ISourceBlock<UpdateEvent> OutPort { get { return _outPort; } }

        private readonly BufferBlock<UpdateEvent> _inPort = new BufferBlock<UpdateEvent>();
        public ITargetBlock<UpdateEvent> InPort { get { return _inPort; } }

        public void Init()
        {
            for (var i = 1; i < Count + 1; ++i)
                _outPort.Post(new UpdateEvent { SequenceId = i, State = 0});
        }

        public struct UpdateEvent
        {
            public int SequenceId { get; set; }
            public int State { get; set; }
        }
    }
}