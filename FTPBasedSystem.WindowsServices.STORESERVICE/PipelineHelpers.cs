using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace FTPBasedSystem.WindowsServices.STORESERVICE
{
    public static class PipelineHelpers
    {
        private static readonly NameValueCollection Configurations = ConfigurationManager.AppSettings;

        private static readonly string Credential = Configurations.Get("FtpCredential");
        private static readonly string HostName = Configurations.Get("FtpHostName");
        private static readonly string FtpBaseRequestUri = "ftp://" + HostName + '/';

        public static string ReadFromFtp(string directory)
        {
            if (directory is null) throw new ArgumentNullException(nameof(directory));

            var ftpIp = FtpBaseRequestUri + directory;
            var ftpRequest = (FtpWebRequest)WebRequest.Create(ftpIp);
            ftpRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            ftpRequest.Credentials = new NetworkCredential(Credential, Credential);

            using var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            Console.WriteLine($"{directory} is downloaded via ftp | " +
                              $"Status => Code: {ftpResponse.StatusCode}, " +
                              $"Description: {ftpResponse.StatusDescription}");

            using var responseStream = ftpResponse.GetResponseStream();
            using var srcReader = new StreamReader(responseStream);
            var fileContents = srcReader.ReadToEnd();
            responseStream.Close();
            srcReader.Close();
            ftpResponse.Close();

            return fileContents;
        }

        public static void UploadFileViaFtp(string fileContents, string to)
        {
            var ftpIp = FtpBaseRequestUri + to;
            var ftpRequest = (FtpWebRequest)WebRequest.Create(ftpIp);
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            ftpRequest.Credentials = new NetworkCredential(Credential, Credential);

            ftpRequest.ContentLength = fileContents.Length;

            using var requestedStream = ftpRequest.GetRequestStream();
            requestedStream.Write(Encoding.UTF8.GetBytes(fileContents), 0, fileContents.Length);
            requestedStream.Close();

            using var ftpResponse = (FtpWebResponse) ftpRequest.GetResponse();
            Console.WriteLine($"\n{to} is uploaded via ftp | " +
                                   $"Status => Code: {ftpResponse.StatusCode}, " +
                                   $"Description: {ftpResponse.StatusDescription}");
            ftpResponse.Close();
        }


        public static void ConnectAndWaitAllMessages()
        {
            var factory = new ConnectionFactory
            {
                Uri = new Uri(Configurations.Get("MQHostName") ?? string.Empty)
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            var queues = new List<string>
            {
                Configurations.Get("DatesQueueName"),
                Configurations.Get("NumbersQueueName"),
                Configurations.Get("TextsQueueName")
            };

            foreach (var queue in queues)
            {
                channel.QueueDeclare(queue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(channel);
                consumer.Received += (sender, e) =>
                {
                    var body = e.Body.ToArray();
                    var tempSerializing = Encoding.UTF8.GetString(body);
                    var queueMsg = JsonConvert.DeserializeObject<QueueMessage>(tempSerializing);
                    Console.WriteLine($"\nMessage which produced by {queueMsg.Name} is received from QUEUE -> {queue}");
                    try
                    {
                        const string directory = "result.txt";
                        var contentOfMsg = ReadFromFtp(directory);
                        var resultContent = $"{(contentOfMsg != string.Empty ? contentOfMsg + '\n' : "")}{queueMsg.Message}";
                        UploadFileViaFtp(resultContent, directory);
                        Console.WriteLine($"\nMessage content added to {directory}");
                    }
                    catch (Exception exception)
                    {
                        Console.WriteLine(exception.Message);
                    }
                };
                Console.WriteLine($"Observing started for new messages from QUEUE -> {queue} >>");
                channel.BasicConsume(queue, true, consumer);
            }

            Console.ReadLine();
        }
    }
}