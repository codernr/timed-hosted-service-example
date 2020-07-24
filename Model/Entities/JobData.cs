using System;
using System.ComponentModel.DataAnnotations;

namespace TimedHostedServiceExample.Model.Entities
{
    public class JobData
    {
        [Key]
        public Guid Id { get; set; }
        
        [Required]
        public int Delay { get; set; }
    }
}