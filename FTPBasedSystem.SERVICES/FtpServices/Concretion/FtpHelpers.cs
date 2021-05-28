using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FTPBasedSystem.SERVICES.FtpServices.Abstraction;
using Microsoft.Extensions.Logging;

namespace FTPBasedSystem.SERVICES.FtpServices.Concretion
{
    public class FtpHelpers : IFtpHelpers
    {
        private readonly ILogger<FtpHelpers> _logger;

        public FtpHelpers(ILogger<FtpHelpers> logger)
        {
            _logger = logger;
        }

        public async Task UploadFile(string username, string password, string from, string to)
        {
            var ftpIp = "ftp://127.0.0.1/" + to;
            var ftpRequest = (FtpWebRequest) WebRequest.Create(ftpIp);
            ftpRequest.UseBinary = true;
            ftpRequest.UsePassive = true;
            ftpRequest.KeepAlive = false;
            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;
            ftpRequest.Credentials = new NetworkCredential(username, password);

            using var srcReader = new StreamReader(from);
            var fileContents = Encoding.UTF8.GetBytes(await srcReader.ReadToEndAsync());
            srcReader.Close();

            ftpRequest.ContentLength = fileContents.Length;

            await using var requestedStream = ftpRequest.GetRequestStream();
            await requestedStream.WriteAsync(fileContents, 0, fileContents.Length);
            requestedStream.Close();

            using var ftpResponse = (FtpWebResponse) await ftpRequest.GetResponseAsync();
            _logger.LogInformation($"{from} is uploaded as ({to}) via ftp | " +
                                   $"Status => Code: {ftpResponse.StatusCode}, " +
                                   $"Description: {ftpResponse.StatusDescription}");
            ftpResponse.Close();
        }
    }
}