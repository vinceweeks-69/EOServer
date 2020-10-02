using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EO.DatabaseContext
{
    public partial class NotInInventory
    {
        public long NotInInventoryId { get; set; }
        public long WorkOrderId { get; set; }
        public long? ArrangementId { get; set; }
        public string NotInInventoryName { get; set; }
        public string NotInInventorySize { get; set; }
        public decimal NotInInventoryPrice { get; set; }
        public int NotInInventoryQuantity { get; set; }
    }
}
