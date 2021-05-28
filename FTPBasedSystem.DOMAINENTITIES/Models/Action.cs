using System;
using System.ComponentModel.DataAnnotations;
using FTPBasedSystem.DOMAINENTITIES.Models.Base;

namespace FTPBasedSystem.DOMAINENTITIES.Models
{
    public class Action : BaseEntity
    {
        [Required]
        public string Model { get; set; } 
        [Required]
        public DateTime ExecutedAt { get; set; }
    }
}