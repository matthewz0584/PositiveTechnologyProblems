using System.Threading.Tasks.Dataflow;
using NUnit.Framework;

namespace PositiveTechnologies
{
    public class TplDataFlowTests
    {
        [Test]
        public void NewlyAddedSourceConsumesOldMessage()
        {
            var provider = new BufferBlock<int>();

            provider.Post(2);

            Assert.That(provider.Count, Is.EqualTo(1));

            var consumer = new BufferBlock<int>();
            provider.LinkTo(consumer);

            Assert.That(consumer.Receive(), Is.EqualTo(2));
            Assert.That(consumer.Count, Is.EqualTo(0));
            Assert.That(provider.Count, Is.EqualTo(0));
        }

        [Test]
        public void TargetFiltersMessages()
        {
            var provider = new BufferBlock<int>();

            var consumerEven = new BufferBlock<int>();
            provider.LinkTo(consumerEven, v => v % 2 == 0);

            var consumerOdd = new BufferBlock<int>();
            provider.LinkTo(consumerOdd, v => v % 2 == 1);

            provider.Post(2);
            provider.Post(1);

            Assert.That(consumerEven.Receive(), Is.EqualTo(2));
            Assert.That(consumerEven.Count, Is.EqualTo(0));

            Assert.That(consumerOdd.Receive(), Is.EqualTo(1));
            Assert.That(consumerOdd.Count, Is.EqualTo(0));

            Assert.That(provider.Count, Is.EqualTo(0));
        }
    }
}