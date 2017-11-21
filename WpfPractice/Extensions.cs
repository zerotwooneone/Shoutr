using System;
using System.Threading.Tasks;
using System.Windows;

namespace WpfPractice
{
    public static class Extensions
    {
        private static readonly Random Random = new Random();

        public static double NextDouble(this Random random, double minimum, double maximum)
        {
            if (minimum > maximum || minimum == maximum) throw new ArgumentException("minimum must be greater than maximum");
            return random.NextDouble() * (maximum - minimum) + minimum;
        }
        public static void Repeat(Action action, int times, double minSeconds, double maxSeconds)
        {
            int count = 0;

            void Func() => Task.Delay(TimeSpan.FromSeconds(Random.NextDouble(.7, 4)))
                .ContinueWith(t => Application.Current.Dispatcher.Invoke(action))
                .ContinueWith(t =>
                {
                    count++;
                    if (count < times)
                    {
                        Func();
                    }
                });

            Func();
        }
    }


}