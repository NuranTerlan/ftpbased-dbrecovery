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
    public class TextService : ITextService
    {
        private readonly IAppDbContext _context;
        private readonly IMapper _mapper;
        private readonly ICustomLoggingService _logger;

        public TextService(IAppDbContext context, IMapper mapper, ICustomLoggingService logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Response<TextDto>> Create(TextDto model)
        {
            var realModel = _mapper.Map<Text>(model);

            var token = new CancellationToken();
            await _context.Texts.AddAsync(realModel, token);

            var isAdded = await _context.SaveChangesAsync(token);
            if (isAdded < 1)
            {
                return Response.Fail<TextDto>("Can't create Text entity!");
            }

            await _logger.LogEntityAction(nameof(Text), token);
            return Response.Success<TextDto>(model, $"New text ({model.TextValue}) is created successfully!");
        }

        public async Task<Response<List<TextDto>>> GetAllWithResponse()
        {
            var texts = await GetAllEntities();

            if (texts is null)
            {
                return Response.Fail<List<TextDto>>("Something happen while fetching data!");
            }

            if (texts.Count == 0)
            {
                return Response.Fail<List<TextDto>>("Texts table is empty! Create new one");
            }

            return Response.Success<List<TextDto>>(texts, $"{texts.Count} texts are fetched successfully");
        }

        public async Task<List<TextDto>> GetAllEntities()
        {
            var texts = await _context.Texts.AsNoTracking().ToListAsync();
            return _mapper.Map<List<TextDto>>(texts);
        }
    }
}