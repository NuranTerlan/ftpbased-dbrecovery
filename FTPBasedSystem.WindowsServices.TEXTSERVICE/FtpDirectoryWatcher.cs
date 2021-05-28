using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using WinSCP;

namespace FTPBasedSystem.WindowsServices.TEXTSERVICE
{
    public class FtpDirectoryWatcher
    {
        private const string Ip = "127.0.0.1";
        private const string Credential = "textservice";

        public void WatchDirectory()
        {
            try
            {
                // Setup session options
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = Ip,
                    UserName = Credential,
                    Password = Credential,
                    FtpMode = FtpMode.Active
                };

                using (var session = new Session())
                {
                    // Connect
                    session.Open(sessionOptions);

                    List<string> prevFiles = null;

                    while (true)
                    {
                        // Collect file list
                        var files =
                            session.EnumerateRemoteFiles(
                                session.HomePath, "*.*", EnumerationOptions.AllDirectories)
                                .Select(fileInfo => fileInfo.Name)
                                .ToList();
                        if (prevFiles == null)
                        {
                            // In the first round, just print number of files found
                            Console.WriteLine($"{files.Count} text-files found currently.");
                        }
                        else
                        {
                            // Then look for differences against the previous list
                            var added = files.Except(prevFiles);
                            if (added.Any())
                            {
                                foreach (var path in added)
                                {
                                    Console.WriteLine($"\nTRIGGER => New file added to text service: {path}");
                                    var content = PipelineHelpers.ReadFromFtp(path, Credential);
                                    Console.WriteLine($"Content of this file: \n{content}");
                                }

                            }

                            // if remove watch needed use commented code below
                            //var removed = prevFiles.Except(files);
                            //if (removed.Any())
                            //{
                            //    Console.WriteLine("Removed files:");
                            //    foreach (var path in removed)
                            //    {
                            //        Console.WriteLine(path);
                            //    }
                            //}
                        }

                        prevFiles = files;

                        //Console.WriteLine("- - - - Idle");
                        Thread.Sleep(TimeSpan.FromSeconds(2));
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        }

    }
}