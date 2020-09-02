using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EO.DatabaseContext
{
    public class CustomerContainer
    {
        public CustomerContainer()
        {

        }

        public long CustomerContainerId { get; set; }
        public long CustomerId { get; set; }
        public string Label { get; set; }
        public long ImageId { get; set; }

        //public virtual ICollection<ContainerImageMap> ContainerImageMap { get; set; }
    }
}
