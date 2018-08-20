using System.Threading.Tasks;
using NServiceBus;
using NServiceBus.Logging;
using pcf_nsb_shared;

class DataResponseMessageHandler :
    IHandleMessages<DataResponseMessage>
{
    static ILog log = LogManager.GetLogger<DataResponseMessageHandler>();
    public Task Handle(DataResponseMessage message, IMessageHandlerContext context)
    {
        log.Info($"Response received with description: {message.String}");
        return Task.CompletedTask;
    }
}
