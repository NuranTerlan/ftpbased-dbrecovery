using System;
using System.IO;
using System.Threading.Tasks;

namespace FTPBasedSystem.WindowsServices.DATESERVICE
{
    public class FileWatcher
    {
        private string _directory;

        public FileWatcher(string directory)
        {
            _directory = directory;
        }

        public void WatchDirectory()
        {
            if (_directory.Equals(string.Empty))
            {
                Console.WriteLine("Directory should contain a value to watch somewhere!");
                return;
            }

            try
            {
                using var watcher = new FileSystemWatcher(_directory);
                // you've to mention path inside constructor, otherwise use commented code below
                //watcher.Path = directory;

                watcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.CreationTime;
                
                // watch all files using code below
                watcher.Filter = "*.*"; // means that watched file can has any name and has any extension

                // enable to use this watcher component
                watcher.EnableRaisingEvents = true;

                // add event handlers
                watcher.Created += new FileSystemEventHandler(OnNewFileAdded);

                //Make an infinite loop to watch directory until we break it manually
                while (true)
                {
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        private static void OnNewFileAdded(object source, FileSystemEventArgs e)
        {
            Console.WriteLine($"Trigger new file observed | name is: {e.Name}");
            var content = PipelineHelpers.ReadFromFtp(e.Name, "credential");
            Console.WriteLine($"Content of this file: {content}");
        }
    }
}
