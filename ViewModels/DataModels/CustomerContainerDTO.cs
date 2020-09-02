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
    public class CustomerContainerDTO
    {
        public long CustomerContainerId { get; set; }
        public long CustomerId { get; set; }
        public string Label { get; set; }
        public long ImageId { get; set; }
    }
}
