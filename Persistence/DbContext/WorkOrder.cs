﻿using System;
using System.Collections.Generic;

namespace EO.DatabaseContext
{
    public partial class WorkOrder
    {
        public WorkOrder()
        {
            WorkOrderInventoryMap = new HashSet<WorkOrderInventoryMap>();
        }

        public long WorkOrderId { get; set; }
        public string PersonInitiator { get; set; }
        public string PersonReceiver { get; set; }
        public string PersonDelivery { get; set; }
        public string DeliveryReceiver { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? ClosedDate { get; set; }
        public string Comments { get; set; }
        public bool Paid { get; set; }
        public bool IsSiteService { get; set; }
        public bool IsDelivery { get; set; }
        public bool IsCancelled { get; set; }
        public long CustomerId { get; set; }
        public long SellerId { get; set; }
        public int OrderType { get; set; }
        public int DeliveryType { get; set; }
        public long DeliveryUserId { get; set; }
        public long DeliveryRecipientId { get; set; }
        public bool Delivered { get; set; }

        public virtual ICollection<WorkOrderInventoryMap> WorkOrderInventoryMap { get; set; }

        public virtual WorkOrderPayment WorkOrderPayment { get; set; }
    }
}
