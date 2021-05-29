using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.DOMAINENTITIES.Models
{
    public class Text : BaseEntity, IEntityModel
    {
        public string TextValue { get; set; }

        public override string ToString()
        {
            return TextValue;
        }
    }
}