using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class WorkOrderDTO
    {
        public long WorkOrderId { get; set; }

        public string Seller { get; set; }

        public string Buyer { get; set; }

        public string DeliveredBy { get; set; }

        public string DeliverTo { get; set; }

        public DateTime CreateDate { get; set; }

        public DateTime UpdateDate { get; set; }

        public DateTime DeliveryDate { get; set; }

        public DateTime ClosedDate { get; set; }

        public string Comments { get; set; }

        public bool Paid { get; set; }

        public bool IsSiteService { get; set; }

        public bool IsDelivery { get; set; }

        public bool IsCancelled { get; set; }

        public long CustomerId { get; set; }

        public long SellerId { get; set; }

        public long DeliveryRecipientId { get; set; }

        public int OrderType { get; set; }

        public int DeliveryType { get; set; }

        public long DeliveryUserId { get; set; }

        public bool Delivered { get; set; }
    }
}
