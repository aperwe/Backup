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
    /// Contains notification about the overall backup operation being finished.
    /// </summary>
    public class BackupFinishedEventInfo : BackupEventInfo
    {
        /// <summary>
        /// Default constructor that initialized the event info with the progress information.
        /// </summary>
        /// <param name="progress">Progress of backup when the event was raised.</param>
        public BackupFinishedEventInfo(ProgressInfo progress)
            : base(progress)
        {

        }
    }
}
