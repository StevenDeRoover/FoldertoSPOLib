using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CN_SharePoint_Library_Syn
{
    static class Program
    {

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {

            if (System.Diagnostics.Process.GetProcesses().Count(p => p.ProcessName == "CN SharePoint Library Syn") > 1)
            {
                foreach (System.Diagnostics.Process myProc in System.Diagnostics.Process.GetProcesses())
                {
                    if (myProc.ProcessName == "CN SharePoint Library Syn" &&
                       myProc.Id != Process.GetCurrentProcess().Id)
                    {
                        myProc.Kill();
                    }
                }

            }


#if(!DEBUG)
           ServiceBase[] ServicesToRun;
           ServicesToRun = new ServiceBase[] 
	   { 
	        new CNSharePointLibrarySyn() 
	   };
           ServiceBase.Run(ServicesToRun);
#else


            if (args.Length != 0)
                if (args[0] == "show")
                    ShowWindow(GetConsoleWindow(), SW_SHOW);

                else
                    ShowWindow(GetConsoleWindow(), SW_HIDE);
            else
                ShowWindow(GetConsoleWindow(), SW_HIDE);

            CNSharePointLibrarySyn myServ = new CNSharePointLibrarySyn();
            myServ.Process();


            System.Threading.Thread.Sleep(-1);


            // here Process is my Service function
            // that will run when my service onstart is call
            // you need to call your own method or function name here instead of Process();
#endif
        }
    }
}
