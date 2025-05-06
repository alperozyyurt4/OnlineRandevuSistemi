using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class Customer : BaseEntity
    {
        public string UserId { get; set; }
        public string PreferredLanguage { get; set; }
        public string notes { get; set; }

        public virtual AppUser User { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }
    }
}
