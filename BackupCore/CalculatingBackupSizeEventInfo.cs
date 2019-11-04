using System;

namespace BackupCore
{
    /// <summary>
    /// Event info sent to clients when calculating total file size of backed-up files is in progress.
    /// </summary>
    public class CalculatingBackupSizeEventInfo : BackupEventInfo
    {
        /// <summary>
        /// Total - calculated so far - size of backup.
        /// </summary>
        public long TotalSize { get; set; }

        /// <summary>
        /// Default constructor that initialized the event info with the progress information.
        /// </summary>
        /// <param name="progress">Progress of backup when the event was raised.</param>
        /// <param name="totalSize">Total size calculated so far.</param>
        public CalculatingBackupSizeEventInfo(ProgressInfo progress, Int64 totalSize)
            : base(progress)
        {
            TotalSize = totalSize;
        }
    }
}
