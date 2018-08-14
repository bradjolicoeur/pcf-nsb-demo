using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace pcf_nsb_server
{
    class Program
    {
        // AutoResetEvent to signal when to exit the application.
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        static async Task Main()
        {
            Console.Title = "Samples.FullDuplex.Server";
            LogManager.Use<DefaultFactory>()
                .Level(LogLevel.Info);
            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Server");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.UseTransport<LearningTransport>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
               .ConfigureAwait(false);

            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key != ConsoleKey.Enter)
                {
                    break;
                }
            }


            // Handle Control+C or Control+Break
            Console.CancelKeyPress += async (o, e) =>
            {
                Console.WriteLine("Exit");

                await endpointInstance.Stop()
                .ConfigureAwait(false);

                // Allow the manin thread to continue and exit...
                waitHandle.Set();
            };

            // Wait
            waitHandle.WaitOne();

        }
    }
}
