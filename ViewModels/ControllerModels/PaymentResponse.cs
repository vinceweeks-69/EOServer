﻿using Android.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ViewModels.ControllerModels
{
    [Serializable]
    [Preserve(AllMembers = true)]
    public class PaymentResponse : ApiResponse
    {
        public PaymentResponse()
        {
            success = false;
            ccConfirm = String.Empty;
            StripeChargeId = String.Empty;
        }
        public string StripeChargeId { get; set; }
        public string ccConfirm { get; set; }
        public bool success { get; set; }
    }
}
