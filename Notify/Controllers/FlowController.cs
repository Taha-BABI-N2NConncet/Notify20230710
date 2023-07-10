using Encryption;
using Microsoft.AspNetCore.Mvc;
using Notify.Interfaces;
using Notify.Models;
using Notify.Repositories;
using Notify.Repositories.NOVUSettingsClasses;
using System.Collections.Generic;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Notify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FlowController : ControllerBase
    {
        private INOVUContext nOVUContext;
        private Serilog.ILogger _logger;
        private bool _loggingEnabled;
        public FlowController(INOVUContext nOVUContext, Serilog.ILogger logger, ILoggingEnable loggingEnable)
        {
            this.nOVUContext = nOVUContext;
            _logger = logger;
            _loggingEnabled = loggingEnable.Enabled;
        }

        // GET: api/<FlowController>
        /// <summary>
        /// Getting All the flows by page  
        /// page size is 10
        /// </summary>
        /// <param name="page">
        /// the page number for pagination
        /// </param>
        /// <returns>
        /// List Of Flows
        /// </returns>
        [HttpGet("GetFlowsByPage/{page}")]
        public async Task<Response<List<Flow>>> GetFlowsByPage(int page, string SponsorID)
        {
            try
            {
                var FlowsTuple = await nOVUContext.GetFlowsByPage(page, SponsorID);
                return Response<List<Flow>>.SetSuccess(FlowsTuple.Item1.ToList());
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<List<Flow>>.SetResponse((List<Flow>)null, false, ex.Message);
            }
        }


        // GET api/<FlowController>
        /// <summary>
        /// Getting Flow by ID
        /// </summary>
        /// <param name="id">
        /// is the Flow id in NOVU Database
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the flow with the specified id
        /// </returns>
        [HttpGet("GetFlowByID/{id}")]
        public async Task<Response<Flow>> GetFlowByID(string id, string SponsorID)
        {
            try
            {
                var FlowTuple = await nOVUContext.GetFlowByID(id, SponsorID);
                return Response<Flow>.SetSuccess(FlowTuple.Item1);
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<Flow>.SetResponse((Flow)null, false, ex.Message);
            }

        }

        //[HttpGet("enchript")]
        //public string enchript(int x, int y, int z)
        //{
        //    return AES.ATPEncrypt("1310af94eafa29d62bbeb697b66397e4");
        //}


        // POST api/<FlowController> {Create Notification}
        /// <summary>
        /// this action is for creating only three level of objects 
        /// 1- Flow
        /// 2- Template
        /// 3- Variable
        /// </summary>
        /// <param name="flow">
        /// is the object that you wish to insert
        /// </param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the same object after being inserted into NOVU Database
        /// filled with all the autofilled data like id's 
        /// </returns>
        [HttpPost]
        public async Task<Response<Flow>> Post([FromBody] Flow flow, string SponsorID)
        {
            Tuple<Classes.Notification, string> result;
            try
            {
                result = await nOVUContext.CreateFlow(flow, SponsorID);
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() => { if (_loggingEnabled) _logger.Error(ex.Message); });
                return Response<Flow>.SetResponse((Flow)null, false, ex.Message);
            }
            if (result.Item1 != null)
            {
                //this means that the Flow(converted to NOVU Notification) have been added to the nouv site
                //so we must add it to our local database

                //Convert Notification(NOVU) to Flow(Notify)
                Flow flow1 = new Flow(result.Item1);

                return Response<Flow>.SetSuccess(flow1);
            }
            //null means that there is some thing wrong please refer to the 
            //string message so that you can get the error message!
            string message = "Create Flow faced a problem please refer to it " + result.Item2;
            
            BackgroundQueueLogger.AddLoggingTaskToQueue(() => {
                if (_loggingEnabled)
                    _logger.Error(message);
            });
            return Response<Flow>.SetResponse((Flow)null, false, message);
        }
        #region JSON Flow Example
        /* please refer to the comment bellow
         * {
             "id": "",
             "name": "flow test 1",
             "notificationGroupId": "",
             "description": "flow test 1 description",
             "templates": [
               {
                   "id": "",
                   "type": "sms",
                   "subject": "",
                   "senderName": "",
                   "preheader": "",
                   "title": "",
                   "content": "sms template for flow test 1 {{x}}",
                   "createdAt": "2023-03-17T08:51:37.603Z",
                   "updatedAt": "2023-03-17T08:51:37.603Z",
                   "variables": [
                     {
                       "id": "",
                       "name": "x",
                       "type": "string",
                       "requiered": true,
                       "templateId": ""
                     }
                   ]
                 },
               {
                 "id": "",
                 "type": "sms",
                 "subject": "",
                 "senderName": "",
                 "preheader": "",
                 "title": "",
                 "content": "sms template 2 for flow test 1 {{y}}",
                 "createdAt": "2023-03-17T08:51:37.603Z",
                 "updatedAt": "2023-03-17T08:51:37.603Z",
                 "variables": [
                   {
                     "id": "",
                     "name": "y",
                     "type": "string",
                     "requiered": true,
                     "templateId": ""
                   }
                 ]
               }
             ]
           }
         */
        #endregion
    }
}
