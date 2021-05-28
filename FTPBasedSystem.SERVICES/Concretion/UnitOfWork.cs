using AutoMapper;
using FTPBasedSystem.DATAACCESS.Data.Abstraction;
using FTPBasedSystem.SERVICES.Abstraction;

namespace FTPBasedSystem.SERVICES.Concretion
{
    public class UnitOfWork : IUnitOfWork
    {
        public UnitOfWork(ICustomLoggingService loggingService, IDateService dateService,
            INumericService numericService, ITextService textService)
        {
            CustomLogging = loggingService;
            Date = dateService;
            Numeric = numericService;
            Text = textService;
        }

        public ICustomLoggingService CustomLogging { get; }
        public IDateService Date { get; }
        public INumericService Numeric { get; }
        public ITextService Text { get; }
    }
}