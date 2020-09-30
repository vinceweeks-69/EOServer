
using Android.Runtime;
using EO.ViewModels.ControllerModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ViewModels.DataModels;

namespace ViewModels.ControllerModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class GetWorkOrderSalesDetailRequest
    {
        public int DiscountType { get; set; }

        public decimal DiscountAmount { get; set; }

        public List<WorkOrderInventoryItemDTO> WorkOrderItems { get; set; }

        public List<NotInInventoryDTO> NotInInventory { get; set; }

        public GetWorkOrderSalesDetailRequest()
        {
            DiscountType = 0;
            DiscountAmount = 0.0M;
            WorkOrderItems = new List<WorkOrderInventoryItemDTO>();
            NotInInventory = new List<NotInInventoryDTO>();
        }

        public GetWorkOrderSalesDetailRequest(List<WorkOrderInventoryItemDTO> workOrderItems, int discountType, decimal discountAmount)
        {
            DiscountType = discountType;
            DiscountAmount = discountAmount;
            WorkOrderItems = workOrderItems;
            NotInInventory = new List<NotInInventoryDTO>();
        }

        public GetWorkOrderSalesDetailRequest(List<WorkOrderInventoryItemDTO> workOrderItems, List<NotInInventoryDTO> notInInventory,  int discountType, decimal discountAmount)
        {
            DiscountType = discountType;
            DiscountAmount = discountAmount;
            WorkOrderItems = workOrderItems;
            NotInInventory = notInInventory;
        }
    }

    [Serializable]
    [Preserve(AllMembers = true)]
    public class GetWorkOrderSalesDetailResponse : ApiResponse
    {
        public decimal SubTotal { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }
    }
}
