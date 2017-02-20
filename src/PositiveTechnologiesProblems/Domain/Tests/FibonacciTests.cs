using NUnit.Framework;

namespace PositiveTechnologiesProblems.Domain.Tests
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
        public void FibonacciSkipOneSequence_Next()
        {
            var fs1 = new FibonacciSkipOneSequence(new FibonacciState(0));
            var fs2 = new FibonacciSkipOneSequence(new FibonacciState(1));

            Assert.That(fs2.Next(fs1.Next(fs2.Next(fs1.Next(fs2.Previous)))).Value, Is.EqualTo(5));
            Assert.That(fs1.Previous.Value, Is.EqualTo(3));
            Assert.That(fs2.Previous.Value, Is.EqualTo(5));
        }
    }
}