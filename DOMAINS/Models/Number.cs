using System.ComponentModel.DataAnnotations;
using FTPBasedSystem.DOMAINS.Models.Base;

namespace FTPBasedSystem.DOMAINS.Models
{
    public class Number : BaseEntity, IEntityModel
    {
        [Required]
        public int NumberValue { get; set; } 
    }
}