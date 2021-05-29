using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace FTPBasedSystem.WindowsServices.NUMERICSERVICE
{
    public static class PipelineHelpers
    {
        public const string Credential = "numericservice";

        public static string ReadFromFtp(string directory)
        {
            if (directory is null) throw new ArgumentNullException(nameof(directory));

            var ftpIp = "ftp://127.0.0.1/" + directory;
            var ftpRequest = (FtpWebRequest) WebRequest.Create(ftpIp);
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
            var ftpIp = "ftp://127.0.0.1/" + to;
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

            using var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            Console.WriteLine($"\n{to} is uploaded via ftp | " +
                              $"Status => Code: {ftpResponse.StatusCode}, " +
                              $"Description: {ftpResponse.StatusDescription}");
            ftpResponse.Close();
        }


        public static string SortAllLines(string tableText, bool isAscending = true)
        {
            if (tableText == null) throw new ArgumentNullException(nameof(tableText));

            var numberStrings = tableText.Trim().Split(Environment.NewLine);

            // filter logic based on: only numbers which have valid numeric structure can be filtered!
            var validNumbers = numberStrings
                .Where(n => int.TryParse(n, out var validNumber))
                .Select(int.Parse);

            var result = isAscending ? validNumbers.OrderBy(n => n) : validNumbers.OrderByDescending(n => n);

            Console.WriteLine($"Received numbers data is sorted by {(isAscending ? "ascending" : "descending")} successfully!");

            return string.Join('\n', result);
        }

        public static void EnqueueTheDateToRabbitMq(string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var factory = new ConnectionFactory
            {
                Uri = new Uri("amqp://guest:guest@localhost:5672")
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            const string queueName = "numbers-queue";
            channel.QueueDeclare(queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = new QueueMessage
            {
                Name = "Numeric-Producer",
                Message = result
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            Console.WriteLine("Publishing the message..");
            channel.BasicPublish("", queueName, null, body);
            Console.WriteLine($"Message published to the {queueName} by {message.Name}");
        }
    }
}