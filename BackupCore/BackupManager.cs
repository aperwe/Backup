using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using BackupCore.DB;

namespace BackupCore
{
    /// <summary>
    /// Manages creation of backups.
    /// </summary>
    public class BackupManager
    {
        /// <summary>
        /// Run ID from the database correspoinding to the current backup session.
        /// </summary>
        private int DBRunID;
        /// <summary>
        /// Total size of the files in the backup.
        /// </summary>
        public Int64 TotalFileSize { get; private set; }

        /// <summary>
        /// Current file size of files that have been successfully backed up by this manager.
        /// </summary>
        public Int64 BackedUpFileSize { get; private set; }

        /// <summary>
        /// Date and time when the backup started.
        /// </summary>
        public DateTime DateStarted { get; private set; }

        /// <summary>
        /// Configuration that determines how backup is going to be carried out.
        /// It also records the results of the all operations.
        /// </summary>
        public DataBridge Configuration { get; set; }

        /// <summary>
        /// List of files to copy to backup.
        /// </summary>
        public Queue<FileInfo> Backlog { get; set; }

        /// <summary>
        /// Progress of the backup managed by this manager.
        /// </summary>
        private ProgressInfo BackupProgress { get; set; }

        /// <summary>
        /// Directory (under backup root) where the current backup is saved.
        /// </summary>
        private DirectoryInfo CurrentBackupDirectory { get; set; }

        /// <summary>
        /// Creates a backup manager that is based off the supplied configuration.
        /// </summary>
        /// <param name="configuration">Configuration that defines what data to backup and where.</param>
        public BackupManager(DataBridge configuration)
        {
            Configuration = configuration;
            Backlog = new Queue<FileInfo>();
            BackupProgress = new ProgressInfo();
        }

        /// <summary>
        /// Creates backup.
        /// </summary>
        public void CreateBackup()
        {
            ThreadPool.QueueUserWorkItem(CreateBackupAsync);
        }

        /// <summary>
        /// Creates a backup on separate thread.
        /// </summary>
        private void CreateBackupAsync(object state)
        {
            try
            {
                DBCreateNewRunID(); //On new computers this will fail
            }
            catch (Exception ex)
            {
            }

            BackupDetailedEvent += EventHandlerFileBackedUp;
            BackupFinished += EventHandlerBackupFinished;
            TotalFileSize = 0;
            BackedUpFileSize = 0;
            BackupStarted();
            DoBackup();
            BackupFinished();
        }

        #region Event handlers
        /// <summary>
        /// Called when another file has been backed up.
        /// </summary>
        void EventHandlerFileBackedUp(BackupEventInfo info)
        {
            if (info is FileBackupFailedEventInfo)
            {
                var currentInfo = (FileBackupFailedEventInfo)info;
                //using (BackupDBDataContext ctx = new BackupDBDataContext(@"Data Source=XENOBI\SQLEXPRESS;Initial Catalog=QBitsBackup;Integrated Security=True;Pooling=False"))
                //{
                //    var fileFailedStateID = (from state in ctx.FileStates
                //                         where state.Name == "Failed"
                //                         select state.ID).First();
                //    File fileRecord = new File { RunID = DBRunID, FullPath = currentInfo.TargetPath, StateID = fileFailedStateID };

                //    if (string.IsNullOrEmpty(fileRecord.FullPath)) fileRecord.FullPath = string.Empty; //Avoid a SqlException

                //    ctx.Files.InsertOnSubmit(fileRecord);
                //    ctx.SubmitChanges();
                //}
            }
            if (info is FileBackupSuccessfulEventInfo)
            {
                var currentInfo = (FileBackupSuccessfulEventInfo)info;
                //using (BackupDBDataContext ctx = new BackupDBDataContext(@"Data Source=XENOBI\SQLEXPRESS;Initial Catalog=QBitsBackup;Integrated Security=True;Pooling=False"))
                //{
                //    var fileOKStateID = (from state in ctx.FileStates
                //                         where state.Name == "Copied"
                //                         select state.ID).First();
                //    File fileRecord = new File { RunID = DBRunID, FullPath = currentInfo.TargetPath, StateID = fileOKStateID };
                //    ctx.Files.InsertOnSubmit(fileRecord);
                //    ctx.SubmitChanges();
                //}
                Configuration.AddBackedUpFile(currentInfo.SourcePath, currentInfo.TargetPath);
            }
        }

