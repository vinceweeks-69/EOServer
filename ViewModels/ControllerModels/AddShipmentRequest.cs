
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
    public class AddShipmentRequest
    {
        public AddShipmentRequest()
        {
            ShipmentInventoryMap = new List<ShipmentInventoryMapDTO>();
        }

        public AddShipmentRequest(ShipmentDTO dto, List<ShipmentInventoryMapDTO> mapDTO, List<ShipmentInventoryImageMapDTO> imageMapDTO)
        {
            ShipmentDTO = dto;
            ShipmentInventoryMap = mapDTO;
            ShipmentInventoryImageMap = imageMapDTO;

        }
        public ShipmentDTO ShipmentDTO { get; set; }

        public List<ShipmentInventoryMapDTO> ShipmentInventoryMap { get; set; }

        public List<ShipmentInventoryImageMapDTO> ShipmentInventoryImageMap { get; set; }
    }
}
