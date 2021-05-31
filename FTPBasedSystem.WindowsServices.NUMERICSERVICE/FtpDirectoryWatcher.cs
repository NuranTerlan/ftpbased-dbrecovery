using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Threading;
using WinSCP;

namespace FTPBasedSystem.WindowsServices.NUMERICSERVICE
{
    public class FtpDirectoryWatcher
    {
        private readonly NameValueCollection _configurations = ConfigurationManager.AppSettings;

        public void WatchDirectory()
        {
            var ip = _configurations.Get("FtpHostName");
            var credential = _configurations.Get("FtpCredential");

            try
            {
                // Setup session options
                var sessionOptions = new SessionOptions
                {
                    Protocol = Protocol.Ftp,
                    HostName = ip,
                    UserName = credential,
                    Password = credential,
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
                            Console.WriteLine($"{files.Count} number-files found currently.");
                        }
                        else
                        {
                            // Then look for differences against the previous list
                            var added = files.Except(prevFiles);
                            if (added.Any())
                            {
                                foreach (var path in added)
                                {
                                    Console.WriteLine($"\nTRIGGER => New file added to numeric service: {path}");
                                    try
                                    {
                                        var content = PipelineHelpers.ReadFromFtp(path);
                                        var sortedNumbers = PipelineHelpers.SortAllLines(content, false);
                                        PipelineHelpers.UploadFileViaFtp(sortedNumbers, path);
                                        PipelineHelpers.EnqueueTheDateToRabbitMq(sortedNumbers);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e.Message);
                                    }
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
                        Thread.Sleep(TimeSpan.FromSeconds(3));
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