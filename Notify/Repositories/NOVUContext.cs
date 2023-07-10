
using Encryption;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Notify.Classes;
using Notify.Interfaces;
using Notify.Models;
using Notify.Repositories.NOVUSettingsClasses;
using Org.BouncyCastle.Asn1.Ocsp;
using RestSharp;
using System.Numerics;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Notify.Repositories
{
    public class NOVUContext : INOVUContext
    {
        #region Fields
        private IConfiguration _configuration;
        private RestClient _client;
        private NOVUSettings _nOVUsettings;
        private Serilog.ILogger _logger;
        private List<Sponsor> _sponsors = null;
        private bool _loggingEnabled;
        #endregion

        #region Constructors
        /// <summary>
        /// Contructor
        /// </summary>
        /// <param name="configuration">the object that can rich the cofig global dictionary</param>
        /// <param name="sponsors">contains the cashed information of the sponsors</param>
        public NOVUContext(IConfiguration configuration, Serilog.ILogger logger, List<Sponsor> sponsors, ILoggingEnable loggingEnable)
        {
            _configuration = configuration;
            _sponsors = sponsors;
            _logger = logger;
            _loggingEnabled = loggingEnable.Enabled;
        }
        #endregion
        #region Fundimentals
        private Sponsor GetSponserByID(string SponserID)
        {
            return _sponsors?.Where(s => s.SponserID == SponserID).FirstOrDefault();
        }
        private RestRequest GetRestRequest(string RequestURL, string SponsorID)
        {
            _nOVUsettings = new NOVUSettings()
            {
                URL     = _configuration.GetValue<string>("NOVUSettings:URL"),
                Version = _configuration.GetValue<string>("NOVUSettings:Version"),
                APIKey  = _configuration.GetValue<string>("NOVUSettings:KeyPrefix")
                          +
                          " "
                          +
                          GetSponserByID(SponsorID)?.APIKeyEncryption
            };
            _client = new RestClient(_nOVUsettings.URL);
            return new RestRequest($"{_nOVUsettings.Version}/{RequestURL}").AddHeader("Authorization", _nOVUsettings.APIKey);
        }
        #endregion


        #region Flow Region
        /// <summary>
        ///   Update a Flow in the spesified NOVU  
        /// </summary>
        /// <param name="Flow">
        /// is an object contains the requiered data to be Updated (inserted)
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// return the Notification-Template object that will be returned from the API
        /// this object will be filled with all the ignored values (like id's etc..)
        /// also return a message for success or failure
        /// </returns>
        public async Task<Tuple<Classes.Notification, string>> UpdateFlow(Flow Flow, string SponsorID)
        {
            var request = GetRestRequest($"notification-templates/{Flow.Id}", SponsorID);
            var notification = new Classes.Notification(Flow);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(notification);
            RestResponse response = await _client.ExecutePutAsync(request);

            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"UpdateFlow FlowID:{Flow.Id},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }
            });



            if (!response.IsSuccessful)
            {
                string message = response.Content + " " + response.ErrorMessage;
                return new Tuple<Classes.Notification, string>(null, message);
            }
            Classes.SerialisationHelp<Classes.Notification> serialisationHelp = null;
            serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<Classes.Notification>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new Tuple<Classes.Notification, string>(serialisationHelp.data, "Success");
        }

        /// <summary>
        /// create a Flow in the spesified NOVU  
        /// </summary>
        /// <param name="Flow">
        /// is an object contains the requiered data to be added (inserted)
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// return the Notification-Template object that will be returned from the API
        /// this object will be filled with all the ignored values (like id's etc..)
        /// also return a message for success or failure
        /// </returns>
        public async Task<Tuple<Classes.Notification, string>> CreateFlow(Flow Flow, string SponsorID)
        {
            var request = GetRestRequest("notification-templates", SponsorID);
            var notification = new Classes.Notification(Flow);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(notification);
            RestResponse response = await _client.ExecutePostAsync(request);



            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"CreateFlow FlowID:{Flow.Id},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }
            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Classes.Notification, string>(null, message);
            }
            Classes.SerialisationHelp<Classes.Notification> serialisationHelp = null;
            serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<Classes.Notification>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new Tuple<Classes.Notification, string>(serialisationHelp.data, "Success");
        }

        /// <summary>
        /// this is the method that can get Flows(Notify) from Notifications in NOVU  
        /// you need to provide page number for pagination
        /// the page size is 10
        /// </summary>
        /// <param name="Page">
        /// is the page number
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// List of Flows(Notify)
        /// </returns>
        public async Task<Tuple<List<Flow>, string>> GetFlowsByPage(int Page, string SponsorID)
        {
            var request = GetRestRequest("notification-templates", SponsorID);
            request.AddParameter("page", Page);
            RestResponse response = await _client.ExecuteAsync(request);

            

            SerialisationHelp<List<Classes.Notification>> Notifications = null;
            if (response.IsSuccessful)
                Notifications = JsonSerializer.Deserialize<SerialisationHelp<List<Classes.Notification>>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });


            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"GetFlowsByPage Page:{Page},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    if (response.IsSuccessful)
                    {
                        _logger.Information(BackgroundQueueLogger.GetObjectJsonString(Notifications.data));
                        _logger.Information($"{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }
                    else 
                    {
                        _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }
                }
            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<List<Models.Flow>, string>(null, message);
            }

            return new Tuple<List<Models.Flow>, string>(Notifications.data.Select(S => new Flow(S)).ToList(), "Success");
        }

        /// <summary>
        /// this is the method that can get the Flow(Notify) from Notifications in NOVU
        /// </summary>
        /// <param name="ID">
        /// ID is the Notification Template in NOVU database
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the Notification Template from NOVU Database in Flow(Notify) Format
        /// </returns>
        public async Task<Tuple<Flow, string>> GetFlowByID(string ID, string SponsorID)
        {
            var request = GetRestRequest($"notification-templates/{ID}", SponsorID);
            RestResponse response = await _client.ExecuteAsync(request);
            SerialisationHelp<Classes.Notification> Notifications = null;
            if (response.IsSuccessful)
            {
                Notifications = Newtonsoft.Json.JsonConvert.DeserializeObject<SerialisationHelp<Classes.Notification>>(response.Content);

            }

            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"GetFlowsByPage FlowID:{ID},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    if (response.IsSuccessful)
                    {
                        _logger.Information($"{BackgroundQueueLogger.GetObjectJsonString(Notifications.data)}");
                        _logger.Information($"{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }
                    else 
                    {
                        _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }
                }
            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Models.Flow, string>(null, message);
            }
            
            return new Tuple<Flow, string>(new Flow(Notifications.data), "Success");
        }
        #endregion

        #region Subscriber Region
        /// <summary>
        /// Get All the the subscribers information divided by page number
        /// </summary>
        /// <param name="Page">
        /// page number
        /// </param> 
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// list of subscribers specified by the page number
        /// </returns>
        public async Task<Tuple<List<Models.Subscriber>, string>> GetSubscribersByPage(int Page, string SponsorID)
        {
            var request = GetRestRequest("subscribers", SponsorID);
            request.AddParameter("page", Page);
            RestResponse response = await _client.ExecuteAsync(request);
            SerialisationHelp<List<Classes.Subscriber>> Subscribers = null;
            if (response.IsSuccessful)
            {
                Subscribers = JsonSerializer.Deserialize<SerialisationHelp<List<Classes.Subscriber>>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"GetSubscribersByPage Page:{Page},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    if (response.IsSuccessful)
                    {
                        _logger.Information($"{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                        string txt = BackgroundQueueLogger.GetObjectJsonString(Subscribers.data);
                        _logger.Information($"{txt}");
                    }
                    else
                    {
                        _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }

                }
            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<List<Models.Subscriber>, string>(null, message);
            }
            
            return new Tuple<List<Models.Subscriber>, string>(Subscribers.data.Select(sub => new Models.Subscriber(sub)).ToList(), "Success");
        }

        /// <summary>
        /// get subscriber (Notify) information by SubscriberID from NOVU database
        /// </summary>
        /// <param name="id">
        /// is the subscriber id
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the requiered subscriber from NOVU database in Notify Format
        /// </returns>
        public async Task<Tuple<Models.Subscriber, string>> GetSubscriberByID(string ID, string SponsorID)
        {
            var request = GetRestRequest($"subscribers/{ID}", SponsorID);
            RestResponse response = await _client.ExecuteAsync(request);
            SerialisationHelp<Classes.Subscriber> Subscriber = null;
            if (response.IsSuccessful)
            {
                Subscriber = JsonSerializer.Deserialize<SerialisationHelp<Classes.Subscriber>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }

            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"GetSubscriberByID SubscriberID:{ID},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    if (response.IsSuccessful)
                    {
                        _logger.Information($"{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                        _logger.Information($"{BackgroundQueueLogger.GetObjectJsonString(Subscriber.data)}");
                    }
                    else
                    {
                        _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                    }

                }
            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Models.Subscriber, string>(null, message);
            }
            return new Tuple<Models.Subscriber, string>(new Models.Subscriber(Subscriber.data), "Success");

        }

        /// <summary>
        /// Create subscriber (Notify) information
        /// </summary>
        /// <param name="Subscriber">
        /// is the subscriber (Notify) information
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the inserted subscriber in NOVU Format
        /// </returns>
        public async Task<Tuple<Classes.Subscriber, string>> CreateSubscriber(Models.Subscriber Subscriber, string SponsorID)
        {
            var request = GetRestRequest("subscribers", SponsorID);
            var subscriber = new Classes.Subscriber(Subscriber);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(subscriber);
            RestResponse response = await _client.ExecutePostAsync(request);


            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"CreateSubscriber SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }

            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Classes.Subscriber, string>(null, message);
            }
            Classes.SerialisationHelp<Classes.Subscriber> serialisationHelp = null;
            serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<Classes.Subscriber>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new Tuple<Classes.Subscriber, string>(serialisationHelp.data, "Success");
        }

        /// <summary>
        /// Update subscriber (Notify) information
        /// </summary>
        /// <param name="Subscriber">
        /// is the subscriber (Notify) information
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the Updated subscriber in NOVU Format
        /// </returns>
        public async Task<Tuple<Classes.Subscriber, string>> UpdateSubscriber(Models.Subscriber Subscriber, string SponsorID)
        {
            var request = GetRestRequest($"subscribers/{Subscriber.Id}", SponsorID);
            var subscriber = new Classes.Subscriber(Subscriber);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(subscriber);
            RestResponse response = await _client.ExecutePutAsync(request);


            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"UpdateSubscriber SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }

            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Classes.Subscriber, string>(null, message);
            }
            Classes.SerialisationHelp<Classes.Subscriber> serialisationHelp = null;
            serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<Classes.Subscriber>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new Tuple<Classes.Subscriber, string>(serialisationHelp.data, "Success");
        }
        /// <summary>
        /// Set device tokens for subscriber 
        /// </summary>
        /// <param name="SubscriberID">Subscriber id</param>
        /// <param name="DeviceTokens">List of Subscriber Device ID's</param>
        /// <param name="providerId">the provider id (in default its fcm)</param>
        /// <param name="SponsorID">a string represent the sponsor id</param>
        /// <returns>the subscriber after update</returns>
        public async Task<Tuple<Classes.Subscriber, string>> SetSubscriberDeviceToken(string SubscriberID, Classes.Channel channel, string SponsorID)
        {
            var request = GetRestRequest($"subscribers/{SubscriberID}/credentials", SponsorID);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(channel);
            RestResponse response = await _client.ExecutePutAsync(request);


            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"SetSubscriberDeviceToken SubscriberID:{SubscriberID},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }

            });

            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<Classes.Subscriber, string>(null, message);
            }
            Classes.SerialisationHelp<Classes.Subscriber> serialisationHelp = null;
            serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<Classes.Subscriber>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            return new Tuple<Classes.Subscriber, string>(serialisationHelp.data, "Success");
        }

        /// <summary>
        /// Delete the Subscriber with the specified subscriberID from NOVU Database
        /// </summary>
        /// <param name="SubscriberID">the subscriber id of the subscriber that need to be deleted</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>true if deleted and false if not</returns>
        public async Task<Tuple<bool, string>> DeleteSubscriber(string SubscriberID, string SponsorID)
        {
            var request = GetRestRequest($"subscribers/{SubscriberID}", SponsorID);
            RestResponse response = await _client.DeleteAsync(request);


            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"DeleteSubscriber SubscriberID:{SubscriberID},SponsorID:{SponsorID}");
                    string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b);
                    _logger.Information($"{parameterStr}");
                    _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}"));
                }

            });
            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return new Tuple<bool, string>(false, message);
            }
            return new Tuple<bool, string>(true, "Success");

        }

        #endregion

        #region Template Region
        /// <summary>
        /// Get the Template from NOVU Database
        /// </summary>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the requieded template in Notify Format
        /// </returns>
        public async Task<Tuple<Models.Template, string>> GetTemplateByID(string flowID, string templateID, string SponsorID)
        {
            Tuple<Flow, string> flowTuple = await GetFlowByID(flowID, SponsorID);



            BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
            {
                //this is for logging
                if (_loggingEnabled)
                {
                    _logger.Information($"GetTemplateByID flowID:{flowID},templateID:{templateID},SponsorID:{SponsorID}");
                    if (flowTuple.Item1 != null)
                        _logger.Information($"flow is exist");
                    else
                        _logger.Information($"flow is not exist");
                }
            });
            if (flowTuple.Item1 != null)
            {
                Flow flow = flowTuple.Item1;
                if (flow.Templates.Count == 0)
                {
                    string message1 = $"the Flow does not contain any templates";

                    BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
                    {
                        if (_loggingEnabled)
                            _logger.Information(message1);

                    });

                    return new Tuple<Models.Template, string>(null, message1);
                }
                Models.Template template = flow.Templates.Where(temp => temp.Id == templateID).FirstOrDefault();
                if (template != null)
                {
                    BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Information($"Success"); });
                    return new Tuple<Models.Template, string>(template, "Success");
                }

                string message = $"the Flow does not contain the requiered template";
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Information(message); });

                return new Tuple<Models.Template, string>(null, message);
            }
            return new Tuple<Models.Template, string>(null, $"the Flow cannot be found\n{flowTuple.Item2}");
        }
        /* please use the following for testing
         * 64143b4eab0b7622528464f6 flow
         * 64143b4dab0b7622528464e6 template
         */


        /// <summary>
        /// insert the Template into the flow that have the passed flowid in NOVU Database
        /// </summary>
        /// <param name="flowId">the id of the flow that you need to insert the template within</param>
        /// <param name="template">the template you wish to insert</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the new Notification(NOVU) after adding the template
        /// </returns>
        public async Task<Tuple<Classes.Notification, string>> CreateTemplate(string flowId, Models.Template template, string SponsorID)
        {
            Tuple<Flow, string> FlowTuple = await GetFlowByID(flowId, SponsorID);

            //this is for logging
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => 
            {
                if (_loggingEnabled) 
                {
                    _logger.Information($"CreateTemplate flowId:{flowId},SponsorID:{SponsorID}"); 
                    if (FlowTuple.Item1 != null) 
                        _logger.Information($"flow is exist"); 
                    else 
                        _logger.Information($"flow is not exist"); 
                }
            });

            if (FlowTuple.Item1 == null)
                return new Tuple<Classes.Notification, string>(null, $"the Flow cannot be found\n{FlowTuple.Item2}");

            Flow flow = FlowTuple.Item1;

            if (flow.Templates == null)
                flow.Templates = new List<Models.Template>();

            flow.Templates.Add(template);

            Tuple<Classes.Notification, string> notificationTuple = await UpdateFlow(flow, SponsorID);
            if (notificationTuple.Item1 == null)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"the Template cannot be added because of Flow Updating Problem\n{notificationTuple.Item2}"); } });
                return new Tuple<Classes.Notification, string>(null, $"the Template cannot be added because of Flow Updating Problem\n{notificationTuple.Item2}");
            }
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"{notificationTuple.Item2}"); } });
            return notificationTuple;
        }
        /// <summary>
        /// update the Template within the flow that have the passed flowid and the template id in NOVU Database
        /// </summary>
        /// <param name="flowID">the id of the flow that you need to update the template within</param>
        /// <param name="template">the update you wish to insert</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the new Notification(NOVU) after updating the template
        /// </returns>
        public async Task<Tuple<Classes.Notification, string>> UpdateTemplate(string flowID, Models.Template template, string SponsorID)
        {

            Tuple<Flow, string> FlowTuple = await GetFlowByID(flowID, SponsorID);
            if (FlowTuple.Item1 == null)
            {
                string message = $"the Flow cannot be found\n{FlowTuple.Item2}";
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information(message); } });
                return new Tuple<Classes.Notification, string>(null, message);
            }
            Flow flow = FlowTuple.Item1;
            if (flow.Templates == null || flow.Templates.Count == 0)
            {
                string message = $"the templates in this flow are empty";
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information(message); } });
                return new Tuple<Classes.Notification, string>(null, message);
            }
            int template1index = flow.Templates.IndexOf(flow.Templates.Where(t => t.Id == template.Id).FirstOrDefault());
            if (template1index == -1)
            {
                string message = $"the specified templte is not exist";
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information(message); } });
                return new Tuple<Classes.Notification, string>(null, message);
            }
            flow.Templates[template1index] = template;
            Tuple<Classes.Notification, string> notificationTuple = await UpdateFlow(flow, SponsorID);
            if (notificationTuple.Item1 == null)
            {
                string message = $"the Template cannot be added because of Flow Updating Problem\n{notificationTuple.Item2}";
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information(message); } });
                return new Tuple<Classes.Notification, string>(null, message);
            }
            return notificationTuple;
        }
        #endregion

        #region Notification Region
        /// <summary>
        /// Trigger a flow for a subscriber!
        /// </summary>
        /// <param name="FlowTriggerID">the ID of the flow that want to be triggerred</param>
        /// <param name="SubscriberId">the ID of the Subscriber that want to receive the flow</param>
        /// <param name="Payload">key value pair set of data represent the variables within the flow</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>string represents the Transaction ID</returns>
        public async Task<string> Trigger(string FlowTriggerID, string SubscriberId, string SponsorID, Dictionary<string, string> Payload)
        {
            var request = GetRestRequest("events/trigger", SponsorID);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new
            {
                name = FlowTriggerID,
                to = new { subscriberId = SubscriberId },
                payload = Payload
            }
            );
            RestResponse response = await _client.ExecutePostAsync(request);


            //this is for logging
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"Trigger FlowTriggerID:{FlowTriggerID},SubscriberId:{SubscriberId},SponsorID:{SponsorID}"); string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b); _logger.Information($"{parameterStr}"); _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}")); } });



            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return null;
            }
            SerialisationHelp<TriggerStatus> serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<TriggerStatus>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"transactionId:{serialisationHelp.data.transactionId}"); } });
            return serialisationHelp.data.transactionId;
        }


        /// <summary>
        /// Trigger a flow for a subscriber!
        /// </summary>
        /// <param name="FlowTriggerID">the ID of the flowtrigger that want to be triggerred</param>
        /// <param name="SubscriberId">the ID of the Subscriber that want to receive the flow</param>
        /// <param name="FirstName">the subscriber first name, to be created or updated</param>
        /// <param name="LastName">the subscriber last name, to be created or updated</param>
        /// <param name="PhoneNumber">the subscriber phone number, to be created or updated</param>
        /// <param name="Email">the subscriber email, to be created or updated</param>
        /// <param name="Payload">key value pair set of data represent the variables within the flow</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>string represents the Transaction ID</returns>
        public async Task<string> Trigger(string FlowTriggerID, string SubscriberId, string FirstName, string LastName, string PhoneNumber, string Email, string SponsorID, Dictionary<string, string> Payload)
        {
            var request = GetRestRequest("events/trigger", SponsorID);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new
            {
                name = FlowTriggerID,
                to = new
                {
                    subscriberId = SubscriberId,
                    firstName = FirstName,
                    lastName = LastName,
                    phone = PhoneNumber,
                    email = Email,
                },
                payload = Payload
            }
            );
            RestResponse response = await _client.ExecutePostAsync(request);


            //this is for logging
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"Trigger FlowTriggerID:{FlowTriggerID},SubscriberId:{SubscriberId},FirstName:{FirstName},LastName:{LastName},PhoneNumber:{PhoneNumber},Email:{Email},SponsorID:{SponsorID}"); string parameterStr = request.Parameters.Select(p => p.Name + " : " + p.Value).ToList().Aggregate((a, b) => a + "\n" + b); _logger.Information($"{parameterStr}"); _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}")); _logger.Information($"{response.Content}:{response.StatusCode}" + (response.IsSuccessful ? "" : $":{response.ErrorMessage}")); } });


            if (!response.IsSuccessful)
            {
                string message = response.Content;
                return null;
            }
            SerialisationHelp<TriggerStatus> serialisationHelp = JsonSerializer.Deserialize<SerialisationHelp<TriggerStatus>>(response.Content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) { _logger.Information($"transactionId:{serialisationHelp.data.transactionId}"); } });
            return serialisationHelp.data.transactionId;
        }
        #endregion
    }
}
