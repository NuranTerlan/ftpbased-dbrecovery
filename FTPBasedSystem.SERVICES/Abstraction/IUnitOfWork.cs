namespace FTPBasedSystem.SERVICES.Abstraction
{
    public interface IUnitOfWork
    {
        ICustomLoggingService CustomLogging { get; }
        IDateService Date { get; }
        INumericService Numeric { get; }
        ITextService Text { get; }
    }
}