using System.Collections.Generic;
using System.Linq;
using System;

namespace BackupCore
{
    /// <summary>
    /// Location being backed up.
    /// </summary>
    public class BackupLocation
    {
        /// <summary>
        /// Directory path.
        /// </summary>
        public string Location { get; set; }
        /// <summary>
        /// Priority (or order) of this backup location.
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        /// <param name="location">Directory path.</param>
        /// <param name="priority">Priority (the lower the number, the higher the priority) with which the backup is to be crated.</param>
        public BackupLocation(string location, int priority)
        {
            Location = location;
            Priority = priority;
        }
        /// <summary>
        /// Displays the path of the backed up folder along with its priority.
        /// </summary>
        public override string ToString()
        {
            return string.Format("{0} ({1})", Location, Priority);
        }
    }
}
