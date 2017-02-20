namespace PositiveTechnologies
{
    public struct FibonacciState
    {
        public long Value { get; private set; }

        public FibonacciState(long value)
            : this()
        {
            Value = value;
        }

        public FibonacciState Next(FibonacciState prev)
        {
            return new FibonacciState(prev.Value + Value);
        }
    }

    public interface IFibonacciSequence
    {
        FibonacciState Previous { get; }
        FibonacciState Next(FibonacciState current);
    }

    public class FibonacciSkipOneSequence : IFibonacciSequence
    {
        public FibonacciState Previous { get; private set; }

        public FibonacciSkipOneSequence(FibonacciState previous)
        {
            Previous = previous;
        }

        public FibonacciState Next(FibonacciState current)
        {
            return Previous = Previous.Next(current);
        }
    }

    public class FibonacciSequence : IFibonacciSequence
    {
        public FibonacciState Previous { get; private set; }

        public FibonacciSequence(FibonacciState previous)
        {
            Previous = previous;
        }

        public FibonacciState Next(FibonacciState current)
        {
            var next = Previous.Next(current);
            Previous = current;
            return next;
        }
    }
}
