
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
    public class AddWorkOrderRequest
    {
        public AddWorkOrderRequest()
        {
            WorkOrder = new WorkOrderDTO();
            ImageMap = new List<WorkOrderImageMapDTO>();
            WorkOrderInventoryMap = new List<WorkOrderInventoryMapDTO>();
            NotInInventory = new List<NotInInventoryDTO>();
            Arrangements = new List<AddArrangementRequest>();
        }
        public WorkOrderDTO WorkOrder { get; set; }

        public List<WorkOrderImageMapDTO> ImageMap { get; set; }

        public List<WorkOrderInventoryMapDTO> WorkOrderInventoryMap { get; set; }

        public List<NotInInventoryDTO> NotInInventory { get; set; }

        public List<AddArrangementRequest> Arrangements { get; set; }
    }

    /// <summary>
    /// Save the work order first - then save any images
    /// </summary>
    [Serializable]
    [Preserve(AllMembers = true)]
    public class AddWorkOrderImageRequest
    {
        public long WorkOrderId { get; set; }

        public long ImageId { get; set; }

        public byte[] Image { get; set; }
    }

    [Serializable]
    [Preserve(AllMembers = true)]
    public class MarkWorkOrderPaidRequest
    {
        public MarkWorkOrderPaidRequest()
        {

        }

        public MarkWorkOrderPaidRequest(long workOrderId)
        {
            WorkOrderId = workOrderId;
        }
        public long WorkOrderId { get; set; }
    }
}
