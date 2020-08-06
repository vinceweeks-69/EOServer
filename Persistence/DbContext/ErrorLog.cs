using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EO.DatabaseContext
{
    public partial class ErrorLog
    {
        public long ErrorLogId { get; set; }

        public string Message { get; set; }

        public string Payload { get; set; }

        public DateTime Date { get; set; }
    }
}
