using System.Collections.Generic;
using System.Threading.Tasks;

namespace FTPBasedSystem.API.ScheduledTasks.Abstract
{
    public interface ICheckDatabaseMiddleware
    {
        Task FetchAndSendToFtpServer();
        Task DeleteOldData(IEnumerable<string> tables);
    }
}