using System;

namespace FTPBasedSystem.WindowsServices.NUMERICSERVICE
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("WS -> Numeric Service is starting to watch proper directory..");
            var watcher = new FtpDirectoryWatcher();
            watcher.WatchDirectory();
        }
    }
}
