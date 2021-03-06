﻿using Android.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ViewModels.DataModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class WorkOrderInventoryMapDTO 
    {
        public long WorkOrderInventoryMapId { get; set; }

        public long WorkOrderId { get; set; }

        public long InventoryId { get; set; }

        public string InventoryName { get; set; }

        public int Quantity { get; set; }

        public string Size { get; set; }

        public long? GroupId { get; set; }

        public string NotInInventoryName { get; set; }

        public string NotInInventorySize { get; set; }

        public decimal NotInInventoryPrice { get; set; }
    }
}
