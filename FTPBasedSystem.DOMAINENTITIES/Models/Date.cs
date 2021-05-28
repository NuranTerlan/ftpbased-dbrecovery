using System;
using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.DOMAINENTITIES.Models
{
    public class Date : BaseEntity, IEntityModel
    {
        public DateTime DateValue { get; set; }
    }
}