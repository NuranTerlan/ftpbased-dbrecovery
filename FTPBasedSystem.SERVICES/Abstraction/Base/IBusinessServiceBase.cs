using System.Collections.Generic;
using System.Threading.Tasks;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.DOMAINENTITIES.Models;
using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.SERVICES.Abstraction.Base
{
    public interface IBusinessServiceBase<T> where T : IEntityDto
    {
        Task<Response<T>> Create(T model);
        Task<Response<List<T>>> GetAllWithResponse();
        Task<List<T>> GetAllEntities();
    }
}