
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
    public class WorkOrderResponse : ApiResponse
    {
        public WorkOrderDTO WorkOrder { get; set; }
        public List<WorkOrderImageMapDTO> ImageMap { get; set; }
        public List<WorkOrderInventoryMapDTO> WorkOrderList { get; set; }
        public List<NotInInventoryDTO> NotInInventory { get; set; }
        public List<GetArrangementResponse> Arrangements { get; set; }

        public WorkOrderResponse()
        {
            WorkOrder = new WorkOrderDTO();
            ImageMap = new List<WorkOrderImageMapDTO>();
            WorkOrderList = new List<WorkOrderInventoryMapDTO>();
            NotInInventory = new List<NotInInventoryDTO>();
            Arrangements = new List<GetArrangementResponse>();
        }
    }
}
