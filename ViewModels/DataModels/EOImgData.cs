﻿using Android.Runtime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ViewModels.DataModels
{

    [Serializable]
    [Preserve(AllMembers = true)]
    public class EOImgData
    {
        public EOImgData()
        {
            isNewImage = true;
        }

        public EOImgData(byte[] imgData)
        {
            this.imgData = imgData;
        }

        public EOImgData(long imageId, byte[] imgData)
        {
            ImageId = imageId;
            this.imgData = imgData;
        }

        public long ImageId { get; set; }

        public byte[] imgData { get; set; }

        public string fileName { get; set; }

        public bool isNewImage { get; set; }
    }
}
