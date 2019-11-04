using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupCore
{
    /// <summary>
    /// Type of backup that can be performed.
    /// </summary>
    public enum BackupType
    {
        /// <summary>
        /// Default backup type. Full, ignores the current value of 'Archive' attribute but resets 'Archive' attribute on every file backed up.
        /// </summary>
        Full,
        /// <summary>
        /// Incremental backup type. Only files that have 'Archive' attribute set are backed up; the attribute gets reset after successful backup of a file.
        /// </summary>
        Incremental
    }
}
