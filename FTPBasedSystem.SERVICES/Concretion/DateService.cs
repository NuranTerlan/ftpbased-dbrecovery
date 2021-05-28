using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.DOMAINENTITIES.Models;
using FTPBasedSystem.SERVICES.Abstraction;
using Microsoft.EntityFrameworkCore;
using Action = FTPBasedSystem.DOMAINENTITIES.Models.Action;

namespace FTPBasedSystem.SERVICES.Concretion
{
    public class DateService : IDateService
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICustomLoggingService _logger;

        public DateService(IAppDbContext context, IMapper mapper, ICustomLoggingService logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<DateDto>> Create(DateDto model)
        {
            var realModel = _mapper.Map<Date>(model);

            var token = new CancellationToken();
            await _context.Dates.AddAsync(realModel, token);

            var isAdded = await _context.SaveChangesAsync(token);
            if (isAdded < 1)
            {
                return Response.Fail<DateDto>("Can't create new date in db or something wrong with process!");
            }

            await _logger.LogEntityAction(nameof(Date), token);
            return Response.Success<DateDto>(model, $"New date ({model.DateValue}) is created successfully!");
        }

        public async Task<Response<List<DateDto>>> GetAllWithResponse()
        {
            var dates = await GetAllEntities();

            if (dates is null)
            {
                return Response.Fail<List<DateDto>>("Something happen while fetching data!");
            }

            if (dates.Count == 0)
            {
                return Response.Fail<List<DateDto>>("Dates table is empty! Create new one");
            }

            return Response.Success<List<DateDto>>(dates, $"{dates.Count} dates are fetched successfully");
        }

        public async Task<List<DateDto>> GetAllEntities()
        {
            var dates = await _context.Dates.AsNoTracking().ToListAsync();
            return _mapper.Map<List<DateDto>>(dates);
        }
    }
}