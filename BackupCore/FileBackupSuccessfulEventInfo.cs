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
    /// Information about a single file being successfully backed up.
    /// </summary>
    public class FileBackupSuccessfulEventInfo : BackupEventInfo
    {
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
        /// <param name="sourcePath">Path of the source file that has been backed up.</param>
        /// <param name="targetPath">Path, where the source file has been backed up.</param>
        public FileBackupSuccessfulEventInfo(ProgressInfo progress, string sourcePath, string targetPath)
            : base(progress)
        {
            SourcePath = sourcePath;
            TargetPath = targetPath;
        }
    }
}
