namespace FTPBasedSystem.DOMAINENTITIES.DTOs
{
    public class TextDto : IEntityDto
    {
        public string TextValue { get; set; }

        public override string ToString()
        {
            return TextValue;
        }
    }
}