using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using BackupCore;
using System.Windows.Threading;
using System.Windows.Forms;

namespace BackupClient
{
    /// <summary>
    /// Helper methods to deal with <see cref="DispatcherObject"/> objects.
    /// </summary>
    public static class DispatcherObjectExtensions
    {
        /// <summary>
        /// Invokes the specified delegate on UI thread.
        /// </summary>
        public static void Invoke(this DispatcherObject me, MethodInvoker code)
        {
            me.Dispatcher.Invoke(code);
        }
    }
}
