using System;
using System.Collections.Specialized;
using System.Configuration;
using System.Globalization;
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
        private static readonly NameValueCollection Configurations = ConfigurationManager.AppSettings;

        private static readonly string Credential = Configurations.Get("FtpCredential");
        private static readonly string HostName = Configurations.Get("FtpHostName");
        private static readonly string FtpBaseRequestUri = "ftp://" + HostName + '/';

        public static string ReadFromFtp(string directory)
        {
            if (directory is null) throw new ArgumentNullException(nameof(directory));

            var ftpIp = FtpBaseRequestUri + directory;
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

            using var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse();
            Console.WriteLine($"\n{to} is uploaded via ftp | " +
                              $"Status => Code: {ftpResponse.StatusCode}, " +
                              $"Description: {ftpResponse.StatusDescription}");
            ftpResponse.Close();
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

            var dateStrings = tableText.Trim().Split(Environment.NewLine);

            // filter logic based on: only dates which have valid format can be filtered!
            //var validDates = dateStrings
            //    .Where(d => DateTime.TryParseExact(d, "dddd, dd MMMM yyyy HH:mm:ss tt", CultureInfo.CurrentCulture,
            //        DateTimeStyles.None, out var dt))
            //    .Select(d => DateTime.ParseExact(d, "dddd, dd MMMM yyyy HH: mm:ss tt", null));
            const string specFormat = "dddd, dd MMMM yyyy HH:mm:ss tt";
            var validDates = dateStrings.Select(ds => new
            {
                valid = DateTime.TryParseExact(ds, specFormat, CultureInfo.CurrentCulture,
                    DateTimeStyles.AdjustToUniversal, out var dt),
                date = dt
            }).Where(x => x.valid).Select(x => x.date);

            var result = isAscending
                ? validDates.OrderBy(d => d).Select(d => d.ToString(specFormat))
                : validDates.OrderByDescending(d => d).Select(d => d.ToString(specFormat));

            Console.WriteLine($"Received dates data is sorted by {(isAscending ? "ascending" : "descending")} successfully!");

            return string.Join('\n', result);
        }

        public static void EnqueueTheDateToRabbitMq(string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var factory = new ConnectionFactory
            {
                Uri = new Uri(Configurations.Get("MQHostName") ?? string.Empty)
            };
            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();
            string queueName = Configurations.Get("QueueName");
            channel.QueueDeclare(queueName,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var message = new QueueMessage
            {
                Name = Configurations.Get("MessageProducer"),
                Message = result
            };

            var body = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            Console.WriteLine("Publishing the message..");
            channel.BasicPublish("", queueName, null, body);
            Console.WriteLine($"Message published to the {queueName} by {message.Name}");
        }
    }
}