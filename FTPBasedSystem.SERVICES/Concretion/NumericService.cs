using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.DOMAINENTITIES.Models;
using FTPBasedSystem.SERVICES.Abstraction;
using Microsoft.EntityFrameworkCore;

namespace FTPBasedSystem.SERVICES.Concretion
{
    public class NumericService : INumericService
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICustomLoggingService _logger;

        public NumericService(IAppDbContext context, IMapper mapper, ICustomLoggingService logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<NumberDto>> Create(NumberDto model)
        {
            var realModel = _mapper.Map<Number>(model);

            var token = new CancellationToken();
            await _context.Numbers.AddAsync(realModel, token);
            var isAdded = await _context.SaveChangesAsync(token);
            if (isAdded < 1)
            {
                return Response.Fail<NumberDto>("Can't create Number entity!");
            }

            await _logger.LogEntityAction(nameof(Number), token);
            return Response.Success<NumberDto>(model, $"New number ({model.NumberValue}) is created successfully!");
        }

        public async Task<Response<List<NumberDto>>> GetAllWithResponse()
        {
            var numbers = await GetAllEntities();

            if (numbers is null)
            {
                return Response.Fail<List<NumberDto>>("Something happen while fetching data!");
            }

            if (numbers.Count == 0)
            {
                return Response.Fail<List<NumberDto>>("Numbers table is empty! Create new one...");
            }

            return Response.Success<List<NumberDto>>(numbers, $"{numbers.Count} numbers are fetched successfully");
        }

        public async Task<List<NumberDto>> GetAllEntities()
        {
            var numbers = await _context.Numbers.AsNoTracking().ToListAsync();
            return _mapper.Map<List<NumberDto>>(numbers);
        }
    }
}