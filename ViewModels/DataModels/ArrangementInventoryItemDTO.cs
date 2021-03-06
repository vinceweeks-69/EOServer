﻿
using Android.Runtime;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class ArrangementInventoryItemDTO : INotifyPropertyChanged
    {
        public ArrangementInventoryItemDTO()
        {

        }

        public ArrangementInventoryItemDTO(long arrangementId, long inventoryId, long inventoryTypeId, string inventoryName, long imageId)
        {
            ArrangementId = arrangementId;
            InventoryId = inventoryId;
            InventoryTypeId = inventoryTypeId;
            InventoryName = inventoryName;
            ImageId = imageId;
        }
        public long ArrangementId { get; set; }

        public long InventoryId { get; set; }

        public long InventoryTypeId { get; set; }

        public string InventoryName { get; set; }

        public int Quantity { get; set; }

        public long ImageId { get; set; }

        public string Size { get; set; }

        protected void OnPropertyChanged(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
