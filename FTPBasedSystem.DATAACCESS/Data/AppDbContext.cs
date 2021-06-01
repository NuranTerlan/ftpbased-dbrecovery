using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace FTPBasedSystem.DATAACCESS.Data
{
    public class AppDbContext : DbContext, IAppDbContext
    {
        private readonly IEnumerable<string> _notDeletedTables;

        public DbSet<Number> Numbers { get; set; }
        public DbSet<Text> Texts { get; set; }
        public DbSet<Date> Dates { get; set; }
        public DbSet<Action> Actions { get; set; }

        public AppDbContext()
        {
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            :base(options)
        {
            _notDeletedTables = new List<string>
            {
                "Actions",
            };
        }

        public async Task ClearAllTables()
        {
            var listOfTables = this.Model.GetEntityTypes()
                .Select(t => t.GetTableName())
                .Distinct();
            
            await ClearSpecificTables(listOfTables);
        }

        public async Task ClearSpecificTables(IEnumerable<string> tables)
        {
            foreach (var table in tables)
            {
                if (_notDeletedTables.Any(t => t.Equals(table)))
                {
                    continue;
                }
                await this.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [{table}]");
            }
        }

        public async Task ClearSpecificTable(string tableName)
        {
            await this.Database.ExecuteSqlRawAsync($"TRUNCATE TABLE [{tableName}]");
        }
    }
}