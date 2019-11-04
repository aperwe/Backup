using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupCore
{
    /// <summary>
    /// Convenience methods.
    /// </summary>
    public static class BackupCoreExtensions
    {
        /// <summary>
        /// Finds and returns the first item that has lower priority than <paramref name="current"/>.
        /// </summary>
        /// <param name="me"/>
        /// <param name="current">Item, in relation to which to find another item with lower priority.</param>
        public static BackupLocation FirstLowerPriorityLocation(this IEnumerable<BackupLocation> me, BackupLocation current)
        {
            return me.Where(c => c.Priority < current.Priority).OrderByDescending(k => k.Priority).First();
        }
        
        /// <summary>
        /// Finds and returns the first item that has higher priority than <paramref name="current"/>.
        /// </summary>
        /// <param name="me"/>
        /// <param name="current">Item, in relation to which to find another item with higher priority.</param>
        public static BackupLocation FirstHigherPriorityLocation(this IEnumerable<BackupLocation> me, BackupLocation current)
        {
            return me.Where(c => c.Priority > current.Priority).OrderBy(k => k.Priority).First();
        }
    }
}
