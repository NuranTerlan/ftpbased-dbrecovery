using System;

namespace FTPBasedSystem.WindowsServices.TEXTSERVICE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "Text Service Analyzer";
            Console.WriteLine("WS -> Text Service is starting to watch proper directory..");
            var watcher = new FtpDirectoryWatcher();
            watcher.WatchDirectory();
        }
    }
}
