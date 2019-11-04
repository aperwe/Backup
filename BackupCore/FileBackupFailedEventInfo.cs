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
    /// Event notification about a file that could not be backed up.
    /// </summary>
    public class FileBackupFailedEventInfo : BackupEventInfo
    {
        /// <summary>
        /// Recorded reason for failure.
        /// </summary>
        public IOException FailureReason { get; private set; }

        /// <summary>
        /// Path of the source file being backed up.
        /// </summary>
        public string SourcePath { get; set; }

        /// <summary>
        /// Path to the backed-up location of the source file.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// Default constructor that initialized the event info with the progress information.
        /// </summary>
        /// <param name="progress">Progress of backup when the event was raised.</param>
        /// <param name="reason">Reason of the failure, given as IOException.</param>
        public FileBackupFailedEventInfo(ProgressInfo progress, IOException reason)
            : base(progress)
        {
            FailureReason = reason;
        }
    }
}
