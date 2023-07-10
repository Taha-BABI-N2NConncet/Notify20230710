using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Notify.Interfaces;
using Notify.Models;
using Notify.Repositories;

namespace Notify.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TemplateController : ControllerBase
    {
        private INOVUContext nOVUContext;
        private Serilog.ILogger _logger;
        private bool _loggingEnabled;
        public TemplateController(INOVUContext nOVUContext, Serilog.ILogger logger, ILoggingEnable loggingEnable)
        {
            this.nOVUContext = nOVUContext;
            _logger = logger;
            _loggingEnabled = loggingEnable.Enabled;
        }

        /// <summary>
        /// insert the Template into the flow that have the passed flowid in NOVU Database
        /// </summary>
        /// <param name="flowId">the id of the flow that you need to insert the template within</param>
        /// <param name="template">the template you wish to insert</param>
        /// <returns>
        /// the new Flow(Notify) after adding the template
        /// </returns>
        //[HttpPost]
        //public async Task<Flow> Post(string flowID,[FromBody] Template template) 
        //{
        //    Tuple<Classes.Notification,string> notificationTuple = await nOVUContext.CreateTemplate(flowID, template);
        //    return new Flow(notificationTuple.Item1);
        //}
        /* please use the following for testing
         * 
         * flowID: 64143b4eab0b7622528464f6
         * template:
         * {
         *    "id": "64143b4dab0b7622528464e6",
         *    "parentID": null,
         *    "type": "sms",
         *    "subject": "",
         *    "senderName": "",
         *    "preheader": "",
         *    "title": "",
         *    "content": "sms template for flow test 2 {{y}}",
         *    "createdAt": "2023-03-17T10:05:01.904Z",
         *    "updatedAt": "2023-03-23T08:15:12.484Z",
         *    "variables": []
         * }
         */


        /// <summary>
        /// update the Template within the flow that have the passed flowid and the template id in NOVU Database
        /// </summary>
        /// <param name="flowId">the id of the flow that you need to update the template within</param>
        /// <param name="template">the update you wish to insert</param>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the new Notification(NOVU) after updating the template
        /// </returns>
        [HttpPut]
        public async Task<Response<Flow>> Put(string flowID, string SponsorID, [FromBody] Template template)
        {
            try
            {
                Tuple<Classes.Notification, string> notificationTuple = await nOVUContext.UpdateTemplate(flowID, template, SponsorID);
                return Response<Flow>.SetSuccess(new Flow(notificationTuple.Item1));
            }
            catch (Exception ex)
            {
                BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
                {
                    if (_loggingEnabled)
                        _logger.Error(ex.Message);

                });

                return Response<Flow>.SetResponse((Flow)null, false, ex.Message);
            }

        }
        /* please use the following for testing
         * 
         * flowID: 64143b4eab0b7622528464f6
         * template:
         * {
         *    "id": "64143b4dab0b7622528464e6",
         *    "parentID": null,
         *    "type": "sms",
         *    "subject": "",
         *    "senderName": "",
         *    "preheader": "",
         *    "title": "",
         *    "content": "sms template for flow test 2 {{y}}",
         *    "createdAt": "2023-03-17T10:05:01.904Z",
         *    "updatedAt": "2023-03-23T08:15:12.484Z",
         *    "variables": []
         * }
         */

        /// <summary>
        /// Get the Template from NOVU Database
        /// </summary>
        /// <param name="SponsorID">
        /// a string represent the sponsor id
        /// </param>
        /// <returns>
        /// the requieded template in Notify Format
        /// </returns>
        [HttpGet("{flowID}/{templateID}")]
        public async Task<Response<Models.Template>> GetTemplate(string flowID, string templateID, string SponsorID)
        {
            try
            {
                var tupleTemplate = await nOVUContext.GetTemplateByID(flowID, templateID, SponsorID);
                return Response<Models.Template>.SetSuccess(tupleTemplate.Item1);
            }
            catch (Exception ex)
            {

                BackgroundQueueLogger.AddLoggingTaskToQueue(() =>
                {
                    if (_loggingEnabled)
                        _logger.Error(ex.Message);
                });
                return Response<Models.Template>.SetResponse((Models.Template)null, false, ex.Message);
            }
        }
        /* please use the following for testing
         * 64143b4eab0b7622528464f6 flow
         * 64143b4dab0b7622528464e6 template
         */
    }
}
