using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using BackupCore;
using BackupClient.Properties;
using System.IO;
using System.Reflection;

namespace BackupClient
{
    /// <summary>
    /// Interaction logic for BackupClientApplication.xaml
    /// </summary>
    public partial class BackupClientApplication : Application
    {
        /// <summary>
        /// Location of the configuration data file. Absolute location. Initialized from configuration file.
        /// </summary>
        public string AbsoluteFileLocation { get; set; }

        /// <summary>
        /// Configuration as initialized from a backing store.
        /// </summary>
        public DataBridge GlobalConfiguration { get; set; }


        /// <summary>
        /// Handles application load event.
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            GlobalConfiguration = new DataBridge();
            TryLoadConfiguration(GlobalConfiguration);
        }


        private void TryLoadConfiguration(DataBridge configuration)
        {
            var relativeOrAbsoluteFileLocation = Environment.ExpandEnvironmentVariables(Settings.Default.ConfigurationLocation);
            var absoluteFileLocation = relativeOrAbsoluteFileLocation;
            if (!Path.IsPathRooted(relativeOrAbsoluteFileLocation))
            {
                absoluteFileLocation = new FileInfo(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), relativeOrAbsoluteFileLocation)).FullName;
            }
            AbsoluteFileLocation = absoluteFileLocation;
            if (System.IO.File.Exists(AbsoluteFileLocation))
            {
                configuration.ReadXml(AbsoluteFileLocation);
            }
        }

        /// <summary>
        /// Checks whether no data is about to be lost and then shuts down the application.
        /// If some data may be lost, it warns the user and allows to cancel the shutdown.
        /// </summary>
        public void ConditionalShutdown()
        {
            //TODO: Add logic here.
            Shutdown();
        }

        /// <summary>
        /// Global reference to this application.
        /// </summary>
        static public BackupClientApplication Global
        {
            get
            {
                return (BackupClientApplication)Application.Current;
            }
        }
    }
}
