using System;

namespace BackupCore
{
    /// <summary>
    /// Contains information about copy progress.
    /// </summary>
    public class ProgressInfo
    {
        /// <summary>
        /// Current item that has been processed.
        /// </summary>
        public Int64 Current { get; set; }

        /// <summary>
        /// Total number of items that are to be processed. Should always be higher than or equal to <see cref="Current"/>.
        /// </summary>
        public Int64 Total { get; set; }

        /// <summary>
        /// Returns progress in the range 0..1. Handles the state when Total is 0 by returning 0.
        /// </summary>
        public double Progress
        {
            get
            {
                switch (Total)
                {
                    case 0:
                        if (Current == 0) return 0;
                        return 1;
                    default:
                        return ((double)Current) / Total;
                }
            }
        }
    }
}
