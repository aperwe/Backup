using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;

namespace BackupCore
{
    /// <summary>
    /// Contains notification about the overall backup operation being started.
    /// </summary>
    public class BackupStartedEventInfo : BackupEventInfo
    {
        /// <summary>
        /// Default constructor that initialized the event info with the progress information.
        /// </summary>
        /// <param name="progress">Progress of backup when the event was raised.</param>
        public BackupStartedEventInfo(ProgressInfo progress)
            : base(progress)
        {

        }
    }
}
