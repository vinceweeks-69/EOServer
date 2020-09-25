
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
    public class FoliageInventoryDTO
    {
        public FoliageDTO Foliage { get; set; }

        public InventoryDTO Inventory { get; set; }

        public long ImageId { get; set; }

        public FoliageInventoryDTO()
        {
            Foliage = new FoliageDTO();

            Inventory = new InventoryDTO();

            ImageId = 0;
        }

        public FoliageInventoryDTO(FoliageDTO foliage, InventoryDTO inventory, long imageId)
        {
            Foliage = foliage;

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
                    TypeId = Foliage.FoliageTypeId,
                    InventoryTypeId = Inventory.InventoryTypeId,
                    Name = Foliage.FoliageName,
                    Size = Foliage.FoliageSize,
                    ServiceCodeId = Inventory.ServiceCodeId,
                    ServiceCode = Inventory.ServiceCodeName,
                    ImageId = ImageId
                };

        }
    }
}
