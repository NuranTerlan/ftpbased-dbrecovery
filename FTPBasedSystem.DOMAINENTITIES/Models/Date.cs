using System;
using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.DOMAINENTITIES.Models
{
    public class Date : BaseEntity, IEntityModel
    {
        public DateTime DateValue { get; set; }

        public override string ToString()
        {
            return $"{DateValue:dddd, dd MMMM yyyy HH:mm:ss tt}";
        }
    }
}