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
    /// Base class containing information about backup progress.
    /// </summary>
    public abstract class BackupEventInfo
    {
        /// <summary>
        /// Event message.
        /// </summary>
        public string Message { get; internal set; }

        /// <summary>
        /// Progress of backup at the time the event was raised.
        /// </summary>
        public ProgressInfo BackupProgress { get; internal set; }

        /// <summary>
        /// Default constructor that initialized the event info with the progress information.
        /// </summary>
        /// <param name="progress">Progress of backup when the event was raised.</param>
        public BackupEventInfo(ProgressInfo progress)
        {
            BackupProgress = progress;
        }
    }
}
