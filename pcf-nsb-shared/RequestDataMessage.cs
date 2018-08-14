using System;
using NServiceBus;

public class RequestDataMessage :
    IMessage
{
    public Guid DataId { get; set; }
    public string String { get; set; }
}
