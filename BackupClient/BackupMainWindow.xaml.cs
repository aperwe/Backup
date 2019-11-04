using BackupCore;
using System;
using System.Windows;
using Forms = System.Windows.Forms;
using System.Reflection;
using System.Windows.Controls;
using System.Linq;
using System.Threading;
using QBits.Intuition.Text;
using System.Data.SqlClient;

namespace BackupClient
{
    /// <summary>
    /// Main window of the Backup client applicaton.
    /// </summary>
    public partial class BackupMainWindow : Window
    {
        /// <summary>
        /// Dialog to select the target folder for backup.
        /// </summary>
        public Forms.FolderBrowserDialog BackupTargetChooser { get; set; }

        /// <summary>
        /// Dialog to add folders to be backed up.
        /// </summary>
        public Forms.FolderBrowserDialog BackedUpFolderChooser { get; set; }

        /// <summary>
        /// Backup manager that manages current backup operation.
        /// </summary>
        public BackupManager Manager { get; private set; }

        /// <summary>
        /// Default constructor.
        /// </summary>
        public BackupMainWindow()
        {
            InitializeComponent();
            BackupTargetChooser = new Forms.FolderBrowserDialog();
            BackedUpFolderChooser = new Forms.FolderBrowserDialog();
        }

        /// <summary>
        /// Event of initialized.
        /// </summary>
        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            BackupLocationsList.ItemsSource = BackupClientApplication.Global.GlobalConfiguration.AllBackupLocations;
            BackupClientApplication.Global.GlobalConfiguration.BackupLocationsUpdated += UpdatedBackupLocations;
            BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolderChanged += BackupTargetFolderChanged;
            if (!string.IsNullOrEmpty(BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder))
            {
                TargetOfBackup.Text = BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder;
            }
            FirstPanel.Content = "Ready";
            var us = Assembly.GetExecutingAssembly().GetName();
            Title = string.Format("Q-Bits backup ({0} v{1})", us.Name, us.Version.ToString());
            BackupLocationsList.SelectionChanged += EnableOrDisableButtonUp;
            BackupLocationsList.SelectionChanged += EnableOrDisableButtonDown;
            OptionResetArchive.IsChecked = BackupClientApplication.Global.GlobalConfiguration.ResetArchive;
        }

        void EnableOrDisableButtonUp(object sender, SelectionChangedEventArgs e)
        {
            var list = (System.Windows.Controls.ListView)sender;
            var first = list.SelectedIndex;
            ButtonMoveUp.IsEnabled = first > 0;
        }

        void EnableOrDisableButtonDown(object sender, SelectionChangedEventArgs e)
        {
            var list = (System.Windows.Controls.ListView)sender;
            var first = list.SelectedIndex;
            ButtonMoveDown.IsEnabled = (first + 1) < list.Items.Count;
        }

        #region Event handlers

        /// <summary>
        /// Event handler called when the user clicks 'Reset Archive attribute' checkbox.
        /// We use it to update configuration for persistency.
        /// </summary>
        private void ClickedOptionResetArchive(object sender, RoutedEventArgs e)
        {
            var checkBox = (CheckBox)sender;
            BackupClientApplication.Global.GlobalConfiguration.ResetArchive = checkBox.IsChecked.Value;
        }

        /// <summary>
        /// Event handler called when the user has selected a backup type: Full or Incremental.
        /// </summary>
        private void BackupTypeChanged(object sender, RoutedEventArgs e)
        {
            string tagToString = ((System.Windows.Controls.RadioButton)sender).Tag.ToString();
            BackupType newType = (BackupType)Enum.Parse(typeof(BackupType), tagToString);
            BackupClientApplication.Global.GlobalConfiguration.BackupType = newType;
        }

        /// <summary>
        /// Event handler called when the user selects menu Edit -> Clean backup history.
        /// <para/>It cleans the recorded backed up entries from the current database.
        /// </summary>
        private void MenuClickedCleanBackupHistory(object sender, RoutedEventArgs e)
        {
            BackupClientApplication.Global.GlobalConfiguration.ClearBackedUpFiles();
        }