        /// <summary>
        /// Called when backup of the last file has finished.
        /// </summary>
        void EventHandlerBackupFinished()
        {
            try
            {
                WriteFinalBackupState();
            }
            catch (DirectoryNotFoundException ex) // May be thrown when backup disk was removed or turned off and it was not possible to locate the target folder.
            {
                BackupDetailedEvent(new FileBackupFailedEventInfo(BackupProgress, ex) { Message = "Backup unsuccessful. Could not write backup state database to the target backup directory." });
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion

        /// <summary>
        /// Writes backup state data (database) to the target directory after backup has finished.
        /// </summary>
        private void WriteFinalBackupState()
        {
            if (CurrentBackupDirectory == null)
            {
                BackupDetailedEvent(new BackupFinishedEventInfo(BackupProgress));
                return;
            }

            Configuration.WriteXml(Path.Combine(CurrentBackupDirectory.FullName, "Backup.xml"));
        }

        /// <summary>
        /// Performs actual backup.
        /// </summary>
        private void DoBackup()
        {
            try
            {
                DirectoryInfo backupRoot = new DirectoryInfo(Configuration.BackupTargetFolder);
                if (!backupRoot.Exists)
                {
                    BackupDetailedEvent(new BackupStartedEventInfo(BackupProgress) { Message = string.Format("Creating {0} because it does not exist.", backupRoot.FullName) });
                    backupRoot.Create();
                }
                DateStarted = DateTime.Now;
                string backupName = "Undetermined backup type";

                switch (Configuration.BackupType)
                {
                    case BackupType.Full:
                        {
                            backupName = "Full";
                        } break;

                    case BackupType.Incremental:
                        {
                            backupName = "Incremental";
                        } break;
                }

                // Year-Month-Day hour hh.mm.ss
                var backupFolderName = string.Format("{6} backup on {0:D4}-{1:D2}-{2:D2} hour {3:D2}.{4:D2}.{5:D2}",
                    DateStarted.Year,
                    DateStarted.Month,
                    DateStarted.Day,
                    DateStarted.Hour,
                    DateStarted.Minute,
                    DateStarted.Second,
                    backupName
                    );

                BackupDetailedEvent(new BackupFinishedEventInfo(BackupProgress) { Message = string.Format("Creating current backup subfolder {0}.", backupFolderName) });
                CurrentBackupDirectory = backupRoot.CreateSubdirectory(backupFolderName);
                if (Configuration.AllBackupLocations.Count() == 0) return;

                #region Calculate common root directory for all folders being backed up
                DirectoryInfo commonRoot = new DirectoryInfo(Configuration.AllBackupLocations.First().Location); //Common root for all backups. Here, we assume all backups are from a single hard drive.

                foreach (var backedUpDirectory in Configuration.AllBackupLocations.Except(new[] { Configuration.AllBackupLocations.First() }))
                {
                    DirectoryInfo bd = new DirectoryInfo(backedUpDirectory.Location);
                    if (bd.FullName == commonRoot.FullName) continue;
                    FindCommonParentDirectory(ref commonRoot, commonRoot, bd);
                }
                #endregion
                TotalFileSize = 0;

                foreach (var backedUpItem in Configuration.AllBackupLocations)
                {
                    try
                    {
                        DirectoryInfo item = new DirectoryInfo(backedUpItem.Location);
                        var backedUpItems = item.GetFiles("*", SearchOption.AllDirectories);
                        FileInfo[] archivableItems = null;
                        switch (Configuration.BackupType)
                        {
                            case BackupType.Full: archivableItems = backedUpItems; break;
                            case BackupType.Incremental: archivableItems = backedUpItems.Where(file => (file.Attributes & FileAttributes.Archive) == FileAttributes.Archive).ToArray(); break;
                        }
                        Array.ForEach(archivableItems, Backlog.Enqueue);
                        int spowalniaczOdpalania = 0;

                        #region Calculate the total size of backup
                        Array.ForEach(archivableItems, (x) =>
                        {
                            TotalFileSize += x.Length;
                            spowalniaczOdpalania++;
                            if (spowalniaczOdpalania % 50 == 0) BackupDetailedEvent(new CalculatingBackupSizeEventInfo(BackupProgress, TotalFileSize));
                        });
                        #endregion

                        //BackupProgress.Total += archivableItems.Length;
                        BackupProgress.Total += TotalFileSize;
                    }
                    catch (DirectoryNotFoundException ex)
                    {
                        BackupDetailedEvent(new FileBackupFailedEventInfo(BackupProgress, ex) { Message = string.Format("{0} Check your backup configuration. This directory will not be included in your backup.", ex.Message) });
                    }
                }

                BackupDetailedEvent(new CalculatingBackupSizeEventInfo(BackupProgress, TotalFileSize)); //Make sure the client gets the final number event always.

                Configuration.ClearBackedUpFiles();
                StartCopyFrom(Backlog, commonRoot, CurrentBackupDirectory);
            }
            catch (DirectoryNotFoundException ex)
            {
                BackupDetailedEvent(new FileBackupFailedEventInfo(BackupProgress, ex) { Message = "Could not create backup directory. Backup has not been performed. Make sure the destination drive is available and has enough space and then try again." });
            }
            catch (IOException ex)
            {
                BackupDetailedEvent(new FileBackupFailedEventInfo(BackupProgress, ex) { Message = "Could not create backup directory. Backup has not been performed. Make sure the destination drive is available and has enough space and then try again." });
            }
        }

        /// <summary>
        /// Begins copying of data from the specified queue to the specified target backup directory.
        /// </summary>
        /// <param name="backlog">Queue of files to be copied.</param>
        /// <param name="commonRoot">Common root of all possible files being backed up.</param>
        /// <param name="currentBackupDirectory">Destination location of all backup files.</param>
        private void StartCopyFrom(Queue<FileInfo> backlog, DirectoryInfo commonRoot, DirectoryInfo currentBackupDirectory)
        {
            while (backlog.Count > 0)
            {
                FileInfo currentItem = null;
                lock (backlog)
                {
                    currentItem = backlog.Dequeue();
                }
                if (currentItem != null) CopyOneItem(currentItem, commonRoot, currentBackupDirectory);
            }
        }

        /// <summary>
        /// Copies one item from its original (base) location to the backup location.
        /// </summary>
        private void CopyOneItem(FileInfo currentItem, DirectoryInfo commonRoot, DirectoryInfo currentBackupDirectory)
        {
            var srcFullName = currentItem.FullName;
            var srcDir = currentItem.Directory;
            var commonDirNameMayBeAbsolute = commonRoot.FullName; //Never with trailing backslash.
            if (commonDirNameMayBeAbsolute.EndsWith("\\")) commonDirNameMayBeAbsolute = commonDirNameMayBeAbsolute.TrimEnd('\\');
            var tgtDirX = srcDir.FullName.Replace(commonDirNameMayBeAbsolute, currentBackupDirectory.FullName);
            var tgtFullName = Path.Combine(tgtDirX, currentItem.Name);
            DirectoryInfo tgtDir = new DirectoryInfo(Path.GetDirectoryName(tgtFullName));
            try
            {
                if (!tgtDir.Exists)
                {
                    tgtDir.Create();
                }

                //BackupProgress.Current++;
                BackupProgress.Current += currentItem.Length; //This is updated always, even if actual copy may fail.

                System.IO.File.Copy(srcFullName, tgtFullName, true);
                BackedUpFileSize += currentItem.Length; //This is updated only when the backup of the current file was successful.

                BackupDetailedEvent(new FileBackupSuccessfulEventInfo(BackupProgress, srcFullName, tgtFullName) { Message = string.Format("Copied from \"{0}\" to \"{1}\".", srcFullName, tgtFullName) });
                if (Configuration.ResetArchive)
                {
                    System.IO.File.SetAttributes(srcFullName, System.IO.File.GetAttributes(srcFullName) & ~FileAttributes.Archive);
                }
            }
            catch (IOException ex)
            {
                BackupDetailedEvent(new FileBackupFailedEventInfo(BackupProgress, ex)
                {
                    Message = string.Format("Failed to copy file {0} to {1}...", srcFullName, tgtFullName),
                    SourcePath = srcFullName,
                    TargetPath = tgtFullName
                });
            }
        }

        #region Events

        /// <summary>
        /// Event raised when the backup thread starts processing.
        /// </summary>
        public event Action BackupStarted;

        /// <summary>
        /// Event raised when the backup thread finishes processing.
        /// </summary>
        public event Action BackupFinished;

        /// <summary>
        /// Detailed events that are happening during backup.
        /// Started and Finished events are not included here.
        /// <para/><see cref="BackupEventInfo"/> parameter is the event description.
        /// </summary>
        public event Action<BackupEventInfo> BackupDetailedEvent;

        #endregion

        /// <summary>
        /// Finds common directory between <paramref name="left"/> and <paramref name="right"/> directories.
        /// Identified common root is placed in <paramref name="commonRoot"/>.
        /// </summary>
        private void FindCommonParentDirectory(ref DirectoryInfo commonRoot, DirectoryInfo left, DirectoryInfo right)
        {
            if (left == null) return;
            if (right == null) return;
            if (left.FullName == right.FullName)
            {
                commonRoot = left;
                return;
            }
            FindCommonParentDirectory(ref commonRoot, left.Parent, right);
            FindCommonParentDirectory(ref commonRoot, left, right.Parent);
        }

        /// <summary>
        /// Creates new RunID in a persistable (external) database. Currently it is our SQLEXPRESS instance.
        /// </summary>
        private void DBCreateNewRunID()
        {
            var userID = DbSqlWrapper.GetCurrentUserID();
            DBRunID = DbSqlWrapper.NewRunID(userID);
        }
    }
}
