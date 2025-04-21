using Sitecore.Practise.App.Services;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.WebApi;
using Sitecore.XConnect.Collection.Model;
using Sitecore.XConnect.Schema;
using Sitecore.Xdb.Common.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace Sitecore.Practise.App.Configurations
{
    public class XConnectClientReader
    {
        public static async Task<XConnectClientConfiguration> GetClient(MigrationSettings settings)
        {
            CertificateHttpClientHandlerModifierOptions options =
                CertificateHttpClientHandlerModifierOptions.Parse(
                    settings.SitecoreReportingClientCertificate);
            options.AllowInvalidClientCertificates = "true";

            // Optional timeout modifier
            var certificateModifier = new CertificateHttpClientHandlerModifier(options);

            List<IHttpClientModifier> clientModifiers = new List<IHttpClientModifier>();
            var timeoutClientModifier = new TimeoutHttpClientModifier(new TimeSpan(0, 0, 20));
            clientModifiers.Add(timeoutClientModifier);

            // This overload takes three client end points - collection, search, and configuration
            var collectionClient = new CollectionWebApiClient(new Uri(settings.XConnectClient), clientModifiers, new[] { certificateModifier });
            var searchClient = new SearchWebApiClient(new Uri(settings.XConnectClient), clientModifiers, new[] { certificateModifier });
            var configurationClient = new ConfigurationWebApiClient(new Uri(settings.XConnectClientConfig), clientModifiers, new[] { certificateModifier });

            XdbModel[] objModels = new XdbModel[1] { CollectionModel.Model};
            var cfg = new XConnectClientConfiguration(
                new XdbRuntimeModel(objModels), collectionClient, searchClient, configurationClient, true);

            try
            {
                await cfg.InitializeAsync();
            }
            catch (XdbModelConflictException ce)
            {
                LogHelper.Error(ce.Message, ce, "XConnectClient_CriticalError");
                throw;
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex, "XConnectClient_CriticalError");
                throw;
            }
            return cfg;
        }
    }
}