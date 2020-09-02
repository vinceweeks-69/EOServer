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
    public class CustomerContainerRequest
    {
        public CustomerContainerRequest()
        {
            CustomerContainer = new CustomerContainerDTO();
        }

        public CustomerContainerRequest(CustomerContainerDTO dto)
        {
            CustomerContainer = dto;
        }

        public CustomerContainerDTO CustomerContainer {get; set;}
    }

    [Serializable]
    [Preserve(AllMembers = true)]
    public class CustomerContainerResponse : ApiResponse
    {
        public CustomerContainerResponse()
        {
            CustomerContainers = new List<CustomerContainerDTO>();
        }

        public CustomerContainerResponse(List<CustomerContainerDTO> dtoList)
        {
            CustomerContainers = dtoList;
        }

        public List<CustomerContainerDTO> CustomerContainers { get; set; }
    }
}
