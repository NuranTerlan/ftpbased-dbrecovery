using System;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;

namespace FTPBasedSystem.WindowsServices.DATESERVICE
{
    public static class PipelineHelpers
    {
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
            const string credential = "dateservice";
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

        public static string SortAllRows(string tableText)
        {
            if (tableText == null) throw new ArgumentNullException(nameof(tableText));
            return string.Empty;
        }

        public static bool EnqueueTheText(string result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            // if successfully added to MQ then return true

            // otherwise false

            return true;
        }
    }
}