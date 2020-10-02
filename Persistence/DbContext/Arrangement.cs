using System;
using System.Collections.Generic;

namespace EO.DatabaseContext
{
    public partial class Arrangement
    {
        public Arrangement()
        {
            ArrangementImageMap = new HashSet<ArrangementImageMap>();
            ArrangementInventoryMap = new HashSet<ArrangementInventoryMap>();
        }

        public long ArrangementId { get; set; }
        public string ArrangementName { get; set; }
        public string DesignerName { get; set; }
        public int _180or360 {get;set;}
        public int Container { get; set; }
        public long? CustomerContainerId { get; set; }
        public string LocationName { get; set; }
        public DateTime UpdateDate { get; set; }
        public long? ServiceCodeId { get; set; }
        public int IsGift { get; set; }
        public string GiftMessage { get; set; }
        public virtual ServiceCode ServiceCode { get; set; }
        public virtual ICollection<ArrangementImageMap> ArrangementImageMap { get; set; }
        public virtual ICollection<ArrangementInventoryMap> ArrangementInventoryMap { get; set; }
    }
}
