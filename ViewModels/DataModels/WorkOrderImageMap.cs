using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{
    public class WorkOrderImageMapDTO
    {
        public long WorkOrderImageMapId { get; set; }

        public long WorkOrderId { get; set; }

        public long ImageId { get; set; }

        //always populated if calling AddWorkOrder and pictures have been taken
        //not necessarily populated on GetWorkOrders
        public byte[] ImageData { get; set; }
    }
}
