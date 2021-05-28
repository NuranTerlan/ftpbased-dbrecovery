using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FTPBasedSystem.API.ScheduledTasks.Abstract;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.SERVICES.Abstraction;
using FTPBasedSystem.SERVICES.FtpServices.Abstraction;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Converters;

namespace FTPBasedSystem.API.ScheduledTasks
{
    public class CheckDatabaseMiddleware : ICheckDatabaseMiddleware
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<CheckDatabaseMiddleware> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFtpHelpers _ftpHelpers;

        public CheckDatabaseMiddleware(IAppDbContext context, ILogger<CheckDatabaseMiddleware> logger, IUnitOfWork unitOfWork, IFtpHelpers ftpHelpers)
        {
            _context = context;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _ftpHelpers = ftpHelpers;
        }

        public async Task FetchAndSendToFtpServer()
        {
            try
            {
                var numbers = await _unitOfWork.Numeric.GetAllEntities();
                var texts = await _unitOfWork.Text.GetAllEntities();
                var dates = await _unitOfWork.Date.GetAllEntities();

                var dictionary = new Dictionary<List<string>, IEnumerable<IEntityDto>>
                {
                    {new List<string> {"number", "numericservice"}, numbers},
                    {new List<string>{"text", "textservice"}, texts},
                    {new List<string>{"date", "dateservice"}, dates}
                };

                bool isAnyDataCameFromDb = numbers.Count > 0 || texts.Count > 0 || dates.Count > 0;

                foreach (var (config,items) in dictionary)
                {
                    var fromFile = config[0];
                    var credential = config[1];
                    _logger.LogInformation($"Checking process is started for {fromFile}s service");
                    var entityDtoList = items as IEntityDto[] ?? items.ToArray();
                    if (!entityDtoList.Any())
                    {
                        _logger.LogInformation($"There is not any data found for {fromFile}s service!");
                        continue;
                    }
                    var (from, to) = await WriteListAndReturnLastConfig(entityDtoList, fromFile);
                    await _ftpHelpers.UploadFile(credential, credential, from, to);
                }

                if (isAnyDataCameFromDb)
                {
                    await DeleteOldData();
                    _logger.LogInformation("Transfer and Truncate processes are completed!");
                }
            }
            catch (WebException e)
            {
                string status = ((FtpWebResponse) e.Response)?.StatusDescription;
                _logger.LogCritical(status);
            }
        }

        private async Task<Tuple<string, string>> WriteListAndReturnLastConfig(IEnumerable<IEntityDto> list, string fromFile)
        {
            var from = $"F:\\tempdbrecovery\\{fromFile}.txt";
            using (var stream = File.WriteAllTextAsync(from, string.Empty))
            {
                if (stream.IsCompletedSuccessfully)
                {
                    _logger.LogInformation($"{from} is cleaned for writing something new in it.");
                }
                stream.Dispose();
            }
            await using (var writer = new StreamWriter(from))
            {
                foreach (var dto in list)
                {
                    await writer.WriteLineAsync(dto.ToString());
                }
            }

            //var key = GenerateKeyEndingForFileName();
            var to = $"{fromFile}s-{Guid.NewGuid()}.txt";

            return Tuple.Create(from, to);
        }

        //private string GenerateKeyEndingForFileName()
        //{
        //    var ticks = DateTime.UtcNow.Ticks;
        //    var bytes = BitConverter.GetBytes(ticks);
        //    var key = Convert.ToBase64String(bytes)
        //        .Replace('+', '-')
        //        .Replace('/', '-')
        //        .TrimEnd('=');

        //    return key;
        //}

        public async Task DeleteOldData()
        {
            await _context.ClearAllTables();
            Console.WriteLine("All entities are cleared!!");
        }
    }
}