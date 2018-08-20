using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
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
                
            }

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
            transport.ConnectionString(GetConnectionString());

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

        private static string GetConnectionString()
        {
            string local = configuration.GetSection("RabbitMQ").Value;
            if (!string.IsNullOrEmpty(local))
                return local;

            var credentials = "$..[?(@.name=='rabbitmq')].credentials";
            var jObj = JObject.Parse(Environment.GetEnvironmentVariable("VCAP_SERVICES"));

            if (jObj.SelectToken($"{credentials}") == null)
                throw new Exception("Expects a PCF managed rabbitmq service binding named 'rabbitmq'");

            var vhost = (string)jObj.SelectToken($"{credentials}.vhost");
            var host = (string)jObj.SelectToken($"{credentials}.hostname");
            var pwd = (string)jObj.SelectToken($"{credentials}.password");
            var username = (string)jObj.SelectToken($"{credentials}.username");

            string connectionString = $"host={host}; username={username}; password={pwd}; virtualhost={vhost}";

            Console.Out.WriteLine(connectionString);

            return connectionString;
        }
    }
}
