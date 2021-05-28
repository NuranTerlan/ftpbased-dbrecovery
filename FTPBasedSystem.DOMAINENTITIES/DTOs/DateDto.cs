using System;
using System.Globalization;

namespace FTPBasedSystem.DOMAINENTITIES.DTOs
{
    public class DateDto : IEntityDto
    {
        public DateTime DateValue { get; set; }

        public override string ToString()
        {
            return $"{DateValue:dddd, dd MMMM yyyy HH:mm:ss tt}";
        }
    }
}