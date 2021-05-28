using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FTPBasedSystem.API.ScheduledTasks
{
    public class CustomTask
    {
        public CustomTask()
        {
            while (true)
            {
                Task.WaitAll(Task.Run(() =>
                {
                    Console.WriteLine("heyy");
                }), Task.Delay(TimeSpan.FromSeconds(5)));
            }
        }
    }
}