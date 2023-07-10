using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Notify.Interfaces;
using Notify.Models;
using Notify.Repositories;
using Notify.Repositories.NOVUSettingsClasses;
using System.Text.RegularExpressions;
using System.Transactions;

namespace Notify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private INOVUContext _nOVUContext;
        private List<Sponsor> _sponsors;
        private Serilog.ILogger _logger;
        private IConfiguration _configuration;
        private bool _loggingEnabled;
        private string variablePattern = @"(\{\{.+?\}\})";

        public NotificationController(INOVUContext nOVUContext,IConfiguration configuration, List<Sponsor> sponsors, Serilog.ILogger logger, ILoggingEnable loggingEnable)
        {
            _nOVUContext = nOVUContext;
            _configuration = configuration;
            _sponsors = sponsors;
            _logger = logger;
            _loggingEnabled = loggingEnable.Enabled;
        }


        /// <summary>
        /// Trigger a flow for a subscriber!
        /// </summary>
        /// <param name="FlowTriggerID">the TriggerID of the flow that want to be triggerred</param>
        /// <param name="SubscriberID">the ID of the Subscriber that want to receive the flow</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <param name="Payload">key value pair set of data represent the variables within the flow</param>
        /// <returns>string represents the Transaction ID</returns>
        [HttpPost("TriggerSubscriberFlow")]
        public async Task<Response<string>> TriggerSubscriberFlow(string FlowTriggerID, string SubscriberId, string SponsorID, Dictionary<string, string> Payload)
        {
            try
            {
                var x = GetFlowIDByTriggerID(SponsorID, FlowTriggerID);
                if (!x.Item2)
                    return Response<string>.SetResponse((string)null, false, x.Item1);

                string FlowID = x.Item1;
                var flow = (await _nOVUContext.GetFlowByID(FlowID, SponsorID)).Item1;
                string errors = "";
                string transactionId = "";
                string finalMessage = "";
                Response<string> result = null;
                //this code was written so that the flow is consisting of one template or more, 
                //after that we considered that it only contains 1 template.
                if (flow != null && flow.Templates != null && flow.Templates.Where(t => t.Type == "sms").Count() != 0)
                {
                    //send the SMSs first
                    result = await TriggerSMSs(flow, SubscriberId, SponsorID, Payload);
                    finalMessage = $"SMS:{errors}";
                }
                else 
                {
                    //trigger the other chanels then
                    transactionId = await _nOVUContext.Trigger(FlowTriggerID, SubscriberId, SponsorID, Payload);
                    finalMessage = $"Flow {transactionId}";
                    result = Response<string>.SetResponse(transactionId, true, $"novu transactionId {transactionId}");
                }
                return result;
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<string>.SetResponse(ex.Message, false, ex.Message);
            }
        }

        /// <summary>Trigger a flow for a subscriber!</summary>
        /// <param name="FlowTriggerID">the ID of the flowtrigger that want to be triggerred</param>
        /// <param name="SubscriberId">the ID of the Subscriber that want to receive the flow (if the record was not exist it will be added, else will be modified)</param>
        /// <param name="FirstName">the subscriber first name, to be created or updated</param>
        /// <param name="LastName">the subscriber last name, to be created or updated</param>
        /// <param name="PhoneNumber">the subscriber phone number, to be created or updated</param>
        /// <param name="Email">the subscriber email, to be created or updated</param>
        /// <param name="Payload">key value pair set of data represent the variables within the flow</param>
        /// <param name="SponsorID">a string represent the sponsor id</param>
        /// <returns>string represents the Transaction ID</returns>
        [HttpPost("TriggerFlowAndCreateOrUpdateSubscriber")]
        public async Task<Response<string>> TriggerFlowAndCreateOrUpdateSubscriber(string FlowTriggerID, string SubscriberId, string FirstName, string LastName, string PhoneNumber, string Email, string SponsorID, Dictionary<string, string> Payload)
        {
            //this code was written so that the flow is consisting of one template or more, 
            //after that we considered that it only contains 1 template.
            try
            {
                var x = GetFlowIDByTriggerID(SponsorID, FlowTriggerID);
                if (!x.Item2)
                    return Response<string>.SetResponse((string)null, false, x.Item1);

                string FlowID = x.Item1;
                var flow = (await _nOVUContext.GetFlowByID(FlowID, SponsorID)).Item1;


                string errors = "";
                string transactionId = "";
                string finalMessage = "";
                Response<string> result = null;

                //this code was written so that the flow is consisting of one template or more, 
                //after that we considered that it only contains 1 template.
                if (flow != null && flow.Templates != null && flow.Templates.Where(t => t.Type == "sms").Count() != 0)
                {
                    //send the SMSs first
                    result = await TriggerSMSs(flow, Payload,PhoneNumber, SponsorID);
                    errors = result.Message;
                    finalMessage = $"SMS:{errors}";
                }
                else
                {
                    //trigger the other chanels then
                    transactionId = await _nOVUContext.Trigger(FlowTriggerID, SubscriberId, FirstName, LastName, PhoneNumber, Email, SponsorID, Payload);
                    finalMessage = $"Flow {transactionId}";
                    result = Response<string>.SetResponse(transactionId, true, $"novu transactionId {transactionId}");
                }
                return result;
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<string>.SetResponse(ex.Message, false,ex.Message);
            }
        }

        [NonAction]
        public async Task<Response<string>> TriggerSMSs(Flow flow, string SubscriberID, string SponsorID, Dictionary<string, string> Payload)
        {
            string results = "";
            var subscriber = (await _nOVUContext.GetSubscriberByID(SubscriberID, SponsorID)).Item1;
            int index = 0;
            Response<string> result = null;
            foreach (var template in flow.Templates)
            {
                if (template.Type == "sms")
                {
                    if (subscriber == null)
                        return Response<string>.SetResponse("",false, "the subscriber is null, it is not exist or there is some kind of a problem!");
                     result = await SendSMS(template, Payload, subscriber);
                    if (!result.Status)
                        results += $"SMS TransactionID {result.Data} Template at {index} in Flow {flow.Id} with phone number {subscriber.PhoneNumber} faced problem- {result.Message}\n";
                    else
                        results += $"SMS TransactionID {result.Data} Template at {index} in Flow {flow.Id} with phone number {subscriber.PhoneNumber} succeeded Status Code 200\n";
                }
                index++;
            }
            string loggingResult = results;
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => {
                if (_loggingEnabled)
                {
                    _logger.Information(loggingResult);
                }
            });
            return result;
        }
        [NonAction]
        public Sponsor GetSponsor(string SponsorID)
        {
            return _sponsors.Where(s => s.SponserID == SponsorID)?.FirstOrDefault();
        }
        [NonAction]
        public Tuple<string, bool> GetFlowIDByTriggerID(string SponsorID, string FlowTriggerID)
        {
            var sponsor = GetSponsor(SponsorID);
            if (sponsor == null)
                return new Tuple<string, bool>("this sponsor is not exist", false);
            //FlowID
            SponserWorkflow sponserWorkflow = sponsor.Workflows.Where(wf => wf.FlowTriggerID == FlowTriggerID).FirstOrDefault();
            if (sponserWorkflow == null)
                return new Tuple<string, bool>("this flow is not exist", false);
            return new Tuple<string, bool>(sponserWorkflow.FlowID, true);
        }
        [NonAction]
        public async Task<Response<string>> TriggerSMSs(Flow flow, Dictionary<string, string> Payload, string PhoneNumber, string SponsorID)
        {
            string results = "";
            int index = 0;
            Response<string> result = null;
            if(flow != null)
            foreach (var template in flow.Templates)
            {
                if (template.Type == "sms")
                {
                    if (PhoneNumber == null)
                        return Response<string>.SetResponse("",false,"the PhoneNumber is null");
                        result = await SendSMS(template, Payload, PhoneNumber);
                        if (!result.Status)
                            results += $"SMS TransactionID {result.Data} Template at {index} in Flow {flow.Id} with phone number {PhoneNumber} faced problem- {result.Message}\n";
                        else
                            results += $"SMS TransactionID {result.Data} Template at {index} in Flow {flow.Id} with phone number {PhoneNumber} succeeded Status Code 200\n";
                }
                index++;
            }
            string loggingResult = results;
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => {
                if (_loggingEnabled)
                {
                    _logger.Information(loggingResult);
                }
            });
            return result;
        }
        [NonAction]
        private async Task<Response<string>> SendSMS(Template template, Dictionary<string, string> payload, Subscriber subscriber)
        {
            string message = template.Content;
            string phoneNumber = subscriber.PhoneNumber;
            message = SetUpMessage(message, payload);
            return await MacrokioskSendMessage(message, phoneNumber);
        }
        [NonAction]
        private async Task<Response<string>> SendSMS(Template template, Dictionary<string, string> payload, string PhoneNumber)
        {
            string message = template.Content;
            string phoneNumber = PhoneNumber;
            message = SetUpMessage(message, payload);
            return await MacrokioskSendMessage(message, phoneNumber);
        }

        [NonAction]
        private async Task<Response<string>> MacrokioskSendMessage(string Message, string PhoneNumber)
        {
            Classes.Macrokiosk macrokiosk = new Classes.Macrokiosk(_configuration);
            return await macrokiosk.SendMessage(Message, PhoneNumber);
        }
        [NonAction]
        private string SetUpMessage(string message, Dictionary<string, string> payload)
        {
            //message = Regex.Unescape(message);
            //message = message.Trim('"');
            message = Regex.Unescape(message);
            if (message.StartsWith("\"") && message.EndsWith("\""))
            {
                message = message.Substring(1, message.Length - 2);
            }

            string[] matches = Regex.Split(message, variablePattern);
            for (int i = 0; i < matches.Length; i++)
            {
                string match = matches[i];
                if (Regex.IsMatch(match, variablePattern))
                {
                    var x1 = payload.Where(p => match.Contains(p.Key));
                    var x2 = x1.FirstOrDefault();
                    matches[i] = x2.Value;
                }
            }

            string ResultMessage = string.Join(" ", matches);

            return ResultMessage;
        }
    }
}
