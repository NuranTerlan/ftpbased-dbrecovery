using System.Collections.Generic;
using System.Threading.Tasks;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.DOMAINENTITIES.Models;
using FTPBasedSystem.SERVICES.Abstraction.Base;

namespace FTPBasedSystem.SERVICES.Abstraction
{
    public interface IDateService : IBusinessServiceBase<DateDto>
    {
    }
}