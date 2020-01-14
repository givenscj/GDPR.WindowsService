using GDPR.Common;
using GDPR.Common.Core;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.ServiceProcess;
using Configuration = GDPR.Common.Configuration;

namespace GDPRWindowsService
{
    public partial class GDPRRequestService : ServiceBase
    {
        static EventProcessorHost eventProcessorHost;

        public GDPRRequestService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {

            try
            {
                //no logging aai
                Configuration.LoadWithMode(ConfigurationManager.AppSettings["Mode"].ToString());
                GDPRCore.Current = new GDPRCore();

                GDPRCore.Current.Log("Loading application stubs");

                //load any extra application stubs...
                Assembly a = Assembly.GetExecutingAssembly();
                FileInfo fi = new FileInfo(a.Location);
                Utility.LoadAssemblies(fi.Directory.FullName + "\\ApplicationStubs");

                if (string.IsNullOrEmpty(Configuration.StorageAccountName) || string.IsNullOrEmpty(Configuration.StorageAccountKey))
                {
                    GDPRCore.Current.Log("You are missing the storage account name/key");
                }

                //this allows you to keep track of your offset/checkpoints
                string storageConnectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}", Configuration.StorageAccountName, Configuration.StorageAccountKey);

                System.Diagnostics.Debugger.Launch();

                string eventHubConnectionString = GDPR.Common.Core.GDPRCore.Current.GetApplicationEventHub(Configuration.ApplicationId);

                //create an event processor for each event hub/application
                foreach (string appId in Configuration.ApplicationMap.Keys)
                {
                    try
                    {
                        eventProcessorHost = new EventProcessorHost(appId.ToLower(), appId.ToLower(), EventHubConsumerGroup.DefaultGroupName, eventHubConnectionString, storageConnectionString);
                        GDPRCore.Current.Log($"Registering EventProcessor for {appId}...");
                        var options = new EventProcessorOptions();
                        options.ExceptionReceived += (sender, e) => { GDPRCore.Current.Log(e.Exception, GDPR.Common.Enums.LogLevel.Error); };
                        eventProcessorHost.RegisterEventProcessorAsync<GDPREventProcessor>(options).Wait();
                    }
                    catch (AggregateException ex)
                    {
                        foreach (Exception inner in ex.InnerExceptions)
                        {
                            GDPRCore.Current.Log(ex, GDPR.Common.Enums.LogLevel.Error);
                        }
                    }
                    catch (Exception ex)
                    {
                        GDPRCore.Current.Log(ex, GDPR.Common.Enums.LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                File.AppendAllText("c:\\temp\\error.log", ex.Message);
            }
        }

        protected override void OnStop()
        {         
            GDPRCore.Current.Log("Stopping service");
            eventProcessorHost.UnregisterEventProcessorAsync().Wait();
        }
    }
}
