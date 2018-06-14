using StateOfNeo.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace StateOfNeo.Data.Models
{
    public class TimeEvent
    {
        [Key]
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }
        public TimeEventType Type { get; set; }
        public DateTime? LastDownTime { get; set; }
    }
}
