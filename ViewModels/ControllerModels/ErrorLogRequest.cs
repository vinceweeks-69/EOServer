using Android.Runtime;
using System;
using ViewModels.DataModels;

namespace ViewModels.ControllerModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class ErrorLogRequest
    {
        public ErrorLogRequest()
        {
            ErrorLog = new ErrorLogDTO();
        }
        public ErrorLogDTO ErrorLog { get; set; }
    }
}
