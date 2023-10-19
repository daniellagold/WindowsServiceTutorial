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

namespace WindowsServiceTurorial
{
    public partial class Service1 : ServiceBase
    {
        Timer timer = new Timer();
        public Service1()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            WriteToFile("Hi! Service started at: " + DateTime.Now);
            Deletefile();
        }
        protected void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Deletefile();
        }
        public void Deletefile()
        {
            try
            {
                timer.Interval = (10000) * 6;
                timer.Elapsed += new System.Timers.ElapsedEventHandler(timer_Elapsed);
                DateTime expirationDate = DateTime.Now.AddDays(-30);

                DirectoryInfo directory = new DirectoryInfo(@"C:\Path_Of_Files");

                FileInfo[] Files = (FileInfo[])directory.GetFiles()
                            .Where(file => file.CreationTime <= expirationDate);
                //string[] Files = Directory.GetFiles(@"C:\Users\i832609\OneDrive - NationalGeneral\Pictures");
                WriteToFile(Files.Count() + " files pulled;");
                bool deletedFiles = false;
                //string[] Files = Directory.GetFiles(@"\\ngic.com\fs\apps\nps\sqa\DropBox\Graveyard");
                for (int i = 0; i < Files.Length; i++)
                {
                    //Here we will find wheter the file is 7 days old
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
                timer.Enabled = true;
                timer.Start();
            }
            catch
            {
            }
        }

        protected override void OnStop()
        {
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
    }
}
