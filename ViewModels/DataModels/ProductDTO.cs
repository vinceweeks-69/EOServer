using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.DataModels
{

    //The new DTO base class for Container, Foliage, Material and Plant
    [Serializable]
    [Preserve(AllMembers = true)]
    public class ProductDTO
    {
        public long Id { get; set; }

        public string Name { get; set;}

        public long NameId { get; set; }

        public string Size { get; set; }

        public long TypeId { get; set; }

        public string TypeName { get; set; }
    }
}
