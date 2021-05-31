using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FTPBasedSystem.DOMAINENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FTPBasedSystem.DATAACCESS.Data.Abstraction
{
    public interface IAppDbContext
    {
        DbSet<Number> Numbers { get; set; }
        DbSet<Text> Texts { get; set; }
        DbSet<Date> Dates { get; set; }
        DbSet<Action> Actions { get; set; }
        DatabaseFacade Database { get; }
        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        Task ClearAllTables();
        Task ClearSpecificTables(IEnumerable<string> tables);
    }
}