using System;
using System.ComponentModel.DataAnnotations;
using FTPBasedSystem.DOMAINS.Models.Base;

namespace FTPBasedSystem.DOMAINS.Models
{
    public class Action : BaseEntity
    {
        [Required]
        public string Model { get; set; }
        [Required]
        public DateTime ExecutedAt { get; set; }
    }
}