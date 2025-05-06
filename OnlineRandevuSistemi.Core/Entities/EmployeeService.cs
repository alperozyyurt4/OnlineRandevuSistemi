using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRandevuSistemi.Core.Entities
{
    public class EmployeeService : BaseEntity
    {
        public int EmployeeId { get; set; }
        public int ServiceId { get; set; }
        public decimal? CustomPrice { get; set; }

        public virtual Employee Employee { get; set; }

        public virtual Service Service { get; set; }
    }
}
