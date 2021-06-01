using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using FTPBasedSystem.API.Configs;
using FTPBasedSystem.API.Helpers;
using FTPBasedSystem.API.ScheduledTasks.Abstract;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.DOMAINENTITIES.DTOs;
using FTPBasedSystem.SERVICES.Abstraction;
using FTPBasedSystem.SERVICES.FtpServices.Abstraction;
using Hangfire;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FTPBasedSystem.API.ScheduledTasks
{
    public class CheckDatabaseMiddleware : ICheckDatabaseMiddleware, IDisposable
    {
        private readonly IAppDbContext _context;
        private readonly ILogger<CheckDatabaseMiddleware> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IFtpHelpers _ftpHelpers;
        private readonly FilePathOptions _filePathOptions;
        private readonly FtpRequestOptions _ftpRequestOptions;

        private readonly SemaphoreSlim _semLock = new SemaphoreSlim(1, 1);

        public CheckDatabaseMiddleware(IAppDbContext context, ILogger<CheckDatabaseMiddleware> logger, IUnitOfWork unitOfWork, 
            IFtpHelpers ftpHelpers, IOptions<FilePathOptions> fileNameOptions, IOptions<FtpRequestOptions> ftpRequestOptions)
        {
            _context = context;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _ftpHelpers = ftpHelpers;
            _filePathOptions = fileNameOptions.Value;
            _ftpRequestOptions = ftpRequestOptions.Value;
        }

        [AutomaticRetry(Attempts = 0)] // it means if job break for some reason don't retry again, just wait for next one
        public async Task FetchAndSendToFtpServer()
        {
            await _semLock.WaitAsync();
            try
            {
                var numbers = await _unitOfWork.Numeric.GetAllEntities();
                var texts = await _unitOfWork.Text.GetAllEntities();
                var dates = await _unitOfWork.Date.GetAllEntities();

                if (numbers is null || texts is null || dates is null)
                {
                    _logger.LogWarning("Something wrong with database because some of tables return null..");
                    return;
                }

                var isAnyDataCameFromDb = numbers.Count > 0 || texts.Count > 0 || dates.Count > 0;

                if (!isAnyDataCameFromDb)
                {
                    _logger.LogInformation("Nothing found in db related with domain entities!");
                    return;
                }

                var dictionary = new Dictionary<List<string>, IEnumerable<IEntityDto>>
                {
                    {new List<string> {_filePathOptions.LocalNumeric, _ftpRequestOptions.NumericCredential}, numbers},
                    {new List<string> {_filePathOptions.LocalText, _ftpRequestOptions.TextCredential}, texts},
                    {new List<string> {_filePathOptions.LocalDate, _ftpRequestOptions.DateCredential}, dates}
                };

                foreach (var (config, items) in dictionary)
                {
                    await using (var transaction = await _context.Database.BeginTransactionAsync())
                    {
                        try
                        {
                            var fromFile = Generators.FirstCaseUpperStringGenerator(config[0]);
                            var credential = config[1].ToLower();
                            _logger.LogInformation($"Checking process is started for {fromFile}s service");

                            var entityDtoList = items as IEntityDto[] ?? items.ToArray();
                            if (!entityDtoList.Any())
                            {
                                _logger.LogInformation($"There is not any data found for {fromFile}s service!");
                                continue;
                            }

                            await DeleteOldData(fromFile + 's');

                            var lastConfig = await WriteListAndReturnLastConfig(entityDtoList, fromFile.ToLower());
                            if (lastConfig is null) continue;

                            // lastConfig.Item1 is {from} and lastConfig.Item2 is {to}
                            await _ftpHelpers.UploadFile(_ftpRequestOptions.HostName, credential, lastConfig.Item1,
                                lastConfig.Item2);
                            await transaction.CommitAsync();
                        }
                        catch (Exception e)
                        {
                            _logger.LogError(e, e.Message);
                            await transaction.RollbackAsync();
                        }
                        finally
                        {
                            await transaction.DisposeAsync();
                        }
                    }
                }
            }
            finally
            {
                _semLock.Release();
            }
        }

        private async Task<Tuple<string, string>> WriteListAndReturnLastConfig(IEnumerable<IEntityDto> list, string fromFile)
        {
            try
            {
                var from = $"{_filePathOptions.TempDataFolder}\\{fromFile}.txt";
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

                    await writer.DisposeAsync();
                }

                var to = $"{fromFile}s-{Guid.NewGuid()}.txt";

                return Tuple.Create(from, to);
            }
            catch (Exception e)
            {
                _logger.LogError(e, e.Message);
                return null;
            }
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

        public async Task DeleteOldData(string tableName)
        {
            await _context.ClearSpecificTable(tableName);
            _logger.LogInformation($"All proper entities are cleared for {tableName}!");
        }

        public void Dispose()
        {
            
        }
    }
}