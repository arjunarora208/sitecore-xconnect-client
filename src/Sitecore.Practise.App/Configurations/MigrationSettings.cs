using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Sitecore.Practise.App.Configurations
{
    public class MigrationSettings
    {
        public MigrationSettings()
        {
            XConnectClient = System.Convert.ToString(ConfigurationManager.ConnectionStrings["XConnectClient"].ConnectionString);
            XConnectClientConfig = System.Convert.ToString(ConfigurationManager.ConnectionStrings["XConnectclientConfig"].ConnectionString);
            SitecoreReportingClientCertificate = System.Convert.ToString(ConfigurationManager.ConnectionStrings["sitecore.reporting.client.certificate"].ConnectionString);
        }

        public string XConnectClient { get; }
        public string XConnectClientConfig { get; }
        public string SitecoreReportingClientCertificate { get; }
    }
}