using System;
using System.Threading;

namespace FTPBasedSystem.API.ScheduledTasks
{
    public static class TestTask
    {
        private static readonly object Flag = new object();

        public static void DoSomething()
        {
            lock (Flag)
            {
                for (var i = 0; i < 8; i++)
                {
                    Thread.Sleep(2000);
                    Console.WriteLine(i);
                }
            }
        }
    }
}