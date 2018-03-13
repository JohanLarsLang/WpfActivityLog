using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ActivityLog
{
    public class Day
    {
        public DateTime Date { get; set; }

        public Day(DateTime date)
        {
            Date = date;    
        }
    }
}
