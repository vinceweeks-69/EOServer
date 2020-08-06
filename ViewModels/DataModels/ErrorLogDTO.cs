using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{
    public class ErrorLogDTO
    {
        public long ErrorLogID { get; set; }

        public string Message { get; set; }

        public string Payload { get; set; }

        public DateTime Date { get; set; }
    }
}
