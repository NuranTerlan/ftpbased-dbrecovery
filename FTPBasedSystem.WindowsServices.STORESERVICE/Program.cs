using System;

namespace FTPBasedSystem.WindowsServices.STORESERVICE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Store Service Analyzer";
            Console.WriteLine("WS -> Store Service waiting for new messages from all services..\n");
            PipelineHelpers.WaitAllMessages();
        }
    }
}
