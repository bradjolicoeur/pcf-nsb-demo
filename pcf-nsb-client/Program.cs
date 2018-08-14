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

            while (true)
            {
                
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


        }
    }
}
