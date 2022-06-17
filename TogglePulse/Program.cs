using Meadow;
using System.Threading;

namespace TogglePulse
{
    internal class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug") return;

            // instantiate and run new meadow app
            app = new TogglePulseApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
