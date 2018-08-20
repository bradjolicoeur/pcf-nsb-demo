using NServiceBus;
using System;

//Typically messages would go in a shared assembly deployed via nuget; 
//the class was copied to the client and server to simplify the demo
namespace pcf_nsb_shared
{
    public class DataResponseMessage :
    IMessage
    {
        public Guid DataId { get; set; }
        public string String { get; set; }
    }
}


