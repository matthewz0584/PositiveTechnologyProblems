namespace PositiveTechnologies
{
    public class FibonacciState
    {
        public int Value { get; private set; }

        public FibonacciState(int value)
        {
            Value = value;
        }

        public FibonacciState Next(FibonacciState prev)
        {
            return new FibonacciState(prev.Value + Value);
        }
    }

    public class FibonacciSequence
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