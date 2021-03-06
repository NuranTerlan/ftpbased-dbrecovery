using System.Threading.Tasks;

namespace FTPBasedSystem.SERVICES.FtpServices.Abstraction
{
    public interface IFtpHelpers
    {
        Task<bool> UploadFile(string hostName, string credential, string from, string to);
    }
}