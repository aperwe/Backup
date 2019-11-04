using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BackupCore
{
    /// <summary>
    /// Bridge that wraps around <see cref="BackupConfiguration"/> class.
    /// </summary>
    public class DataBridge
    {
        /// <summary>
        /// Database that is wrapped by this class.
        /// </summary>
        private BackupConfiguration WrappedData { get; set; }

        /// <summary>
        /// Crates a new instance of the wrapper around strongly-typed data.
        /// </summary>
        public DataBridge()
        {
            WrappedData = new BackupConfiguration();
        }


        #region Wrapper methods

        /// <summary>
        /// Reads XML data into the database.
        /// </summary>
        public void ReadXml(string fileName)
        {
            WrappedData.ReadXml(fileName);
        }

        /// <summary>
        /// Writes XML data from the database into a persistent storage (disk file).
        /// </summary>
        /// <param name="fileName">The file name (including path) under which to write.</param>
        public void WriteXml(string fileName)
        {
            WrappedData.WriteXml(fileName);
        }

        /// <summary>
        /// Returns all backup locations configured in this database. They are ordered by priority.
        /// </summary>
        public IEnumerable<BackupLocation> AllBackupLocations
        {
            get
            {
                return WrappedData.GetAllBackupLocations().OrderBy(bl => bl.Priority);
            }
        }

        /// <summary>
        /// Adds a new backup location. Doesn't add empty or null strings and doesn't add duplicate locations.
        /// </summary>
        /// <param name="location">New location to be added</param>
        public void AddBackupLocation(string location)
        {
            WrappedData.AddBackupLocation(location);
            BackupLocationsUpdated();
        }

        /// <summary>
        /// Removes a backup location. Doesn't remove non-existing locations.
        /// </summary>
        /// <param name="location">Location to be removed.</param>
        public void RemoveBackupLocation(string location)
        {
            WrappedData.RemoveBackupLocation(location);
            BackupLocationsUpdated();

        }
        /// <summary>
        /// Gets or sets target folder where files will be backed up.
        /// </summary>
        public string BackupTargetFolder
        {
            get
            {
                return WrappedData.GetParam("BackupTargetFolder");
            }
            set
            {
                WrappedData.SetParam("BackupTargetFolder", value);
                BackupTargetFolderChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets the flag that indicates whether every file backed up should have 'A' (Archive) flag reset after successful backup.
        /// </summary>
        public bool ResetArchive
        {
            get
            {
                return WrappedData.GetBoolParam("ResetArchiveAttribute");
            }
            set
            {
                WrappedData.SetParam("ResetArchiveAttribute", value);
            }
        }

        /// <summary>
        /// Records a notification about a file being backed up.
        /// </summary>
        /// <param name="sourcePath">Path of the source file that has been backed up.</param>
        /// <param name="targetPath">Path, where the source file has been backed up.</param>
        public void AddBackedUpFile(string sourcePath, string targetPath)
        {
            WrappedData.AddBackedUpFile(sourcePath, targetPath);
        }

        /// <summary>
        /// Removes all records of backed up files, so that the store with backed up files is empty (for reuse).
        /// </summary>
        public void ClearBackedUpFiles()
        {
            WrappedData.BackedUpFiles.Clear();
        }

        /// <summary>
        /// Swaps priorities between two backed up folders.
        /// </summary>
        public void SwapPriorities(BackupLocation previous, BackupLocation next)
        {
            WrappedData.SwapPriorities(previous, next);
            BackupLocationsUpdated();
        }

        /// <summary>
        /// Type of backup.
        /// </summary>
        public BackupType BackupType
        {
            get
            {
                return WrappedData.GetEnumType<BackupType>();
            }
            set
            {
                WrappedData.SetParam("BackupType", value);
            }
        }
        #endregion

        #region Events
        /// <summary>
        /// Event raised when the list of backup locations is updated (either by adding or removing entries).
        /// </summary>
        public event Action BackupLocationsUpdated;
        /// <summary>
        /// Event raised when <see cref="BackupTargetFolder"/> property is updated.
        /// </summary>
        public event Action<string> BackupTargetFolderChanged;
        #endregion
    }
}
