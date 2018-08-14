using NServiceBus;
using NServiceBus.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace pcf_nsb_client
{
    class Program
    {
        // AutoResetEvent to signal when to exit the application.
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        static async Task Main()
        {
            Console.Title = "Samples.FullDuplex.Client";
            LogManager.Use<DefaultFactory>()
                .Level(LogLevel.Info);
            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Client");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.UseTransport<LearningTransport>();

            var endpointInstance = await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
            Console.WriteLine("Press enter to send a message");
            Console.WriteLine("Press any key to exit");


            while (true)
            {
                var key = Console.ReadKey();
                Console.WriteLine();

                if (key.Key != ConsoleKey.Enter)
                {
                    break;
                }
                var guid = Guid.NewGuid();
                Console.WriteLine($"Requesting to get data by id: {guid:N}");

                var message = new RequestDataMessage
                {
                    DataId = guid,
                    String = "String property value"
                };
                await endpointInstance.Send("Samples.FullDuplex.Server", message)
                    .ConfigureAwait(false);

                // Sleep as long as you need.
                Thread.Sleep(1000);
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
