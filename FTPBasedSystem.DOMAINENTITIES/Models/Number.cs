using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.DOMAINENTITIES.Models
{
    public class Number : BaseEntity, IEntityModel
    {
        public int NumberValue { get; set; }
    }
}