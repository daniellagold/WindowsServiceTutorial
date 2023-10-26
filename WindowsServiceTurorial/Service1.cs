using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.IO;
using System.Timers;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using static System.IO.Directory;

namespace WindowsServiceTurorial
{
    public partial class Service1 : ServiceBase
    {
        string filePath = "C:\\Users\\I832609\\PicturesTest";
        System.Timers.Timer timer = new System.Timers.Timer();
        private CancellationTokenSource cts = new CancellationTokenSource();
        private Task mainTask = null;
        private TimeSpan FileCheckInterval = TimeSpan.FromMinutes(60);
        private TimeSpan IntervalAfterFail = TimeSpan.FromMinutes(5);
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            mainTask = new Task(DeleteFiles, cts.Token, TaskCreationOptions.LongRunning);
            mainTask.Start();
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_RUNNING;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);

        }
        public void DeleteFiles()
        {
            CancellationToken cancellation = cts.Token;
            TimeSpan interval = TimeSpan.Zero;
            DateTime expirationDate = DateTime.Now.AddDays(-30);
            DirectoryInfo directory = new DirectoryInfo(filePath);
            try
            {
                while (!cancellation.WaitHandle.WaitOne(interval))
                {
                    FileInfo[] Files = (FileInfo[])directory.GetFiles()
                                .Where(file => file.CreationTime <= expirationDate);
                    //string[] Files = Directory.GetFiles(@"C:\Users\i832609\OneDrive - NationalGeneral\Pictures");
                    WriteToFile(Files.Count() + " files pulled;");
                    bool deletedFiles = false;
                    //string[] Files = Directory.GetFiles(@"\\ngic.com\fs\apps\nps\sqa\DropBox\Graveyard");
                    for (int i = 0; i < Files.Length; i++)
                    {
                        if (DateTime.Now.Subtract(File.GetCreationTime(Files[i].ToString())).TotalDays > expirationDate.Day)
                        {
                            WriteToFile("Deleting " + Files[i].ToString() + " created at: " + File.GetCreationTime(Files[i].ToString()));
                            File.Delete(Files[i].ToString());
                            WriteToFile("File Deleted Successfully");
                            deletedFiles = true;
                        }
                    }
                    if (!deletedFiles)
                    {
                        WriteToFile("No files older then 30 days found");
                    }
                    if (cancellation.IsCancellationRequested)
                    {
                        break;
                    }
                    interval = FileCheckInterval;
                }
            }
            catch
            {
                interval = IntervalAfterFail;
            }
        }

        protected override void OnStop()
        {
            cts.Cancel();
            mainTask.Wait();

            // Update the service state to Stopped.
            ServiceStatus serviceStatus = new ServiceStatus();
            serviceStatus.dwCurrentState = ServiceState.SERVICE_STOPPED;
            SetServiceStatus(this.ServiceHandle, ref serviceStatus);
            WriteToFile("Service Complete.");
        }

        public void WriteToFile(string message)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "\\Logs";
            if (!Directory.Exists(path)) { Directory.CreateDirectory(path);  };
            string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\ServiceLog_";
            if (!File.Exists(filePath)) 
            { 
                using (StreamWriter sw = File.CreateText(filePath)) { sw.WriteLine(message); }
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filePath)) { sw.WriteLine(message); }
            }

        }
        #region Service State
        private enum ServiceState
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
        private struct ServiceStatus
        {
            public int dwServiceType;
            public ServiceState dwCurrentState;
            public int dwControlsAccepted;
            public int dwWin32ExitCode;
            public int dwServiceSpecificExitCode;
            public int dwCheckPoint;
            public int dwWaitHint;
        };
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool SetServiceStatus(System.IntPtr handle, ref ServiceStatus serviceStatus);
        #endregion
    }
}
