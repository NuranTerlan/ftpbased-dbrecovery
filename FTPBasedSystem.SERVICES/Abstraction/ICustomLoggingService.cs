using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FTPBasedSystem.DOMAINENTITIES.Models;

namespace FTPBasedSystem.SERVICES.Abstraction
{
    public interface ICustomLoggingService
    {
        Task LogEntityAction(string modelName, CancellationToken token);
        Task<Response<List<Action>>> GetAllActions();
    }
}