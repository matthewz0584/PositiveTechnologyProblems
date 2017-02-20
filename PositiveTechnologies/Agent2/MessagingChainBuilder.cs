using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using log4net;
using PositiveTechnologies;

namespace Agent2
{
    public interface IOutboundTransport
    {
        Task Send(UpdateMessageDto um);
    }

    public class UpdateMessageDto
    {
        public int SequenceId { get; set; }
        public long State { get; set; }
    }

    public class MessagingChainBuilder
    {
        public IOutboundTransport OutboundTransport { get; private set; }
        public ILog Log { get; private set; }

        public MessagingChainBuilder(IOutboundTransport outboundTransport, ILog log)
        {
            OutboundTransport = outboundTransport;
            Log = log;
        }

        public ITargetBlock<UpdateMessageDto> BuildMessagingChain(IFibonacciDistributedSequencesManager fdsm)
        {
            var unpacker = CreateUnpackerBlock();
            var logger = CreateLoggerBlock("Outgoing {0}");
            var packer = CreatePackerBlock();
            var sender = CreateSenderBlock();
            unpacker.LinkTo(fdsm.InPort);
            fdsm.OutPort.LinkTo(logger);
            logger.LinkTo(packer);
            packer.LinkTo(sender);
            return unpacker;
        }

        public IPropagatorBlock<UpdateMessageDto, FibonacciDistributedSequencesManager.UpdateMessage> CreateUnpackerBlock()
        {
            return new TransformBlock<UpdateMessageDto, FibonacciDistributedSequencesManager.UpdateMessage>(umd =>
                new FibonacciDistributedSequencesManager.UpdateMessage
                {
                    SequenceId = umd.SequenceId,
                    State = new FibonacciState(umd.State)
                });
        }

        public IPropagatorBlock<FibonacciDistributedSequencesManager.UpdateMessage, UpdateMessageDto> CreatePackerBlock()
        {
            return new TransformBlock<FibonacciDistributedSequencesManager.UpdateMessage, UpdateMessageDto>(um =>
                new UpdateMessageDto { SequenceId = um.SequenceId, State = um.State.Value });
        }

        public IPropagatorBlock<FibonacciDistributedSequencesManager.UpdateMessage, FibonacciDistributedSequencesManager.UpdateMessage> CreateLoggerBlock(string text)
        {
            return new TransformBlock<FibonacciDistributedSequencesManager.UpdateMessage, FibonacciDistributedSequencesManager.UpdateMessage>(
                um => { Log.Info(String.Format(text, um)); return um; });
        }

        public ITargetBlock<UpdateMessageDto> CreateSenderBlock()
        {
            return new ActionBlock<UpdateMessageDto>(um => OutboundTransport.Send(um));
        }
    }
}