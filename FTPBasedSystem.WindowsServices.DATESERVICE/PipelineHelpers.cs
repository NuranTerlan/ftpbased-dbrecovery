using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace FTPBasedSystem.WindowsServices.DATESERVICE
{
    public static class PipelineHelpers
    {
        public static string ReadFromFtp(string directory, string credential)
        {
            if (directory is null) throw new ArgumentNullException(nameof(directory));
            if (credential is null) throw new ArgumentNullException(nameof(credential));

            var ftpIp = "ftp://127.0.0.1/" + directory;
            var ftpRequest = (FtpWebRequest) WebRequest.Create(ftpIp);
            ftpRequest.CachePolicy = new RequestCachePolicy(RequestCacheLevel.CacheIfAvailable);

            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;
            ftpRequest.Credentials = new NetworkCredential(credential, credential);

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

        /// <summary>
        /// Sort the given text
        /// </summary>
        /// <param name="tableText"></param>
        /// <param name="isAscending">if true -> Ascending (default), if false -> Descending</param>
        /// <returns></returns>
        public static string SortAllLines(string tableText, bool isAscending = true)
        {
            if (tableText == null) throw new ArgumentNullException(nameof(tableText));

            var dateStrings = tableText.Split('\n');

            // filter logic based on: only dates which have valid format can be filtered!
            var validDates = dateStrings
                .Where(x => DateTime.TryParse(x, out var dt))
                .Select(DateTime.Parse);

            var result = isAscending ? validDates.OrderBy(d => d) : validDates.OrderByDescending(d => d);

            Console.WriteLine($"Received data is sorted by {(isAscending ? "ascending" : "descending")} successfully!");

            return string.Join('\n', result);
        }

        public static void EnqueueTheDateToRabbitMq(string result)
        {
            try
            {
                if (result == null) throw new ArgumentNullException(nameof(result));

                var factory = new ConnectionFactory
                {
                    Uri = new Uri("amqp://guest:guest@localhost:5672")
                };
                using var connection = factory.CreateConnection();
                using var channel = connection.CreateModel();
                const string queueName = "dates-queue";
                channel.QueueDeclare(queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var message = new QueueMessage
                {
                    Name = "Date-Producer",
                    Message = result
                };

                var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                Console.WriteLine("Publishing the message..");
                channel.BasicPublish("", queueName, null, body);
                Console.WriteLine($"Message published to the {queueName} by {message.Name}");
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}