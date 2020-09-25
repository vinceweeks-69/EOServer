
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
    public class PlantInventoryDTO
    {
        public PlantDTO Plant { get; set; }

        public InventoryDTO Inventory { get; set; }

        public long ImageId { get; set; }

        public PlantInventoryDTO()
        {
            Plant = new PlantDTO();

            Inventory = new InventoryDTO();

            ImageId = 0;
        }

        public PlantInventoryDTO(PlantDTO plant, InventoryDTO inventory, long imageId)
        {
            Plant = plant;

            Inventory = inventory;

            ImageId = imageId;
        }

        public ArrangementInventoryFilteredItem ToFilteredItem()
        {
            return

                new ArrangementInventoryFilteredItem()
                {
                    Id = Inventory.InventoryId,
                    Type = Inventory.InventoryName,
                    TypeId = Plant.PlantTypeId,
                    InventoryTypeId = Inventory.InventoryTypeId,
                    Name = Plant.PlantName,
                    Size = Plant.PlantSize,
                    ServiceCodeId = Inventory.ServiceCodeId,
                    ServiceCode = Inventory.ServiceCodeName,
                    ImageId = ImageId
                };

        }
    }
}
