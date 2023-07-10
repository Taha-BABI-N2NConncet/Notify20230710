using Microsoft.AspNetCore.Mvc;
using Notify.Interfaces;
using Notify.Models;
using Notify.Repositories;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Notify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private INOVUContext nOVUContext;
        private IIdentityGenerator identityGenerator;
        private Serilog.ILogger _logger;
        private bool _loggingEnabled;
        public SubscriberController(INOVUContext nOVUContext, IIdentityGenerator identityGenerator, Serilog.ILogger logger, ILoggingEnable loggingEnable)
        {
            this.nOVUContext = nOVUContext;
            this.identityGenerator = identityGenerator;
            _logger = logger;
            _loggingEnabled = loggingEnable.Enabled;
        }
        // GET api/<SubscriberController>
        /// <summary>
        /// Get a list of Subscribers devided by page
        /// </summary>
        /// <param name="page">
        /// is the page number (for pagination)
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// list of Subscriber (Notify)
        /// </returns>
        [HttpGet("GetSubscribersByPage/{page}")]
        public async Task<Response<List<Subscriber>>> GetSubscribersByPage(int page, string SponsorID)
        {
            try
            {
                var SubscribersTuple = await nOVUContext.GetSubscribersByPage(page, SponsorID);
                return Response<List<Subscriber>>.SetSuccess(SubscribersTuple.Item1.ToList());
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<List<Subscriber>>.SetResponse((List<Subscriber>)null, false, ex.Message);
            }
        }

        // GET api/<SubscriberController>
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
        [HttpGet("GetSubscriberByID/{id}")]
        public async Task<Response<Subscriber>> GetSubscriberByID(string id, string SponsorID)
        {
            try
            {

                var SubscriberTuple = await nOVUContext.GetSubscriberByID(id, SponsorID);
                return Response<Subscriber>.SetSuccess(SubscriberTuple.Item1);
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<Subscriber>.SetResponse((Subscriber)null, false, ex.Message);
            }
        }

        // POST api/<SubscriberController>
        /// <summary>
        /// insert subscriber (Notify) information to NOVU database in NOVU Format
        /// and add that object to the local database in Notify Format
        /// </summary>
        /// <param name="subscriber">
        /// the requiered Subscriber to be added 
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the inserted subscriber in Notify Format
        /// </returns>
        [HttpPost]
        public async Task<Models.Response<Subscriber>> Post(Models.Subscriber subscriber, string SponsorID)
        {
            try
            {
                Tuple<Classes.Subscriber, string> result = await nOVUContext.CreateSubscriber(subscriber, SponsorID);
                if (result.Item1 != null)
                {
                    //this means that the Subscriber(converted to NOVU Subscriber) have been added to the nouv site
                    //so we must add it to our local database

                    //Convert Subscriber(NOVU) to Subscriber(Notify)
                    Models.Subscriber Subscriber1 = new Models.Subscriber(result.Item1);
                    return Models.Response<Subscriber>.SetSuccess(Subscriber1);
                }
                else
                {
                    //null means that there is some thing wrong please refer to the 
                    //string message so that you can get the error message!
                    return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, "");

                }
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, ex.Message);
            }
        }
        #region JSON Flow Example
        /*please refer to the comment bellow
         {
           "firstName": "Mhammad",
           "lastName": "Salem",
           "phoneNumber": "+963964560869",
           "email": "test@test.com",
           "messagesBalance": 1000,
           "deviceTokens": 
           [
             {
               "tokenString": "test123456testdevicetoken"
             }
           ]
         }
         
         */
        #endregion


        /// <summary>
        /// update subscriber (Notify) information to NOVU database in NOVU Format
        /// </summary>
        /// <param name="subscriber">
        /// the requiered Subscriber to be added 
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the Updated Subscriber in Notify Format
        /// </returns>
        [HttpPut]
        public async Task<Models.Response<Subscriber>> Put(Subscriber subscriber, string SponsorID)
        {
            try
            {
                Tuple<Classes.Subscriber, string> result = await nOVUContext.UpdateSubscriber(subscriber, SponsorID);
                if (result.Item1 != null)
                {
                    //this means that the Subscriber(converted to NOVU Subscriber) have been updated to the nouv site
                    //Convert Subscriber(NOVU) to Subscriber(Notify)
                    Models.Subscriber Subscriber1 = new Models.Subscriber(result.Item1);
                    return Models.Response<Subscriber>.SetSuccess(Subscriber1);
                }
                else
                {
                    //null means that there is some thing wrong please refer to the 
                    //string message so that you can get the error message!
                    return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, "");

                }
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, ex.Message);
            }
        }

        /// <summary>
        /// Set device tokens for subscriber 
        /// </summary>
        /// <param name="SubscriberID">Subscriber id</param>
        /// <param name="DeviceTokens">List of Subscriber Device ID's</param>
        /// <param name="providerId">the provider id (in default its fcm)</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>the subscriber after update</returns>
        [HttpPut("SetSubscriberDeviceToken/{SubscriberID}")]
        public async Task<Models.Response<Subscriber>> SetSubscriberDeviceToken(string SubscriberID, List<string> DeviceTokens, string SponsorID, string providerId = "fcm")
        {
            try
            {
                List<DeviceToken> DeviceTokensList = DeviceTokens.Select(dt => new DeviceToken() { SbuscriberID = SubscriberID, TokenString = dt }).ToList();
                var channel = new Classes.Channel() { providerId = providerId, credentials = new Classes.Credentials(DeviceTokensList) };

                Tuple<Classes.Subscriber, string> result = await nOVUContext.SetSubscriberDeviceToken(SubscriberID, channel, SponsorID);
                if (result.Item1 != null)
                {
                    return Models.Response<Subscriber>.SetSuccess(new Subscriber(result.Item1));
                }
                return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, "");
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Models.Response<Subscriber>.SetResponse((Subscriber)null, false, ex.Message);
            }

        }

        /// <summary>
        /// Delete the Subscriber with the specified subscriberID from NOVU Database
        /// </summary>
        /// <param name="subscriberId">the subscriber id of the subscriber that need to be deleted</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        [HttpDelete]
        public async Task<Response<Subscriber>> Delete(string subscriberId, string SponsorID)
        {
            try
            {
                var subscriber = (await GetSubscriberByID(subscriberId, SponsorID)).Data;
                if (subscriber == null)
                    return Response<Subscriber>.SetResponse((Subscriber)null, false, "there is no subscriber with this subscriberid in the database");


                Tuple<bool, string> result = await nOVUContext.DeleteSubscriber(subscriberId, SponsorID);
                return Response<Subscriber>.SetResponse(subscriber, result.Item1, result.Item2);
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<Subscriber>.SetResponse((Subscriber)null, false, ex.Message);
            }

        }
    }
}