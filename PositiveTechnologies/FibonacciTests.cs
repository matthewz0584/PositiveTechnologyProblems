using System;
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
}