using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NServiceBus;
using NServiceBus.Logging;
using Steeltoe.Extensions.Configuration.CloudFoundry;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace pcf_nsb_server
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

            Console.Title = "Samples.FullDuplex.Server";
            LogManager.Use<DefaultFactory>()
                .Level(LogLevel.Info);

            EndpointConfiguration endpointConfiguration = ConfigureNSB(serviceCollection);

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

        private static EndpointConfiguration ConfigureNSB(ServiceCollection serviceCollection)
        {
            var builder = new ContainerBuilder();

            builder.Populate(serviceCollection);

            var container = builder.Build();

            var endpointConfiguration = new EndpointConfiguration("Samples.FullDuplex.Server");
            endpointConfiguration.UsePersistence<LearningPersistence>();
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.UseConventionalRoutingTopology();
            transport.ConnectionString("host=rabbitmq.local.pcfdev.io; username=6da81eaf-6607-4b91-8730-9e9e676c1973; password=9feelcfev1hlk2aiquspkra2kj; virtualhost=eb49218e-e0f1-4ab1-8798-729c6b5c222e");

            endpointConfiguration.EnableInstallers();

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
               .AddCloudFoundry()
               .Build();

            // Add access to generic IConfigurationRoot
            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);
        }
    }
}
