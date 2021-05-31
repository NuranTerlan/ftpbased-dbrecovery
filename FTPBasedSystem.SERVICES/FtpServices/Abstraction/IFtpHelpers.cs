using System.Threading.Tasks;

namespace FTPBasedSystem.SERVICES.FtpServices.Abstraction
{
    public interface IFtpHelpers
    {
        Task<bool> UploadFile(string username, string password, string from, string to);
    }
}