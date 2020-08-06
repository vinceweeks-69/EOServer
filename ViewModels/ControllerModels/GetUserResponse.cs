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
    public class GetUserResponse : ApiResponse
    {
        public List<UserDTO> Users { get; set; }

        public GetUserResponse()
        {
            Users = new List<UserDTO>();
        }

        public GetUserResponse(List<UserDTO> users)
        {
            Users = users;
        }
    }
}
