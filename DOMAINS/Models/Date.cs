using System;
using System.ComponentModel.DataAnnotations;
using FTPBasedSystem.DOMAINS.Models.Base;

namespace FTPBasedSystem.DOMAINS.Models
{
    public class Date : BaseEntity, IEntityModel
    {
        [Required]
        public DateTime DateValue { get; set; }
    }
}