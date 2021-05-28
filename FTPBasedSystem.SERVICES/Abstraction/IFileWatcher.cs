using System.Threading.Tasks;

namespace FTPBasedSystem.SERVICES.Abstraction
{
    public interface IFileWatcher
    {
        Task WatchDirectory(string directory);
    }
}