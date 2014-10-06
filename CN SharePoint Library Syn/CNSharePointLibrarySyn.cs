using System;
using System.Diagnostics;
using System.ServiceProcess;
using System.IO;
using System.Collections;
using System.Text;
using System.Runtime.InteropServices;

namespace CN_SharePoint_Library_Syn
{
    public partial class CNSharePointLibrarySyn : ServiceBase
    {

        public enum ServiceState
        {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct ServiceStatus
        {
            public long dwServiceType;
            public ServiceState dwCurrentState;
            public long dwControlsAccepted;
            public long dwWin32ExitCode;
            public long dwServiceSpecificExitCode;
            public long dwCheckPoint;
            public long dwWaitHint;
        };

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(IntPtr handle, ref ServiceStatus serviceStatus);

        public static FileSystemWatcher fileSysetmWatcher;

        public CNSharePointLibrarySyn()
        {
            InitializeComponent();
        }
        protected override void OnStart(string[] args)
        {
            // Update the service state to Start Pending.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_START_PENDING;
            serviceStatus.dwWaitHint = 100000;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

            Process();

            // Update the service state to Running.
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
        }

        public void Process()
        {
            Global.WriteLog(string.Format("Service Start At {0}", DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss")), EventLogEntryType.Information);
            try
            {
                //System.Diagnostics.Debugger.Launch();
                foreach (WatchedFolder sMonitorFolder in Global.DirectoryToWatch())
                {
                    if (Directory.Exists(sMonitorFolder.Networklocation))
                    {
                        fileSysetmWatcher = new FileSystemWatcher();
                        fileSysetmWatcher.Path = sMonitorFolder.Networklocation;
                        fileSysetmWatcher.Filter = "*.*";
                        fileSysetmWatcher.IncludeSubdirectories = true;
                        fileSysetmWatcher.NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite | NotifyFilters.DirectoryName;


                        fileSysetmWatcher.Created +=
                            new System.IO.FileSystemEventHandler((s, e) => fileSysetmWatcher_created(s, e, sMonitorFolder));

                        fileSysetmWatcher.Deleted +=
                            new System.IO.FileSystemEventHandler((s, e) => fileSysetmWatcher_deleted(s, e, sMonitorFolder));
                        fileSysetmWatcher.Renamed +=
                            new System.IO.RenamedEventHandler((s, e) => fileSysetmWatcher_Renamed(s, e, sMonitorFolder));

                        fileSysetmWatcher.Changed +=
                            new System.IO.FileSystemEventHandler((s, e) => fileSysetmWatcher_Changed(s, e, sMonitorFolder));

                        fileSysetmWatcher.EnableRaisingEvents = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Global.WriteLog("Error: " + ex.Message, EventLogEntryType.Error);
            }
        }


        /// <summary>
        /// Check Performance of the Applocation.
        /// </summary>
        void CheckPerformance()
        {
            PerformanceCounter cpuCounter;
            PerformanceCounter ramCounter;

            cpuCounter = new PerformanceCounter();

            cpuCounter.CategoryName = "Processor";
            cpuCounter.CounterName = "% Processor Time";
            cpuCounter.InstanceName = "_Total";

            ramCounter = new PerformanceCounter("Memory", "Available MBytes");
            string cpuUses = cpuCounter.NextValue() + "%";
            string ram = ramCounter.NextValue() + "MB";

            Console.WriteLine("Cpu Uses " + cpuUses + "\n" + "Memory Uses  " + ram);
        }

        //Invoke when a Create event would be performed
        void fileSysetmWatcher_created(object sender, FileSystemEventArgs e, WatchedFolder folder)
        {
            if (Global.GetFileExtention(e.FullPath))
            {
                Global.WriteLog(string.Format("New File {0} Created On {1} ",
                    DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss"), e.FullPath), EventLogEntryType.Information);
                SPHelper.AddFileToSPLib(e.FullPath, folder.SpSite, folder.SpLib);

            }
            else
            {
                try
                {
                    Global.WriteLog(string.Format("New File {0} Created On {1} ",
                        DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss"), e.FullPath), EventLogEntryType.Information);
                }
                catch (Exception ex)
                {
                    Global.WriteLog(string.Format("Error On File {0} Created On {1} :: {2} ", DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss"), e.FullPath, ex.Message),
                        EventLogEntryType.Information);
                }
            }
            return;
        }

        /// <summary>
        /// fileSysetmWatcher_deleted: Fired when the Watcher object detects a folder/file delete event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void fileSysetmWatcher_deleted(object sender, FileSystemEventArgs e, WatchedFolder folder)
        {
            SPHelper.DeleteFileToSPLib(e.Name, folder.SpSite, folder.SpLib);

            Global.WriteLog("deleted:" + e.FullPath, EventLogEntryType.Information);

        }

        void fileSysetmWatcher_Renamed(object sender, RenamedEventArgs e, WatchedFolder folder)
        {

            if (Global.GetFileExtention(e.FullPath))
            {
                Global.WriteLog(e.ChangeType + ": " + e.FullPath, EventLogEntryType.Information);

                SPHelper.RenameFileToSPLib(e.OldFullPath, e.FullPath, folder.SpSite, folder.SpLib);
            }
        }

        /// <summary>
        /// fileSysetmWatcher_Changed: Fired when the Watcher object detects a folder/file change event.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        //Invoke when a  changed event would be performed

        void fileSysetmWatcher_Changed(object sender, FileSystemEventArgs e, WatchedFolder folder)
        {
            if (Global.GetFileExtention(e.FullPath))
            {
                Global.WriteLog(e.ChangeType + ": " + e.FullPath, EventLogEntryType.Information);

                SPHelper.AddFileToSPLib(e.FullPath, folder.SpSite, folder.SpLib);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        protected override void OnStop()
        {
            Global.WriteLog(string.Format("Service Stop At {0}", DateTime.Now.ToString("dd MMMM yyyy hh:mm:ss")), EventLogEntryType.Information);
        }


    }
}
