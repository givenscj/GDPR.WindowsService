using System;
using System.IO;
using System.ServiceProcess;

namespace GDPRWindowsService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new GDPRRequestService()
            };

            try
            {
                ServiceBase.Run(ServicesToRun);
            }
            catch (Exception ex)
            {
                File.AppendAllText("c:\\temp\\error.log", ex.Message);
            }
        }
    }
}
