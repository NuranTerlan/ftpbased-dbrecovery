namespace FTPBasedSystem.DOMAINENTITIES.DTOs
{
    public class NumberDto : IEntityDto
    {
        public int NumberValue { get; set; }

        public override string ToString()
        {
            return $"{NumberValue}";
        }
    }
}