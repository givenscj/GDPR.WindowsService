using System.ComponentModel;

namespace GDPRWindowsService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        GDPRRequestService Service;

        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void RetrieveServiceName()
        {
            var serviceName = Context.Parameters["servicename"];

            if (!string.IsNullOrEmpty(serviceName))
            {
                this.serviceInstaller1.ServiceName = serviceName;
                this.serviceInstaller1.DisplayName = serviceName;
            }
        }

        public override void Install(System.Collections.IDictionary stateSaver)
        {
            RetrieveServiceName();
            base.Install(stateSaver);
        }


        public override void Uninstall(System.Collections.IDictionary savedState)

        {
            RetrieveServiceName();
            base.Uninstall(savedState);
        }


    }
}