        /// <summary>
        /// Event handler called when the user presses 'Move up' arrow button.
        /// </summary>
        private void ButtonClickedPriorityUp(object sender, RoutedEventArgs e)
        {
            var item = (BackupLocation)BackupLocationsList.SelectedItem;
            var indexToRestore = BackupLocationsList.SelectedIndex - 1;
            var previous = BackupClientApplication.Global.GlobalConfiguration.AllBackupLocations.FirstLowerPriorityLocation(item);
            BackupClientApplication.Global.GlobalConfiguration.SwapPriorities(item, previous);
            BackupLocationsList.SelectedIndex = indexToRestore;
        }

        /// <summary>
        /// Event handler called when the user presses 'Move down' arrow button.
        /// </summary>
        private void ButtonClickedPriorityDown(object sender, RoutedEventArgs e)
        {
            var item = (BackupLocation)BackupLocationsList.SelectedItem;
            var indexToRestore = BackupLocationsList.SelectedIndex + 1;
            var next = BackupClientApplication.Global.GlobalConfiguration.AllBackupLocations.FirstHigherPriorityLocation(item);
            BackupClientApplication.Global.GlobalConfiguration.SwapPriorities(item, next);
            BackupLocationsList.SelectedIndex = indexToRestore;
        }

        /// <summary>
        /// Event handler called when the user exits the text box that edits the target path.
        /// </summary>
        private void TargetOfBackupFinishedEditing(object sender, RoutedEventArgs e)
        {
            BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder = TargetOfBackup.Text;
        }

        /// <summary>
        /// Event hander called when the target folder for backup gets updated in the configuration database.
        /// </summary>
        /// <param name="newPath">New path to which backup target has been updated.</param>
        void BackupTargetFolderChanged(string newPath)
        {
            TargetOfBackup.Text = newPath;
        }

