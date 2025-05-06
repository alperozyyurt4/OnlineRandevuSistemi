using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OnlineRandevuSistemi.Core.Enums;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class Notification : BaseEntity
    {
        public string UserId { get; set; }
        public int? AppointmentId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime? ReadDate { get; set; }

        public virtual AppUser User { get; set; }
        public virtual Appointment Appointment { get; set; }
    }
}
