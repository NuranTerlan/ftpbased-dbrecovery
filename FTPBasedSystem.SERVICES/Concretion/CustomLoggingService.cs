using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.Models;
using FTPBasedSystem.SERVICES.Abstraction;
using Microsoft.EntityFrameworkCore;
using Action = FTPBasedSystem.DOMAINENTITIES.Models.Action;

namespace FTPBasedSystem.SERVICES.Concretion
{
    public class CustomLoggingService : ICustomLoggingService
    {
        private readonly IAppDbContext _context;

        public CustomLoggingService(IAppDbContext context)
        {
            _context = context;
        }

        public async Task LogEntityAction(string modelName, CancellationToken token)
        {
            var action = new Action
            {
                Model = modelName,
                ExecutedAt = DateTime.UtcNow
            };

            await _context.Actions.AddAsync(action, token);
            

            await _context.SaveChangesAsync(token);
        }

        public async Task<Response<List<Action>>> GetAllActions()
        {
            var actions = await _context.Actions.AsNoTracking().ToListAsync();

            if (actions is null)
            {
                return Response.Fail<List<Action>>("Something happen while fetching actions!!");
            }

            if (actions.Count == 0)
            {
                return Response.Fail<List<Action>>("Log an action to fetch them");
            }

            return Response.Success<List<Action>>(actions, "Action logs are fetched successfully!");
        }
    }
}