using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Logging;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace pcf_nsb_client
{
    class Program
    {
        // AutoResetEvent to signal when to exit the application.
        private static readonly AutoResetEvent waitHandle = new AutoResetEvent(false);

        public static IConfigurationRoot configuration;

        static async Task Main()
        {
            // Create service collection
            ServiceCollection serviceCollection = new ServiceCollection();
            ConfigureServices(serviceCollection);


            Console.WriteLine(configuration.GetSection("toast").Value); //just testing the config setup

            Console.Title = "Samples.FullDuplex.Client";
            LogManager.Use<DefaultFactory>()
                .Level(LogLevel.Info);

            EndpointConfiguration endpointConfiguration = ConfigureNSB(serviceCollection);

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

        private static EndpointConfiguration ConfigureNSB(ServiceCollection serviceCollection)
        {
            var builder = new ContainerBuilder();

            builder.Populate(serviceCollection);

            var container = builder.Build();

            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Client");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            endpointConfiguration.UseTransport<LearningTransport>();

            endpointConfiguration.UseContainer<AutofacBuilder>(
               customizations: customizations =>
               {
                   customizations.ExistingLifetimeScope(container);
               });
            return endpointConfiguration;
        }

        private static void ConfigureServices(ServiceCollection serviceCollection)
        {
            var environmentName = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            configuration = new ConfigurationBuilder()
               .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
               .AddJsonFile($"appsettings.{environmentName}.json", optional: true)
               .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
