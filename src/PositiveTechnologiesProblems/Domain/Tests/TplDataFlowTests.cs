using System;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace PositiveTechnologiesProblems.Domain.Tests
{
    public class TplDataFlowTests
    {
        [Test]
        public async Task NewlyAddedSourceConsumesOldMessage()
        {
            var provider = new BufferBlock<int>();

            provider.Post(2);

            Assert.That(provider.Count, Is.EqualTo(1));

            var consumer = new BufferBlock<int>();
            provider.LinkTo(consumer, new DataflowLinkOptions { PropagateCompletion = true });
            provider.Complete();

            Assert.That(consumer.ReceiveWithTimeout(), Is.EqualTo(2));

            await consumer.Completion;
            Assert.That(consumer.Count, Is.EqualTo(0));
            Assert.That(provider.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task TargetFiltersMessages()
        {
            var provider = new BufferBlock<int>();

            var consumerEven = new BufferBlock<int>();
            provider.LinkTo(consumerEven, new DataflowLinkOptions {PropagateCompletion = true}, v => v % 2 == 0);

            var consumerOdd = new BufferBlock<int>();
            provider.LinkTo(consumerOdd, new DataflowLinkOptions { PropagateCompletion = true }, v => v % 2 == 1);

            provider.Post(2);
            provider.Post(1);
            provider.Complete();

            Assert.That(consumerEven.ReceiveWithTimeout(), Is.EqualTo(2));
            Assert.That(consumerOdd.ReceiveWithTimeout(), Is.EqualTo(1));
            
            await Task.WhenAll(consumerEven.Completion, consumerOdd.Completion);
            Assert.That(consumerEven.Count, Is.EqualTo(0));
            Assert.That(consumerOdd.Count, Is.EqualTo(0));
            Assert.That(provider.Count, Is.EqualTo(0));
        }
    }

    public static class DataFlowBlockTesting
    {
        public static T ReceiveWithTimeout<T>(this ISourceBlock<T> block)
        {
            return block.Receive(TimeSpan.FromMilliseconds(1000));
        }
    }
}