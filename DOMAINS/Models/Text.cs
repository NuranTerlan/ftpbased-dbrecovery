using System.ComponentModel.DataAnnotations;
using FTPBasedSystem.DOMAINS.Models.Base;

namespace FTPBasedSystem.DOMAINS.Models
{
    public class Text : BaseEntity, IEntityModel
    {
        [Required]
        public string TextValue { get; set; }
    }
}