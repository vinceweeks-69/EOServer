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
    public class ShipmentInventoryImageMapDTO
    {
        public ShipmentInventoryImageMapDTO()
        {

        }

        public ShipmentInventoryImageMapDTO(long shipmentInventoryImageMapId, long shipmentInventoryMapId, long imageId, byte[] imageData)
        {
            ShipmentInventoryImageMapId = shipmentInventoryImageMapId;
            ShipmentInventoryMapId = shipmentInventoryMapId;
            ImageId = imageId;
            ImageData = imageData;
        }

        public long ShipmentInventoryImageMapId { get; set; }

        public long ShipmentInventoryMapId { get; set; }

        public long ImageId { get; set; }

        public byte[] ImageData { get; set; }
    }
}
