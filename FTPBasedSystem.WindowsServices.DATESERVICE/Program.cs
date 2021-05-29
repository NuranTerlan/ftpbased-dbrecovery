using System;

namespace FTPBasedSystem.WindowsServices.DATESERVICE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Date Service Analyzer";
            Console.WriteLine("WS -> Date Service is starting to watch proper directory..");
            var watcher = new FtpDirectoryWatcher();
            watcher.WatchDirectory();
        }
    }
}
