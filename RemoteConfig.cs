using System;
using System.Collections.Generic;

namespace Eden.Update
{
    [Serializable]
    public class RemoteConfig
    {
        /// <summary>
        /// remote enable update or not.
        /// </summary>
        public bool Enable { get; set; }

        public bool ForceUpdate { get; set; }
        
        /// <summary>
        /// update file list.
        /// </summary>
        public UpdateFileList UpdateFileList { get; set; }

        public RemoteConfig()
        {
            UpdateFileList = new UpdateFileList();
            Enable = true;
        }
    }
}