        /// <summary>
        /// Event handler called when 'Browse' button for target location is clicked.
        /// This method updates the destination folder for backup.
        /// </summary>
        private void ButtonClickedBrowseBackupTarget(object sender, RoutedEventArgs e)
        {
            BackupTargetChooser.Description = "Choose the destination folder where backup should be created.";
            BackupTargetChooser.ShowNewFolderButton = false;
            if (!string.IsNullOrEmpty(BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder))
            {
                BackupTargetChooser.SelectedPath = BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder;
            }
            Forms.DialogResult userResponse = BackupTargetChooser.ShowDialog();
            switch (userResponse)
            {
                case System.Windows.Forms.DialogResult.OK:
                    BackupClientApplication.Global.GlobalConfiguration.BackupTargetFolder = BackupTargetChooser.SelectedPath;
                    break;
                default:
                    System.Windows.MessageBox.Show("No target location for backup selected.", "Backup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    break;
            }
        }

        /// <summary>
        /// Event handler called when the list of backup locations is updated (either by adding or removing entries).
        /// </summary>
        void UpdatedBackupLocations()
        {
            BackupLocationsList.ItemsSource = BackupClientApplication.Global.GlobalConfiguration.AllBackupLocations;
        }

        /// <summary>
        /// Reacts to the user pressing 'Add' button.
        /// </summary>
        private void ButtonClickedAdd(object sender, RoutedEventArgs e)
        {
            BackedUpFolderChooser.Description = "Choose folder with your data you would like to add to back up.";
            BackedUpFolderChooser.ShowNewFolderButton = false;
            Forms.DialogResult userResponse = BackedUpFolderChooser.ShowDialog();
            switch (userResponse)
            {
                case System.Windows.Forms.DialogResult.OK:
                    AddBackupPath(BackedUpFolderChooser.SelectedPath);
                    break;
                default:
                    System.Windows.MessageBox.Show("No backup location selected.", "Backup", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                    break;
            }
        }

        /// <summary>
        /// Reacts to the user pressing 'Remove' button. Removes the selected backup location from backup.
        /// </summary>
        private void ButtonClickedRemove(object sender, RoutedEventArgs e)
        {
            var item = (BackupLocation)BackupLocationsList.SelectedItem;
            if (item == null) return;
            BackupClientApplication.Global.GlobalConfiguration.RemoveBackupLocation(item.Location);
        }

        /// <summary>
        /// Reacts to the user pressing 'Run' button.
        /// </summary>
        private void ButtonClickedRun(object sender, RoutedEventArgs e)
        {
            Manager = new BackupManager(BackupClientApplication.Global.GlobalConfiguration);
            BackupClientApplication.Global.GlobalConfiguration.ResetArchive = OptionResetArchive.IsChecked.Value;

            Manager.BackupStarted += EventBackupStarted;
            Manager.BackupFinished += EventBackupFinished;
            Manager.BackupDetailedEvent += EventBackupDetailedEvent;
            Manager.CreateBackup();
        }

        /// <summary>
        /// Event handler called when backup manager signals another file has been backed up (successfully or unsuccessfully).
        /// </summary>
        private void EventBackupDetailedEvent(BackupEventInfo e)
        {
            ThreadPool.QueueUserWorkItem(delegate
            {
                if (e is CalculatingBackupSizeEventInfo)
                {
                    var f = (CalculatingBackupSizeEventInfo)e;
                    this.Invoke(() =>
                        {
                            FourthPanel.Content = string.Format("Total backup size: {0}.", ByteSizeConverter.ConvertByteSizeToString(f.TotalSize));
                        });
                    return;
                }
                this.Invoke(() =>
                {
                    ActionLog.Items.Add(e.Message);
                    Progress.Value = e.BackupProgress.Progress;
                    ThirdPanel.Content = string.Format("Copied {0} out of {1} files so far ({2:P}).", e.BackupProgress.Current, e.BackupProgress.Total, e.BackupProgress.Progress);
                    if (e is FileBackupSuccessfulEventInfo)
                    {
                        var fbsei = (FileBackupSuccessfulEventInfo)e;
                        var fileInfo = new System.IO.FileInfo(fbsei.SourcePath);
                        var helper = new ProgressInfo { Total = Manager.TotalFileSize, Current = Manager.BackedUpFileSize };
                        var time = (long)(DateTime.Now - Manager.DateStarted).TotalSeconds;
                        var speedString = "Calculating backup speed...";
                        if (time > 1)
                        {
                            speedString = string.Format("Speed: {0}/s.", ByteSizeConverter.ConvertByteSizeToString(helper.Current / time));
                        }
                        FourthPanel.Content = string.Format("Copied {0}/{1} so far ({2:P}). {3}",
                            ByteSizeConverter.ConvertByteSizeToString(helper.Current),
                            ByteSizeConverter.ConvertByteSizeToString(helper.Total),
                            helper.Progress, speedString);
                    }
                });
            });
        }

        private void EventBackupStarted()
        {
            this.Invoke(() =>
            {
                string message = "Backup started";
                FirstPanel.Content = message;
                ActionLog.Items.Add(message);
                Progress.Value = 0;
                ThirdPanel.Content = null;
            });
        }

        private void EventBackupFinished()
        {
            this.Invoke(() =>
            {
                string message = "Backup finished";
                FirstPanel.Content = message;
                ActionLog.Items.Add(message);
                Progress.Value = 1;
                ThirdPanel.Content = null;
            });
        }

        /// <summary>
        /// Reacts to the user pressing 'Exit' button.
        /// </summary>
        private void ButtonClickedExit(object sender, RoutedEventArgs e)
        {
            BackupClientApplication.Global.ConditionalShutdown();
        }

        /// <summary>
        /// Reacts to the user pressing 'Quit' menu.
        /// </summary>
        private void MenuClickedQuit(object sender, RoutedEventArgs e)
        {
            BackupClientApplication.Global.ConditionalShutdown();
        }

        /// <summary>
        /// Reacts to the user pressing 'Save' menu.
        /// </summary>
        private void MenuClickedSave(object sender, RoutedEventArgs e)
        {
            BackupClientApplication.Global.GlobalConfiguration.WriteXml(BackupClientApplication.Global.AbsoluteFileLocation);
        }

        #endregion

        private void AddBackupPath(string path)
        {
            BackupClientApplication.Global.GlobalConfiguration.AddBackupLocation(path);
        }
    }
}