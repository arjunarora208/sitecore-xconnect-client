using Newtonsoft.Json;
using Sitecore.Practise.App.Configurations;
using Sitecore.Practise.App.Services;
using Sitecore.XConnect;
using Sitecore.XConnect.Client;
using Sitecore.XConnect.Client.Serialization;
using Sitecore.XConnect.Collection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Sitecore.Practise.App.Controllers
{
    public class ManageContactsController : Controller
    {
        JsonSerializerSettings _serializerSettings;
        readonly DateTime startDate = new DateTime(2025, 01, 01, 00, 00, 00).ToUniversalTime();// As directed by Sitecore Documentation
        readonly DateTime endDate = new DateTime(2025, 03, 31, 00, 00, 00).ToUniversalTime();// As directed by Sitecore Documentation
        const int batchSize = 200;// Take it from web.config
        string executionIdentifier = Guid.NewGuid().ToString();

        public async Task<ActionResult> FetchCount()
        {
            try
            {
                var _client = await XConnectClientReader.GetClient(new Configurations.MigrationSettings());
                using (Sitecore.XConnect.Client.XConnectClient client = new XConnectClient(_client))
                {
                    var interactionFacets = client.Model.Facets.Where(c => c.Target == EntityType.Interaction).Select(x => x.Name).ToList();
                    var facetArray = interactionFacets.ToArray();

                    var interactionCursor = await client.CreateInteractionEnumerator(
                            new InteractionEnumeratorOptions(batchSize, DateTime.UtcNow,
                            new InteractionExpandOptions(facetArray)
                            {
                                Contact = new RelatedContactExpandOptions(PersonalInformation.DefaultFacetKey)
                            })
                            {
                                MinStartDateTime = startDate,
                                MaxStartDateTime = endDate
                            }
                            );
                    ViewBag.StartDate = startDate;
                    ViewBag.EndDate = endDate;
                    ViewBag.TotalCount = interactionCursor.TotalCount;
                    LogHelper.Info($"Interactions found from {startDate} to {endDate} : {interactionCursor.TotalCount}", null, executionIdentifier);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error(ex.Message, ex, executionIdentifier);
                ViewBag.ErrorMessage = ex.Message;
            }
            return View();
        }

        public async Task<ActionResult> Process(byte[] bookmarkParam = null)
        {
            byte[] bookmark = null;
            string batchId = null;
            try
            {
                var _client = await XConnectClientReader.GetClient(new Configurations.MigrationSettings());
                IAsyncInteractionBatchEnumerator interactionCursor = null;
                using (Sitecore.XConnect.Client.XConnectClient client = new XConnectClient(_client))
                {
                    _serializerSettings = new JsonSerializerSettings
                    {
                        ContractResolver = new XdbJsonContractResolver(client.Model,
                                   serializeFacets: true,
                                   serializeContactInteractions: false),
                        DateTimeZoneHandling = DateTimeZoneHandling.Utc,
                        DefaultValueHandling = DefaultValueHandling.Ignore
                    };

                    var interactionFacets = client.Model.Facets.Where(c => c.Target == EntityType.Interaction).Select(x => x.Name).ToList();
                    var interactionFacetArray = interactionFacets.ToArray();

                    if (bookmarkParam == null)
                    {
                        interactionCursor = await client.CreateInteractionEnumerator(
                       new InteractionEnumeratorOptions(batchSize, DateTime.UtcNow,
                       new InteractionExpandOptions(interactionFacetArray)
                       {
                           Contact = new RelatedContactExpandOptions(PersonalInformation.DefaultFacetKey)
                       })
                       {
                           MinStartDateTime = startDate,
                           MaxStartDateTime = endDate
                       }
                       );
                    }
                    else
                    {
                        interactionCursor = await client.CreateInteractionEnumerator(
                       new BookmarkInteractionEnumeratorOptions(batchSize, bookmarkParam,
                       new InteractionExpandOptions(interactionFacetArray)
                       {
                           Contact = new RelatedContactExpandOptions(PersonalInformation.DefaultFacetKey)
                       })
                       );
                    }
                    int BatchCount = 1;
                    while (await interactionCursor.MoveNext())
                    {
                        batchId = Guid.NewGuid().ToString(); //Unique ID for each batch
                        bookmark = interactionCursor.GetBookmark();
                        LogHelper.SaveBookmark(bookmark);
                        string batchFileName = $"{batchId}-Batch{BatchCount}";

                        if (interactionCursor.Current.Count > 0)
                        {
                            var currentBatch = interactionCursor.Current.ToList();
                            LogHelper.Info($"Batch Count: {BatchCount} || Total Records In Current Batch: {currentBatch.Count} || Batch Id: {batchId}", string.Empty, batchFileName);
                            int RowCount = 1;
                            foreach (var interaction in currentBatch)
                            {
                                var contact = FetchContact(System.Convert.ToString(interaction.Contact.Id), client, batchFileName, RowCount);
                                if (contact != null)
                                {
                                    var result = ProcessData(interaction, contact, batchFileName, RowCount);
                                    LogHelper.Info($"RowCount: {RowCount} || Status: {result.Result}  || ContactId: {contact.Id} || InteractionId: {interaction.Id}", null, batchFileName);
                                }
                                RowCount++;
                            }
                        }
                        else
                            LogHelper.Info($"Batch Count: {BatchCount} || Total Records In Current Batch: 0 || Batch Id: {batchId}", string.Empty, batchFileName);
                        BatchCount++;
                    }
                    ViewBag.Message = "Records Processed : " + interactionCursor.TotalCount;
                }
            }
            catch (XdbExecutionException ex)
            {
                LogHelper.Error(ex.Message, ex, "CriticalError");
                if (bookmark != null)
                    LogHelper.SaveBookmark(bookmark);
            }
            catch (Exception ex)
            {
                LogHelper.SaveBookmark(bookmark);
                LogHelper.Error(ex.Message, ex, "CriticalError");
                ViewBag.Error = $"Error: {ex.Message} Inner Exception: {ex.InnerException}";
            }
            return View();
        }

        public async Task<ActionResult> ResumeProcess()
        {
            var bookmark = LogHelper.GetBookmark();
            if (bookmark != null)
                LogHelper.Info("Bookmark Found: Processing resumes", "Info", "ResumeProcess");
            else
                throw new Exception("No bookmark found");
            return await Process(bookmark);
        }

        private Contact FetchContact(string contactId, XConnectClient client, string batchFileName, int rowCount)
        {
            if (!string.IsNullOrEmpty(contactId))
                try
                {
                    Contact contact = null;
                    var contactFacets = client.Model.Facets.Where(c => c.Target == EntityType.Contact).Select(x => x.Name).ToList();
                    var facetArray = contactFacets.ToArray();

                    var reference = new Sitecore.XConnect.ContactReference(Guid.Parse(contactId));
                    contact = client.Get<Contact>(reference, new ContactExecutionOptions(new ContactExpandOptions(facetArray) { }));

                    return contact;
                }
                catch (Exception ex)
                {
                    LogHelper.Error($"RowCount: {rowCount} || Error fetching contact: ContactId: {contactId}", ex, batchFileName);
                    return null;
                }
            else
                return null;

        }

        private async Task<bool> ProcessData(Interaction objInteraction, Contact objContact, string batchFileName, int rowCount)
        {
            return true;
        }
    }
}