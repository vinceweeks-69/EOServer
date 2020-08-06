
using Android.Runtime;
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
    public class WorkOrderRequest
    {
        public WorkOrderRequest()
        {
            WorkOrder = new WorkOrderDTO();

            InventoryIdList = new List<long>();
        }
        public WorkOrderDTO WorkOrder { get; set; }

        public List<long> InventoryIdList { get; set; }
    }

    [Serializable]
    [Preserve(AllMembers = true)]
    public class WorkOrderImageIdResponse
    {
        public WorkOrderImageIdResponse()
        {
             ImageIdList = new List<long>();
        }

        public WorkOrderImageIdResponse(List<long> imageIds)
        {
            ImageIdList = imageIds;
        }

        public long WorkOrderId { get; set; }
        public List<long> ImageIdList { get; set; }
    }
}
